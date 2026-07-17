using HealthcareScheduling.Application.Common.Interfaces;

namespace HealthcareScheduling.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
