namespace HealthcareScheduling.Application.Services;

public static class VerificationCodeGenerator
{
    public static string GenerateFourDigitCode() =>
        Random.Shared.Next(1000, 10000).ToString();
}
