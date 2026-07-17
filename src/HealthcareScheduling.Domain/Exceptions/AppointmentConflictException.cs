namespace HealthcareScheduling.Domain.Exceptions;

public class AppointmentConflictException : Exception
{
    public AppointmentConflictException(string message) : base(message)
    {
    }
}
