using FlightControl.Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightControl.Dal.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email)
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.FullName)
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(256);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(32);

        // No HasDefaultValue here: PendingApproval is the enum's CLR default (0), so EF would
        // treat it as "not set" and let the database default overwrite it. The default value
        // for existing rows lives only in the AddUserStatus migration.
        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(32);
    }
}