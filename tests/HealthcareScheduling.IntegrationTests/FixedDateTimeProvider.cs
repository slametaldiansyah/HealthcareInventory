using HealthcareScheduling.Application.Common.Interfaces;

namespace HealthcareScheduling.IntegrationTests;

public class FixedDateTimeProvider : IDateTimeProvider
{
    public FixedDateTimeProvider(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; set; }
}
