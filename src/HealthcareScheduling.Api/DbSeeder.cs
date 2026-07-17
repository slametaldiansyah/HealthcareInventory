using HealthcareScheduling.Application.Common.Interfaces;
using HealthcareScheduling.Domain.Entities;
using HealthcareScheduling.Domain.Enums;
using HealthcareScheduling.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HealthcareScheduling.Api;

public static class DbSeeder
{
    public static readonly Guid SeedDoctorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid SeedAdminUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid SeedRegularUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid SeedDoctorUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    /// <summary>Patient Id equals the seeded regular user Id.</summary>
    public static readonly Guid SeedPatientId = SeedRegularUserId;

    /// <summary>Backward-compatible alias for SeedAdminUserId.</summary>
    public static readonly Guid SeedUserId = SeedAdminUserId;

    public const string SeedAdminEmail = "admin@healthcare.local";
    public const string SeedUserEmail = SeedAdminEmail;
    public const string SeedRegularUserEmail = "user@healthcare.local";
    public const string SeedDoctorEmail = "doctor@healthcare.local";
    public const string SeedUserPassword = "Password123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();

        if (!await context.Doctors.AnyAsync(d => d.Id == SeedDoctorId))
        {
            context.Doctors.Add(new Doctor
            {
                Id = SeedDoctorId,
                Name = "Dr. Jane Smith",
                TimeZoneId = "UTC",
                WorkingSchedules =
                [
                    new WorkingSchedule
                    {
                        Id = Guid.NewGuid(),
                        DayOfWeek = DayOfWeek.Monday,
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(12, 0)
                    },
                    new WorkingSchedule
                    {
                        Id = Guid.NewGuid(),
                        DayOfWeek = DayOfWeek.Wednesday,
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(12, 0)
                    },
                    new WorkingSchedule
                    {
                        Id = Guid.NewGuid(),
                        DayOfWeek = DayOfWeek.Friday,
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(12, 0)
                    }
                ]
            });
            await context.SaveChangesAsync();
        }

        await EnsureUserAsync(
            context,
            passwordHasher,
            SeedAdminUserId,
            "Healthcare Admin",
            SeedAdminEmail,
            UserRole.Admin,
            UserAccountStatus.Active,
            doctorId: null);

        await EnsureUserAsync(
            context,
            passwordHasher,
            SeedRegularUserId,
            "Healthcare User",
            SeedRegularUserEmail,
            UserRole.User,
            UserAccountStatus.Active,
            doctorId: null);

        await EnsureUserAsync(
            context,
            passwordHasher,
            SeedDoctorUserId,
            "Dr. Jane Smith",
            SeedDoctorEmail,
            UserRole.Doctor,
            UserAccountStatus.Active,
            doctorId: SeedDoctorId);

        await context.SaveChangesAsync();
    }

    private static async Task EnsureUserAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        Guid id,
        string name,
        string email,
        UserRole role,
        UserAccountStatus status,
        Guid? doctorId)
    {
        var existing = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existing is null)
        {
            context.Users.Add(new User
            {
                Id = id,
                Name = name,
                Email = email,
                PasswordHash = passwordHasher.Hash(SeedUserPassword),
                Role = role,
                Status = status,
                DoctorId = doctorId
            });
            return;
        }

        existing.Role = role;
        existing.Status = status;
        existing.DoctorId = doctorId;
        existing.VerificationCode = null;
        existing.VerificationCodeExpiresAt = null;
    }
}
