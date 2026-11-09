namespace SchoolAdmission.Application.AdmissionClasses.Dtos;

public sealed record CreateAdmissionClassRequest(string Name, int TotalSeats);

public sealed record AdmissionClassResponse(string Id, string Name, int TotalSeats);

public sealed record AdmissionClassDetailResponse(
    string Id,
    string Name,
    int TotalSeats,
    int HeldSeats,
    int ConfirmedSeats,
    int AvailableSeats
);

