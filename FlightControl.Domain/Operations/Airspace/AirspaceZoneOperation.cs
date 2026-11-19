using FlightControl.Dal.Entities;
using FlightControl.Dal.Spatial;
using FlightControl.Domain.Exceptions;
using FlightControl.Domain.Mappings;
using FlightControl.Dto.Airspace;
using FlightControl.Repository.Interfaces;
using FlightControl.Validation.Core;

namespace FlightControl.Domain.Operations.Airspace;

public class AirspaceZoneOperation : IAirspaceZoneOperation
{
    private const string ZoneNotFoundMessage = "Airspace zone not found.";

    private readonly AbstractValidator<AirspaceZoneRequest> _zoneValidator;
    private readonly AbstractValidator<ZonesAtPointQuery> _pointValidator;
    private readonly IAirspaceZoneRepository _zoneRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AirspaceZoneOperation(
        AbstractValidator<AirspaceZoneRequest> zoneValidator,
        AbstractValidator<ZonesAtPointQuery> pointValidator,
        IAirspaceZoneRepository zoneRepository,
        IUnitOfWork unitOfWork)
    {
        _zoneValidator = zoneValidator;
        _pointValidator = pointValidator;
        _zoneRepository = zoneRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AirspaceZoneResponse> CreateAsync(AirspaceZoneRequest request)
    {
        _zoneValidator.ThrowIfInvalid(request);

        AirspaceZone zone = request.ToEntity();
        _zoneRepository.Add(zone);
        await _unitOfWork.SaveChangesAsync();

        return zone.ToResponse();
    }

    public async Task<IReadOnlyList<AirspaceZoneResponse>> ListAsync()
    {
        IEnumerable<AirspaceZone> zones = await _zoneRepository.GetAllAsync();
        return zones.Select(z => z.ToResponse()).ToList();
    }

    public async Task<AirspaceZoneResponse> GetByIdAsync(Guid id)
    {
        AirspaceZone zone = await GetZoneAsync(id);
        return zone.ToResponse();
    }

    public async Task<AirspaceZoneResponse> UpdateAsync(Guid id, AirspaceZoneRequest request)
    {
        _zoneValidator.ThrowIfInvalid(request);

        AirspaceZone zone = await GetZoneAsync(id);
        request.ApplyTo(zone);
        await _unitOfWork.SaveChangesAsync();

        return zone.ToResponse();
    }

    public async Task DeleteAsync(Guid id)
    {
        AirspaceZone zone = await GetZoneAsync(id);
        _zoneRepository.Delete(zone);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<AirspaceZoneResponse>> GetAtPointAsync(ZonesAtPointQuery query)
    {
        _pointValidator.ThrowIfInvalid(query);

        var point = Geo.CreatePoint(query.Latitude, query.Longitude);
        DateTime at = query.At ?? DateTime.UtcNow;

        IReadOnlyList<AirspaceZone> zones =
            await _zoneRepository.GetActiveAtPointAsync(point, query.AltitudeMeters, at);

        return zones.Select(z => z.ToResponse()).ToList();
    }

    private async Task<AirspaceZone> GetZoneAsync(Guid id) =>
        await _zoneRepository.GetByIdAsync(id) ?? throw new NotFoundException(ZoneNotFoundMessage);
}