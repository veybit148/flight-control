using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Domain.Common;
using FlightControl.Domain.Exceptions;
using FlightControl.Domain.Mappings;
using FlightControl.Domain.Security;
using FlightControl.Dto.Fleets;
using FlightControl.Repository.Interfaces;
using FlightControl.Validation.Core;

namespace FlightControl.Domain.Operations.Fleets;

public class FleetOperation : IFleetOperation
{
    private const string FleetNotFoundMessage = "Fleet not found.";
    private const string PilotNotFoundMessage = "Pilot not found.";

    private readonly AbstractValidator<CreateFleetRequest> _createValidator;
    private readonly AbstractValidator<AddFleetMemberRequest> _addMemberValidator;
    private readonly IFleetRepository _fleetRepository;
    private readonly IFleetMembershipRepository _membershipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public FleetOperation(
        AbstractValidator<CreateFleetRequest> createValidator,
        AbstractValidator<AddFleetMemberRequest> addMemberValidator,
        IFleetRepository fleetRepository,
        IFleetMembershipRepository membershipRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _createValidator = createValidator;
        _addMemberValidator = addMemberValidator;
        _fleetRepository = fleetRepository;
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<FleetResponse> CreateAsync(CreateFleetRequest request)
    {
        _createValidator.ThrowIfInvalid(request);

        CreateFleetRequest normalized = request with { Name = request.Name.Trim() };

        if (await _fleetRepository.ExistsByNameAsync(CurrentUserId, normalized.Name))
        {
            throw new ConflictException("You already have a fleet with this name.");
        }

        Fleet fleet = normalized.ToEntity(CurrentUserId);

        _fleetRepository.Add(fleet);
        await _unitOfWork.SaveChangesAsync();

        return fleet.ToResponse();
    }

    public async Task<IReadOnlyList<FleetResponse>> ListAsync()
    {
        IEnumerable<Fleet> fleets = IsAuthority
            ? await _fleetRepository.GetAllAsync()
            : await _fleetRepository.GetListByOwnerIdAsync(CurrentUserId);

        return fleets.Select(f => f.ToResponse()).ToList();
    }

    public async Task<FleetResponse> GetByIdAsync(Guid id)
    {
        Fleet fleet = await GetAccessibleFleetAsync(id, allowAuthority: true);
        return fleet.ToResponse();
    }

    public async Task DeleteAsync(Guid id)
    {
        Fleet fleet = await GetAccessibleFleetAsync(id, allowAuthority: false);

        _fleetRepository.Delete(fleet);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<FleetMemberResponse> AddMemberAsync(Guid fleetId, AddFleetMemberRequest request)
    {
        _addMemberValidator.ThrowIfInvalid(request);

        Fleet fleet = await GetAccessibleFleetAsync(fleetId, allowAuthority: false);

        User? pilot = await _userRepository.GetByEmailAsync(EmailNormalizer.Normalize(request.PilotEmail));

        if (pilot == null || pilot.Role != UserRole.Pilot)
        {
            throw new NotFoundException(PilotNotFoundMessage);
        }

        if (await _membershipRepository.ExistsAtFleetAsync(fleet.Id, pilot.Id))
        {
            throw new ConflictException("This pilot is already a member of the fleet.");
        }

        FleetMembership membership = new()
        {
            FleetId = fleet.Id,
            PilotId = pilot.Id
        };

        _membershipRepository.Add(membership);
        await _unitOfWork.SaveChangesAsync();

        return membership.ToMemberResponse(pilot);
    }

    public async Task<IReadOnlyList<FleetMemberResponse>> ListMembersAsync(Guid fleetId)
    {
        Fleet fleet = await GetAccessibleFleetAsync(fleetId, allowAuthority: true);

        IReadOnlyList<FleetMembership> memberships = await _membershipRepository.GetListByFleetIdAsync(fleet.Id);

        if (memberships.Count == 0)
        {
            return [];
        }

        IReadOnlyList<User> pilots = await _userRepository.GetByIdsAsync(
            memberships.Select(m => m.PilotId).ToList());

        Dictionary<Guid, User> pilotsById = pilots.ToDictionary(u => u.Id);

        return memberships
            .Select(m => m.ToMemberResponse(pilotsById[m.PilotId]))
            .ToList();
    }

    public async Task RemoveMemberAsync(Guid fleetId, Guid pilotId)
    {
        Fleet fleet = await GetAccessibleFleetAsync(fleetId, allowAuthority: false);

        FleetMembership membership = await _membershipRepository.GetByFleetAndPilotAsync(fleet.Id, pilotId)
            ?? throw new NotFoundException("This pilot is not a member of the fleet.");

        _membershipRepository.Delete(membership);
        await _unitOfWork.SaveChangesAsync();
    }

    // Helpers

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new InvalidOperationException("FleetOperation requires an authenticated user.");

    private UserRole CurrentRole => _currentUser.Role
        ?? throw new InvalidOperationException("FleetOperation requires an authenticated user.");

    private bool IsAuthority => CurrentRole == UserRole.Authority;

    // Returns the fleet if the current user may access it; otherwise behaves as if it does not exist.
    private async Task<Fleet> GetAccessibleFleetAsync(Guid id, bool allowAuthority)
    {
        Fleet? fleet = await _fleetRepository.GetByIdAsync(id);

        bool canAccess = fleet != null &&
            (fleet.OwnerId == CurrentUserId || (allowAuthority && IsAuthority));

        if (!canAccess)
        {
            throw new NotFoundException(FleetNotFoundMessage);
        }

        return fleet!;
    }
}