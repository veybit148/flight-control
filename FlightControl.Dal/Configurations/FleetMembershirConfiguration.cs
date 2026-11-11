using FlightControl.Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightControl.Dal.Configurations;

public class FleetMembershipConfiguration: IEntityTypeConfiguration<FleetMembership>
{
    public void Configure(EntityTypeBuilder<FleetMembership> builder)
    {
        builder.HasIndex(fm => new {fm.FleetId, fm.PilotId})
            .IsUnique();

        builder.HasOne<Fleet>()
            .WithMany()
            .HasForeignKey(fm => fm.FleetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(fm => fm.PilotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}