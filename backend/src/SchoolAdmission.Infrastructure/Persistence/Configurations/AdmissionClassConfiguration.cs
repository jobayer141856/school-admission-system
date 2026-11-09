using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolAdmission.Domain.AdmissionClasses;

namespace SchoolAdmission.Infrastructure.Persistence.Configurations;

public sealed class AdmissionClassConfiguration : IEntityTypeConfiguration<AdmissionClass>
{
    public void Configure(EntityTypeBuilder<AdmissionClass> builder)
    {
        builder.ToTable("admission_classes", t =>
        {
            t.HasCheckConstraint("CK_admission_classes_total_seats", "\"total_seats\" > 0");
        });

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(AdmissionClass.MaxNameLength)
            .IsRequired();

        builder.Property(c => c.TotalSeats)
            .HasColumnName("total_seats")
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}

