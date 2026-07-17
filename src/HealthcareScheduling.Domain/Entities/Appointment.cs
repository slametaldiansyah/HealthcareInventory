using HealthcareScheduling.Domain.Enums;

namespace HealthcareScheduling.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Active;
    public byte[] RowVersion { get; set; } = [];

    public Doctor Doctor { get; set; } = null!;
}
