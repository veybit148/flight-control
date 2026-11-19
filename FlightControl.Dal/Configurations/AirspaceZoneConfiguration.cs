using FlightControl.Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightControl.Dal.Configurations;

public class AirspaceZoneConfiguration : IEntityTypeConfiguration<AirspaceZone>
{
    public void Configure(EntityTypeBuilder<AirspaceZone> builder)
    {
        builder.Property(z => z.Name)
            .HasMaxLength(256);

        builder.Property(z => z.Type)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(z => z.Area)
            .HasColumnType("geography (Polygon, 4326)");

        builder.HasIndex(z => z.Area)
            .HasMethod("gist");
    }
}