using SchoolAdmission.Domain.Common;
using SchoolAdmission.Domain.Exceptions;

namespace SchoolAdmission.Domain.AdmissionClasses;

public sealed class AdmissionClass : Entity
{
    public const int MaxNameLength = 160;

    public string Name { get; private set; } = string.Empty;
    public int TotalSeats { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    // Private constructor for EF Core
    private AdmissionClass() { }

    private AdmissionClass(Guid id, string name, int totalSeats, DateTimeOffset createdAtUtc)
        : base(id)
    {
        Name = name;
        TotalSeats = totalSeats;
        CreatedAtUtc = createdAtUtc;
    }

    public static AdmissionClass Create(string name, int totalSeats, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Admission class name is required.");
        }

        var trimmedName = name.Trim();
        if (trimmedName.Length > MaxNameLength)
        {
            throw new DomainException($"Admission class name cannot exceed {MaxNameLength} characters.");
        }

        if (totalSeats <= 0)
        {
            throw new DomainException("Total seats must be a positive integer.");
        }

        return new AdmissionClass(Guid.NewGuid(), trimmedName, totalSeats, now);
    }
}

