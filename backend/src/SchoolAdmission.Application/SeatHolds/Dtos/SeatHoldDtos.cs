namespace SchoolAdmission.Application.SeatHolds.Dtos;

public sealed record SeatHoldResponse(
    string HoldId,
    string ClassId,
    string StudentId,
    string Status,
    DateTimeOffset ExpiresAt
);

public sealed record SeatHoldDetailResponse(
    string HoldId,
    string ClassId,
    string Status,
    DateTimeOffset ExpiresAt
);

public sealed record ConfirmHoldResponse(
    string HoldId,
    string Status
);

public sealed record StudentHoldItemResponse(
    string HoldId,
    string ClassId,
    string ClassName,
    string Status,
    DateTimeOffset ExpiresAt
);

