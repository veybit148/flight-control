using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Domain.Exceptions;
using FlightControl.Domain.Mappings;
using FlightControl.Domain.Security;
using FlightControl.Dto.Fleets;
using FlightControl.Repository.Interfaces;
using FlightControl.Validation.Core;

namespace FlightControl.Domain.Operations.Fleets;

public class AircraftOperation : IAircraftOperation
{
    private const string AircraftNotFoundMessage = "Aircraft not found.";
    private const string FleetNotFoundMessage = "Fleet not found.";

    private readonly AbstractValidator<CreateAircraftRequest> _createValidator;
    private readonly AbstractValidator<UpdateAircraftRequest> _updateValidator;
    private readonly AbstractValidator<ChangeAircraftStatusRequest> _statusValidator;
    private readonly AbstractValidator<AircraftQuery> _queryValidator;
    private readonly IAircraftRepository _aircraftRepository;
    private readonly IAircraftStatusChangeRepository _statusChangeRepository;
    private readonly IFleetRepository _fleetRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public AircraftOperation(
        AbstractValidator<CreateAircraftRequest> createValidator,
        AbstractValidator<UpdateAircraftRequest> updateValidator,
        AbstractValidator<ChangeAircraftStatusRequest> statusValidator,
        AbstractValidator<AircraftQuery> queryValidator,
        IAircraftRepository aircraftRepository,
        IAircraftStatusChangeRepository statusChangeRepository,
        IFleetRepository fleetRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _statusValidator = statusValidator;
        _queryValidator = queryValidator;
        _aircraftRepository = aircraftRepository;
        _statusChangeRepository = statusChangeRepository;
        _fleetRepository = fleetRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<AircraftResponse> CreateAsync(CreateAircraftRequest request)
    {
        _createValidator.ThrowIfInvalid(request);

        CreateAircraftRequest normalized = request with
        {
            RegistrationNumber = NormalizeRegistration(request.RegistrationNumber)
        };

        if (await _aircraftRepository.ExistsByRegistrationNumberAsync(normalized.RegistrationNumber))
        {
            throw new ConflictException("This registration number is already in use.");
        }

        await EnsureFleetBelongsToCurrentUserAsync(normalized.FleetId);

        Aircraft aircraft = normalized.ToEntity(CurrentUserId);

        _aircraftRepository.Add(aircraft);
        await _unitOfWork.SaveChangesAsync();

        return aircraft.ToResponse(await GetOwnerNameAsync(aircraft.OwnerId));
    }

    public async Task<IReadOnlyList<AircraftResponse>> ListAsync(AircraftQuery query)
    {
        _queryValidator.ThrowIfInvalid(query);

        Guid? ownerId = IsAuthority ? null : CurrentUserId;
        AircraftStatus? status = query.Status?.ToEnum<AircraftStatus>();
        AircraftCategory? category = query.Category?.ToEnum<AircraftCategory>();

        IReadOnlyList<Aircraft> aircraft = await _aircraftRepository
            .GetListByFiltersAsync(ownerId, query.FleetId, status, category);

        if (aircraft.Count == 0)
        {
            return [];
        }

        List<Guid> ownerIds = aircraft
            .Select(a => a.OwnerId)
            .Distinct()
            .ToList();

        IReadOnlyList<User> owners = await _userRepository.GetByIdsAsync(ownerIds);

        Dictionary<Guid, string> ownerNames = owners.ToDictionary(u => u.Id, u => u.FullName);

        return aircraft
            .Select(a => a.ToResponse(ownerNames[a.OwnerId]))
            .ToList();
    }

    public async Task<AircraftResponse> GetByIdAsync(Guid id)
    {
        Aircraft aircraft = await GetAccessibleAircraftAsync(id, allowAuthority: true);
        return aircraft.ToResponse(await GetOwnerNameAsync(aircraft.OwnerId));
    }

    public async Task<AircraftResponse> UpdateAsync(Guid id, UpdateAircraftRequest request)
    {
        _updateValidator.ThrowIfInvalid(request);

        Aircraft aircraft = await GetAccessibleAircraftAsync(id, allowAuthority: false);

        UpdateAircraftRequest normalized = request with
        {
            RegistrationNumber = NormalizeRegistration(request.RegistrationNumber)
        };

        if (normalized.RegistrationNumber != aircraft.RegistrationNumber &&
            await _aircraftRepository.ExistsByRegistrationNumberAsync(normalized.RegistrationNumber))
        {
            throw new ConflictException("This registration number is already in use.");
        }

        if (normalized.FleetId != aircraft.FleetId)
        {
            await EnsureFleetBelongsToCurrentUserAsync(normalized.FleetId);
        }

        normalized.ApplyTo(aircraft);
        await _unitOfWork.SaveChangesAsync();

        return aircraft.ToResponse(await GetOwnerNameAsync(aircraft.OwnerId));
    }

    public async Task<AircraftResponse> ChangeStatusAsync(Guid id, ChangeAircraftStatusRequest request)
    {
        _statusValidator.ThrowIfInvalid(request);

        Aircraft aircraft = await GetAccessibleAircraftAsync(id, allowAuthority: true);
        AircraftStatus newStatus = request.Status.ToEnum<AircraftStatus>();

        if (newStatus == aircraft.Status)
        {
            throw new ConflictException($"The aircraft is already {newStatus}.");
        }

        if (!CanChangeStatus(aircraft.Status, newStatus, IsAuthority))
        {
            throw new ForbiddenException("You are not allowed to make this status change.");
        }

        _statusChangeRepository.Add(new AircraftStatusChange
        {
            AircraftId = aircraft.Id,
            FromStatus = aircraft.Status,
            ToStatus = newStatus,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            ChangedById = CurrentUserId
        });

        aircraft.Status = newStatus;

        await _unitOfWork.SaveChangesAsync();

        return aircraft.ToResponse(await GetOwnerNameAsync(aircraft.OwnerId));
    }

    public async Task DeleteAsync(Guid id)
    {
        Aircraft aircraft = await GetAccessibleAircraftAsync(id, allowAuthority: false);

        _aircraftRepository.Delete(aircraft);
        await _unitOfWork.SaveChangesAsync();
    }

    // Helpers

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new InvalidOperationException("AircraftOperation requires an authenticated user.");

    private UserRole CurrentRole => _currentUser.Role
        ?? throw new InvalidOperationException("AircraftOperation requires an authenticated user.");

    private bool IsAuthority => CurrentRole == UserRole.Authority;

    private static string NormalizeRegistration(string value) => value.Trim().ToUpperInvariant();

    // Returns the aircraft if the current user may access it; otherwise behaves as if it does not exist.
    private async Task<Aircraft> GetAccessibleAircraftAsync(Guid id, bool allowAuthority)
    {
        Aircraft? aircraft = await _aircraftRepository.GetByIdAsync(id);

        bool canAccess = aircraft != null &&
            (aircraft.OwnerId == CurrentUserId || (allowAuthority && IsAuthority));

        if (!canAccess)
        {
            throw new NotFoundException(AircraftNotFoundMessage);
        }

        return aircraft!;
    }

    // A fleet can be set only by its owner, and only fleet owners or military users own fleets.
    private async Task EnsureFleetBelongsToCurrentUserAsync(Guid? fleetId)
    {
        if (fleetId == null)
        {
            return;
        }

        bool canOwnFleets = CurrentRole is UserRole.FleetOwner or UserRole.Military;
        Fleet? fleet = canOwnFleets ? await _fleetRepository.GetByIdAsync(fleetId.Value) : null;

        if (fleet == null || fleet.OwnerId != CurrentUserId)
        {
            throw new NotFoundException(FleetNotFoundMessage);
        }
    }

    private async Task<string> GetOwnerNameAsync(Guid ownerId)
    {
        // The owner always exists: users with aircraft cannot be deleted (restrict).
        User owner = await _userRepository.GetByIdAsync(ownerId)
            ?? throw new InvalidOperationException($"Owner {ownerId} of an aircraft was not found.");

        return owner.FullName;
    }

     // Owners manage maintenance; only the authority can ground an aircraft or lift a grounding.
    private static bool CanChangeStatus(AircraftStatus from, AircraftStatus to, bool isAuthority) =>
        isAuthority || (from, to) is
            (AircraftStatus.Active, AircraftStatus.Maintenance) or
            (AircraftStatus.Maintenance, AircraftStatus.Active);   
}