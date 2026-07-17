namespace HealthcareScheduling.Domain.Exceptions;

public class CancellationNotAllowedException : Exception
{
    public CancellationNotAllowedException(string message) : base(message)
    {
    }
}
