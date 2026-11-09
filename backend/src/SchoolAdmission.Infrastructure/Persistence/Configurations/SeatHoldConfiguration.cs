using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolAdmission.Domain.AdmissionClasses;
using SchoolAdmission.Domain.SeatHolds;

namespace SchoolAdmission.Infrastructure.Persistence.Configurations;

public sealed class SeatHoldConfiguration : IEntityTypeConfiguration<SeatHold>
{
    public void Configure(EntityTypeBuilder<SeatHold> builder)
    {
        builder.ToTable("seat_holds");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id");

        builder.Property(h => h.AdmissionClassId)
            .HasColumnName("admission_class_id")
            .IsRequired();

        builder.HasOne<AdmissionClass>()
            .WithMany()
            .HasForeignKey(h => h.AdmissionClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(h => h.StudentId)
            .HasColumnName("student_id")
            .HasConversion(
                id => id.Value,
                value => new StudentId(value))
            .HasMaxLength(StudentId.MaxLength)
            .IsRequired();

        builder.Property(h => h.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(h => h.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(h => h.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(h => h.ConfirmedAtUtc)
            .HasColumnName("confirmed_at_utc")
            .HasColumnType("timestamptz");

        builder.Property(h => h.CancelledAtUtc)
            .HasColumnName("cancelled_at_utc")
            .HasColumnType("timestamptz");

        // Indexes specified in docs/07-database-design.md
        builder.HasIndex(h => new { h.AdmissionClassId, h.Status, h.ExpiresAtUtc })
            .HasDatabaseName("IX_seat_holds_class_status_expires");

        builder.HasIndex(h => new { h.StudentId, h.Status, h.ExpiresAtUtc })
            .HasDatabaseName("IX_seat_holds_student_status_expires");
    }
}

