using FlightControl.Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightControl.Dal.Configurations;

public class FleetConfiguration : IEntityTypeConfiguration<Fleet>
{
    public void Configure(EntityTypeBuilder<Fleet> builder)
    {
        builder.Property(f => f.Name)
            .HasMaxLength(256);
        builder.HasIndex(f => new {f.OwnerId, f.Name})
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
