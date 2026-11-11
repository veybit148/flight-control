using FlightControl.Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightControl.Dal.Configurations;

public class AircraftConfiguration : IEntityTypeConfiguration<Aircraft>
{
    public void Configure(EntityTypeBuilder<Aircraft> builder)
    {
        builder.Property(a => a.RegistrationNumber)
            .HasMaxLength(32);

        builder.Property(a => a.Category)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(32);


        builder.HasIndex(a=> a.RegistrationNumber)
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        
        builder.HasOne<Fleet>()
            .WithMany()
            .HasForeignKey(a => a.FleetId)
            .OnDelete(DeleteBehavior.SetNull);        
    }
}
