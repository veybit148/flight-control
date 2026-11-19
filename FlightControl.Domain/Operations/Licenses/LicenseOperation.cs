using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Domain.Exceptions;
using FlightControl.Domain.Mappings;
using FlightControl.Domain.Security;
using FlightControl.Dto.Licenses;
using FlightControl.Repository.Interfaces;
using FlightControl.Validation.Core;

namespace FlightControl.Domain.Operations.Licenses;

public class LicenseOperation : ILicenseOperation
{
    private const string LicenseNotFoundMessage = "License not found.";

    private readonly AbstractValidator<CreateLicenseRequest> _createValidator;
    private readonly AbstractValidator<UpdateLicenseRequest> _updateValidator;
    private readonly AbstractValidator<SuspendLicenseRequest> _suspendValidator;
    private readonly IPilotLicenseRepository _licenseRepository;
    private readonly ILicenseCategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFlightPermitRequestRepository _permitRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public LicenseOperation(
        AbstractValidator<CreateLicenseRequest> createValidator,
        AbstractValidator<UpdateLicenseRequest> updateValidator,
        AbstractValidator<SuspendLicenseRequest> suspendValidator,
        IPilotLicenseRepository licenseRepository,
        ILicenseCategoryRepository categoryRepository,
        IUserRepository userRepository,
        IFlightPermitRequestRepository permitRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _suspendValidator = suspendValidator;
        _licenseRepository = licenseRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _permitRepository = permitRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<LicenseResponse> CreateAsync(CreateLicenseRequest request)
    {
        _createValidator.ThrowIfInvalid(request);

        CreateLicenseRequest normalized = request with
        {
            LicenseNumber = request.LicenseNumber.Trim().ToUpperInvariant()
        };

        if (await _licenseRepository.ExistsByLicenseNumberAsync(normalized.LicenseNumber))
        {
            throw new ConflictException("This license number is already in use.");
        }

        PilotLicense license = normalized.ToEntity(CurrentUserId);
        _licenseRepository.Add(license);

        List<AircraftCategory> categories = ParseCategories(normalized.Categories);
        foreach (AircraftCategory category in categories)
        {
            _categoryRepository.Add(new LicenseCategory { LicenseId = license.Id, Category = category });
        }

        await _unitOfWork.SaveChangesAsync();

        return license.ToResponse(await GetPilotNameAsync(license.PilotId), categories);
    }

    public async Task<IReadOnlyList<LicenseResponse>> ListAsync()
    {
        Guid? pilotId = IsAuthority ? null : CurrentUserId;

        IReadOnlyList<PilotLicense> licenses = await _licenseRepository.GetListByPilotIdAsync(pilotId);

        if (licenses.Count == 0)
        {
            return [];
        }

        IReadOnlyList<LicenseCategory> categories =
            await _categoryRepository.GetByLicenseIdsAsync(licenses.Select(l => l.Id).ToList());

        ILookup<Guid, AircraftCategory> categoriesByLicense =
            categories.ToLookup(c => c.LicenseId, c => c.Category);

        IReadOnlyList<User> pilots =
            await _userRepository.GetByIdsAsync(licenses.Select(l => l.PilotId).Distinct().ToList());

        Dictionary<Guid, string> pilotNames = pilots.ToDictionary(u => u.Id, u => u.FullName);

        return licenses
            .Select(l => l.ToResponse(pilotNames[l.PilotId], categoriesByLicense[l.Id]))
            .ToList();
    }

    public async Task<LicenseResponse> GetByIdAsync(Guid id)
    {
        PilotLicense license = await GetAccessibleLicenseAsync(id, allowAuthority: true);
        return await BuildResponseAsync(license);
    }

    public async Task<LicenseResponse> UpdateAsync(Guid id, UpdateLicenseRequest request)
    {
        _updateValidator.ThrowIfInvalid(request);

        PilotLicense license = await GetAccessibleLicenseAsync(id, allowAuthority: false);
        license.ExpiresAt = request.ExpiresAt;

        HashSet<AircraftCategory> requested = ParseCategories(request.Categories).ToHashSet();
        IReadOnlyList<LicenseCategory> existing = await _categoryRepository.GetByLicenseIdsAsync([license.Id]);

        foreach (LicenseCategory row in existing.Where(c => !requested.Contains(c.Category)))
        {
            _categoryRepository.Delete(row);
        }

        foreach (AircraftCategory category in requested.Except(existing.Select(c => c.Category)))
        {
            _categoryRepository.Add(new LicenseCategory { LicenseId = license.Id, Category = category });
        }

        await _unitOfWork.SaveChangesAsync();

        return license.ToResponse(await GetPilotNameAsync(license.PilotId), requested);
    }

    public async Task DeleteAsync(Guid id)
    {
        PilotLicense license = await GetAccessibleLicenseAsync(id, allowAuthority: false);

        // Permit requests reference the license (restrict) and are never deleted.
        if (await _permitRepository.ExistsByLicenseIdAsync(license.Id))
        {
            throw new ConflictException("This license has permit requests and can't be deleted.");
        }

        _licenseRepository.Delete(license);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<LicenseResponse> SuspendAsync(Guid id, SuspendLicenseRequest request)
    {
        _suspendValidator.ThrowIfInvalid(request);

        PilotLicense license = await _licenseRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(LicenseNotFoundMessage);

        if (license.Status == LicenseStatus.Suspended)
        {
            throw new ConflictException("The license is already suspended.");
        }

        license.Status = LicenseStatus.Suspended;
        license.SuspensionReason = request.Reason.Trim();

        await _unitOfWork.SaveChangesAsync();

        return await BuildResponseAsync(license);
    }

    public async Task<LicenseResponse> ReinstateAsync(Guid id)
    {
        PilotLicense license = await _licenseRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(LicenseNotFoundMessage);

        if (license.Status == LicenseStatus.Valid)
        {
            throw new ConflictException("The license is already valid.");
        }

        license.Status = LicenseStatus.Valid;
        license.SuspensionReason = null;

        await _unitOfWork.SaveChangesAsync();

        return await BuildResponseAsync(license);
    }

    // Helpers

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new InvalidOperationException("LicenseOperation requires an authenticated user.");

    private UserRole CurrentRole => _currentUser.Role
        ?? throw new InvalidOperationException("LicenseOperation requires an authenticated user.");

    private bool IsAuthority => CurrentRole == UserRole.Authority;

    private static List<AircraftCategory> ParseCategories(IEnumerable<string> values) =>
        values.Select(v => v.ToEnum<AircraftCategory>()).Distinct().ToList();

    // Returns the license if the current user may access it; otherwise behaves as if it does not exist.
    private async Task<PilotLicense> GetAccessibleLicenseAsync(Guid id, bool allowAuthority)
    {
        PilotLicense? license = await _licenseRepository.GetByIdAsync(id);

        bool canAccess = license != null &&
            (license.PilotId == CurrentUserId || (allowAuthority && IsAuthority));

        if (!canAccess)
        {
            throw new NotFoundException(LicenseNotFoundMessage);
        }

        return license!;
    }

    private async Task<LicenseResponse> BuildResponseAsync(PilotLicense license)
    {
        IReadOnlyList<LicenseCategory> categories = await _categoryRepository.GetByLicenseIdsAsync([license.Id]);
        return license.ToResponse(await GetPilotNameAsync(license.PilotId), categories.Select(c => c.Category));
    }

    private async Task<string> GetPilotNameAsync(Guid pilotId)
    {
        // The pilot always exists: users with licenses cannot be deleted (restrict).
        User pilot = await _userRepository.GetByIdAsync(pilotId)
            ?? throw new InvalidOperationException($"Pilot {pilotId} of a license was not found.");

        return pilot.FullName;
    }
}