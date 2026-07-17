namespace HealthcareScheduling.Application.DTOs;

/// <summary>
/// Create appointment request. For role User, PatientId is ignored and taken from the JWT.
/// For role Admin, PatientId is required.
/// </summary>
public sealed class CreateAppointmentRequestDto
{
    public Guid DoctorId { get; init; }
    public DateTimeOffset Start { get; init; }
    public int Duration { get; init; }

    /// <summary>Required only for Admin. Ignored for User (taken from token).</summary>
    public Guid? PatientId { get; init; }
}
