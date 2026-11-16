using FlightControl.Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightControl.Dal.Configurations;

public class LicenseCategoryConfiguration : IEntityTypeConfiguration<LicenseCategory>
{
    public void Configure(EntityTypeBuilder<LicenseCategory> builder)
    {
        builder.Property(c => c.Category)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(c => new { c.LicenseId, c.Category })
            .IsUnique();

        builder.HasOne<PilotLicense>()
            .WithMany()
            .HasForeignKey(c => c.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}