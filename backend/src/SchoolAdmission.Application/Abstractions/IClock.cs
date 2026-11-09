namespace SchoolAdmission.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

