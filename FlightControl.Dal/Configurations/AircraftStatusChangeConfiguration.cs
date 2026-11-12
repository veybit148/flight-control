using FlightControl.Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightControl.Dal.Configurations;

public class AircraftStatusChangeConfiguration : IEntityTypeConfiguration<AircraftStatusChange>
{
    public void Configure(EntityTypeBuilder<AircraftStatusChange> builder)
    {
        builder.Property(c => c.FromStatus)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(c => c.ToStatus)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(c => c.Reason)
            .HasMaxLength(500);

        builder.HasOne<Aircraft>()
            .WithMany()
            .HasForeignKey(c => c.AircraftId)
            .OnDelete(DeleteBehavior.Cascade);   // karar senin, aşağıda

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.ChangedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}