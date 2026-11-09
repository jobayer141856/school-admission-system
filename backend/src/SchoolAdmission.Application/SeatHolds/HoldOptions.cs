namespace SchoolAdmission.Application.SeatHolds;

public sealed class HoldOptions
{
    public const string SectionName = "HOLD_DURATION_SECONDS";
    public const int DefaultDurationSeconds = 600;

    public int DurationSeconds { get; set; } = DefaultDurationSeconds;
}

