using FlightControl.Dal;
using FlightControl.Dal.Entities;
using FlightControl.Repository.Interfaces;

namespace FlightControl.Repository.Repositories;

public class AircraftStatusChangeRepository
    : BaseRepository<AircraftStatusChange>, IAircraftStatusChangeRepository
{
    public AircraftStatusChangeRepository(AppDbContext context) : base(context) { }
}
