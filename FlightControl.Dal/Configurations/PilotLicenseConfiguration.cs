using FlightControl.Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightControl.Dal.Configurations;

public class PilotLicenseConfiguration : IEntityTypeConfiguration<PilotLicense>
{
    public void Configure(EntityTypeBuilder<PilotLicense> builder)
    {
        builder.Property(l => l.LicenseNumber)
            .HasMaxLength(32);

        builder.HasIndex(l => l.LicenseNumber)
            .IsUnique();

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(l => l.SuspensionReason)
            .HasMaxLength(500);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(l => l.PilotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}