using SchoolAdmission.Domain.Exceptions;

namespace SchoolAdmission.Domain.SeatHolds;

public readonly record struct StudentId
{
    public const int MaxLength = 64;

    public string Value { get; }

    public StudentId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Student ID cannot be empty.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new DomainException($"Student ID cannot exceed {MaxLength} characters.");
        }

        Value = trimmed;
    }

    public static StudentId From(string value) => new(value);

    public override string ToString() => Value;

    public static implicit operator string(StudentId studentId) => studentId.Value;
}

