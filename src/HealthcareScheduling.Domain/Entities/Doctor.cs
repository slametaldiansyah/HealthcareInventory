namespace HealthcareScheduling.Domain.Entities;

public class Doctor
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "UTC";

    public ICollection<WorkingSchedule> WorkingSchedules { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}
