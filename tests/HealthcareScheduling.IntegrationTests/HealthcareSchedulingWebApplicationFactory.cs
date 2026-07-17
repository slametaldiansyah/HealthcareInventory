using HealthcareScheduling.Application.Common.Interfaces;
using HealthcareScheduling.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HealthcareScheduling.IntegrationTests;

public class HealthcareSchedulingWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public HealthcareSchedulingWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_connectionString));

            services.RemoveAll<IDateTimeProvider>();
            services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(DateTime.UtcNow));
        });
    }

    public void SetUtcNow(DateTime utcNow)
    {
        var provider = Services.GetRequiredService<IDateTimeProvider>() as FixedDateTimeProvider;
        if (provider is not null)
        {
            provider.UtcNow = utcNow;
        }
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await context.Appointments.ExecuteDeleteAsync();
        await context.WorkingSchedules.ExecuteDeleteAsync();
        await context.Doctors.ExecuteDeleteAsync();
        await context.Users.ExecuteDeleteAsync();
    }
}
