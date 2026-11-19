using FlightControl.Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightControl.Dal.Configurations;

public class FlightPermitRequestConfiguration : IEntityTypeConfiguration<FlightPermitRequest>
{
    public void Configure(EntityTypeBuilder<FlightPermitRequest> builder)
    {
        builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.Purpose).HasMaxLength(500);
        builder.Property(p => p.DecisionNote).HasMaxLength(1000);
        builder.Property(p => p.Version).IsRowVersion();
        
        builder.Property(p => p.Area).HasColumnType("geography (Polygon, 4326)");
        builder.HasIndex(p => p.Area).HasMethod("gist");

        builder.HasIndex(p => new { p.Status, p.PlannedStart });

        builder.HasOne<User>().WithMany().HasForeignKey(p => p.RequesterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(p => p.PilotId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(p => p.DecidedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Aircraft>().WithMany().HasForeignKey(p => p.AircraftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PilotLicense>().WithMany().HasForeignKey(p => p.LicenseId).OnDelete(DeleteBehavior.Restrict);
    }
}