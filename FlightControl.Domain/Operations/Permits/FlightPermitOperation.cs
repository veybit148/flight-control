using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Dal.Spatial;
using FlightControl.Domain.Exceptions;
using FlightControl.Domain.Mappings;
using FlightControl.Domain.Rules;
using FlightControl.Domain.Security;
using FlightControl.Dto.Common;
using FlightControl.Dto.Permits;
using FlightControl.Repository.Filters;
using FlightControl.Repository.Interfaces;
using FlightControl.Validation.Core;

namespace FlightControl.Domain.Operations.Permits;

public class FlightPermitOperation : IFlightPermitOperation
{
    private const string PermitNotFoundMessage = "Permit request not found.";
    private const string AircraftNotFoundMessage = "Aircraft not found.";
    private const string LicenseNotFoundMessage = "License not found.";

    private readonly AbstractValidator<CreatePermitRequest> _createValidator;
    private readonly AbstractValidator<PermitQuery> _queryValidator;
    private readonly AbstractValidator<ApprovePermitRequest> _approveValidator;
    private readonly AbstractValidator<RejectPermitRequest> _rejectValidator;
    private readonly IFlightPermitRequestRepository _permitRepository;
    private readonly IAircraftRepository _aircraftRepository;
    private readonly IPilotLicenseRepository _licenseRepository;
    private readonly IFleetMembershipRepository _membershipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IEnumerable<IPermitRule> _rules;
    private readonly IAirspaceZoneRepository _zoneRepository;
    private readonly ILicenseCategoryRepository _categoryRepository;
    private readonly IPermitCheckResultRepository _checkResultRepository;

    public FlightPermitOperation(
        AbstractValidator<CreatePermitRequest> createValidator,
        AbstractValidator<PermitQuery> queryValidator,
        AbstractValidator<ApprovePermitRequest> approveValidator,
        AbstractValidator<RejectPermitRequest> rejectValidator,
        IFlightPermitRequestRepository permitRepository,
        IAircraftRepository aircraftRepository,
        IPilotLicenseRepository licenseRepository,
        IFleetMembershipRepository membershipRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IEnumerable<IPermitRule> rules,
        IAirspaceZoneRepository zoneRepository,
        ILicenseCategoryRepository categoryRepository,
        IPermitCheckResultRepository checkResultRepository)
    {
        _createValidator = createValidator;
        _queryValidator = queryValidator;
        _approveValidator = approveValidator;
        _rejectValidator = rejectValidator;
        _permitRepository = permitRepository;
        _aircraftRepository = aircraftRepository;
        _licenseRepository = licenseRepository;
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _rules = rules;
        _zoneRepository = zoneRepository;
        _categoryRepository = categoryRepository;
        _checkResultRepository = checkResultRepository;
    }

    public async Task<PermitResponse> CreateAsync(CreatePermitRequest request)
    {
        _createValidator.ThrowIfInvalid(request);

        PermitRequestType type = CurrentRole switch
        {
            UserRole.Pilot => PermitRequestType.Individual,
            UserRole.FleetOwner => PermitRequestType.Fleet,
            UserRole.Military => PermitRequestType.Military,
            _ => throw new ForbiddenException("Only pilots, fleet owners and military users can request permits.")
        };

        Aircraft aircraft = await _aircraftRepository.GetByIdAsync(request.AircraftId)
            ?? throw new NotFoundException(AircraftNotFoundMessage);

        if (aircraft.OwnerId != CurrentUserId)
        {
            throw new NotFoundException(AircraftNotFoundMessage);
        }

        PilotLicense license = await _licenseRepository.GetByIdAsync(request.LicenseId)
            ?? throw new NotFoundException(LicenseNotFoundMessage);

        if (type == PermitRequestType.Individual)
        {
            if (license.PilotId != CurrentUserId)
            {
                throw new NotFoundException(LicenseNotFoundMessage);
            }
        }
        else
        {
            if (aircraft.FleetId is not Guid fleetId)
            {
                throw new ConflictException("Fleet requests need an aircraft assigned to one of your fleets.");
            }

            if (!await _membershipRepository.ExistsAtFleetAsync(fleetId, license.PilotId))
            {
                throw new NotFoundException(LicenseNotFoundMessage);
            }
        }

        FlightPermitRequest permit = new()
        {
            RequesterId = CurrentUserId,
            Type = type,
            AircraftId = aircraft.Id,
            LicenseId = license.Id,
            PilotId = license.PilotId,
            FleetId = type == PermitRequestType.Individual ? null : aircraft.FleetId,
            Area = Geo.CreatePolygon(request.Area.Select(p => (p.Latitude, p.Longitude))),
            PlannedStart = request.PlannedStart,
            PlannedEnd = request.PlannedEnd,
            MaxAltitudeMeters = request.MaxAltitudeMeters,
            Purpose = request.Purpose.Trim()
        };

        _permitRepository.Add(permit);
        List<PermitCheckResult> checks = await RunChecksAsync(permit, aircraft, license);
        await _unitOfWork.SaveChangesAsync();

        return await BuildResponseAsync(permit, aircraft, license, checks);
    }

    public async Task<PagedResponse<PermitResponse>> ListAsync(PermitQuery query)
    {
        _queryValidator.ThrowIfInvalid(query);

        PermitFilter filter = new(
            RequesterId: IsAuthority ? null : CurrentUserId,
            Status: query.Status?.ToEnum<PermitStatus>(),
            Type: query.Type?.ToEnum<PermitRequestType>(),
            From: query.From,
            To: query.To,
            Page: query.Page,
            PageSize: query.PageSize,
            MilitaryFirst: string.Equals(query.Sort, "militaryFirst", StringComparison.OrdinalIgnoreCase));

        var (permits, totalCount) = await _permitRepository.GetPagedAsync(filter);

        if (permits.Count == 0)
        {
            return new PagedResponse<PermitResponse>([], query.Page, query.PageSize, totalCount);
        }

        Dictionary<Guid, Aircraft> aircraftById =
            (await _aircraftRepository.GetByIdsAsync(permits.Select(p => p.AircraftId).Distinct().ToList()))
            .ToDictionary(a => a.Id);

        Dictionary<Guid, PilotLicense> licensesById =
            (await _licenseRepository.GetByIdsAsync(permits.Select(p => p.LicenseId).Distinct().ToList()))
            .ToDictionary(l => l.Id);

        List<Guid> userIds = permits.Select(p => p.RequesterId)
            .Concat(permits.Select(p => p.PilotId))
            .Distinct()
            .ToList();

        Dictionary<Guid, string> namesById =
            (await _userRepository.GetByIdsAsync(userIds)).ToDictionary(u => u.Id, u => u.FullName);

        ILookup<Guid, PermitCheckResult> checksByPermit =
            (await _checkResultRepository.GetByPermitIdsAsync(permits.Select(p => p.Id).ToList()))
            .ToLookup(c => c.PermitRequestId);

        List<PermitResponse> items = permits
            .Select(p => p.ToResponse(
                namesById[p.RequesterId],
                aircraftById[p.AircraftId],
                licensesById[p.LicenseId],
                namesById[p.PilotId],
                checksByPermit[p.Id]))
            .ToList();

        return new PagedResponse<PermitResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<PermitResponse> GetByIdAsync(Guid id)
    {
        FlightPermitRequest permit = await GetAccessiblePermitAsync(id, allowAuthority: true);
        return await BuildResponseAsync(permit);
    }

    public async Task<PermitResponse> CancelAsync(Guid id)
    {
        FlightPermitRequest permit = await GetAccessiblePermitAsync(id, allowAuthority: false);

        permit.Cancel(DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();

        return await BuildResponseAsync(permit);
    }

    public async Task<PermitResponse> ApproveAsync(Guid id, ApprovePermitRequest request)
    {
        _approveValidator.ThrowIfInvalid(request);

        FlightPermitRequest permit = await _permitRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(PermitNotFoundMessage);

        // Checked before re-running the rules, so a decided request keeps the report it was decided with.
        permit.EnsurePending("approved");

        Aircraft aircraft = await _aircraftRepository.GetByIdAsync(permit.AircraftId)
            ?? throw new InvalidOperationException($"Aircraft {permit.AircraftId} of a request was not found.");

        PilotLicense license = await _licenseRepository.GetByIdAsync(permit.LicenseId)
            ?? throw new InvalidOperationException($"License {permit.LicenseId} of a request was not found.");

        List<PermitCheckResult> checks = await RunChecksAsync(permit, aircraft, license);

        if (checks.Any(c => c.Outcome == CheckOutcome.Failed))
        {
            await _unitOfWork.SaveChangesAsync();
            throw new ConflictException("The request fails one or more pre-checks and can't be approved.");
        }

        if (checks.Any(c => c.Outcome == CheckOutcome.Warning) && string.IsNullOrWhiteSpace(request.Note))
        {
            await _unitOfWork.SaveChangesAsync();

            ValidationResult noteRequired = new();
            noteRequired.AddError(nameof(request.Note), "The request has warnings; a note is required to approve it.");
            throw new ValidationException(noteRequired);
        }

        permit.Approve(CurrentUserId, DateTime.UtcNow, request.Note?.Trim());
        await _unitOfWork.SaveChangesAsync();

        return await BuildResponseAsync(permit, aircraft, license, checks);
    }

    public async Task<PermitResponse> RejectAsync(Guid id, RejectPermitRequest request)
    {
        _rejectValidator.ThrowIfInvalid(request);

        FlightPermitRequest permit = await _permitRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(PermitNotFoundMessage);

        permit.Reject(CurrentUserId, DateTime.UtcNow, request.Reason.Trim());
        await _unitOfWork.SaveChangesAsync();

        return await BuildResponseAsync(permit);
    }

    // Helpers

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new InvalidOperationException("FlightPermitOperation requires an authenticated user.");

    private UserRole CurrentRole => _currentUser.Role
        ?? throw new InvalidOperationException("FlightPermitOperation requires an authenticated user.");

    private bool IsAuthority => CurrentRole == UserRole.Authority;

    // Returns the request if the current user may access it; otherwise behaves as if it does not exist.
    private async Task<FlightPermitRequest> GetAccessiblePermitAsync(Guid id, bool allowAuthority)
    {
        FlightPermitRequest? permit = await _permitRepository.GetByIdAsync(id);

        bool canAccess = permit != null &&
            (permit.RequesterId == CurrentUserId || (allowAuthority && IsAuthority));

        if (!canAccess)
        {
            throw new NotFoundException(PermitNotFoundMessage);
        }

        return permit!;
    }

    // Runs every registered rule against fresh data and replaces the stored report. Does not save.
    private async Task<List<PermitCheckResult>> RunChecksAsync(
        FlightPermitRequest permit, Aircraft aircraft, PilotLicense license)
    {
        IReadOnlyList<LicenseCategory> categories = await _categoryRepository.GetByLicenseIdsAsync([license.Id]);

        PermitCheckContext context = new(
            permit,
            aircraft,
            license,
            categories.Select(c => c.Category).ToList(),
            await _zoneRepository.GetActiveIntersectingAsync(permit.Area, permit.PlannedStart, permit.PlannedEnd),
            await _permitRepository.GetApprovedOverlappingAsync(permit.Id, permit.PlannedStart, permit.PlannedEnd),
            await _permitRepository.GetApprovedOverlappingAsync(permit.Id, permit.PlannedStart, permit.PlannedEnd, permit.Area));

        foreach (PermitCheckResult old in await _checkResultRepository.GetByPermitIdsAsync([permit.Id]))
        {
            _checkResultRepository.Delete(old);
        }

        List<PermitCheckResult> results = _rules
            .Select(rule => rule.Check(context))
            .Select(r => new PermitCheckResult
            {
                PermitRequestId = permit.Id,
                RuleName = r.RuleName,
                Outcome = r.Outcome,
                Description = r.Description
            })
            .ToList();

        foreach (PermitCheckResult result in results)
        {
            _checkResultRepository.Add(result);
        }

        return results;
    }

    private async Task<PermitResponse> BuildResponseAsync(
        FlightPermitRequest permit,
        Aircraft? aircraft = null,
        PilotLicense? license = null,
        IEnumerable<PermitCheckResult>? checks = null)
    {
        // Related rows always exist: aircraft, licenses and users with requests cannot be deleted (restrict).
        aircraft ??= await _aircraftRepository.GetByIdAsync(permit.AircraftId)
            ?? throw new InvalidOperationException($"Aircraft {permit.AircraftId} of a request was not found.");

        license ??= await _licenseRepository.GetByIdAsync(permit.LicenseId)
            ?? throw new InvalidOperationException($"License {permit.LicenseId} of a request was not found.");

        checks ??= await _checkResultRepository.GetByPermitIdsAsync([permit.Id]);

        Dictionary<Guid, string> namesById =
            (await _userRepository.GetByIdsAsync([permit.RequesterId, permit.PilotId]))
            .ToDictionary(u => u.Id, u => u.FullName);

        return permit.ToResponse(
            namesById[permit.RequesterId],
            aircraft,
            license,
            namesById[permit.PilotId],
            checks);
    }
}