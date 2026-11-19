using FlightControl.Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightControl.Dal.Configurations;

public class PermitCheckResultConfiguration : IEntityTypeConfiguration<PermitCheckResult>
{
    public void Configure(EntityTypeBuilder<PermitCheckResult> builder)
    {
        builder.Property(r => r.RuleName).HasMaxLength(64);
        builder.Property(r => r.Outcome).HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.Description).HasMaxLength(1000);

        builder.HasOne<FlightPermitRequest>()
            .WithMany()
            .HasForeignKey(r => r.PermitRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}