using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using HealthcareScheduling.Api;
using HealthcareScheduling.Application.DTOs;
using HealthcareScheduling.Domain.Enums;

namespace HealthcareScheduling.IntegrationTests;

public class SchedulingApiTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Server=localhost,1433;Database=HealthcareSchedulingDb_Test;User Id=sa;Password=Your_strong_Password123;TrustServerCertificate=True;";

    private HealthcareSchedulingWebApplicationFactory? _factory;
    private HttpClient? _client;

    private async Task ResetAndSeedAsync()
    {
        await _factory!.ResetDatabaseAsync();
        await DbSeeder.SeedAsync(_factory.Services);
        await AuthenticateAsync();
    }

    private async Task AuthenticateAsync(string email = DbSeeder.SeedAdminEmail, string password = DbSeeder.SeedUserPassword)
    {
        var loginResponse = await _client!.PostAsJsonAsync("/api/auth/login", new LoginRequestDto(email, password));

        loginResponse.EnsureSuccessStatusCode();
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
    }

    private static CreateAppointmentRequestDto AdminBook(DateTimeOffset start, int duration = 30, Guid? patientId = null) =>
        new()
        {
            DoctorId = DbSeeder.SeedDoctorId,
            Start = start,
            Duration = duration,
            PatientId = patientId ?? DbSeeder.SeedPatientId
        };

    private static CreateAppointmentRequestDto UserBook(DateTimeOffset start, int duration = 30) =>
        new()
        {
            DoctorId = DbSeeder.SeedDoctorId,
            Start = start,
            Duration = duration
        };

    public async Task InitializeAsync()
    {
        _factory = new HealthcareSchedulingWebApplicationFactory(ConnectionString);
        _client = _factory.CreateClient();
        await ResetAndSeedAsync();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetAvailability_ReturnsExpectedSlots_ForMondayMorning()
    {
        await ResetAndSeedAsync();
        var monday = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc);
        var response = await _client!.GetAsync(
            $"/api/doctors/{DbSeeder.SeedDoctorId}/availability?from={monday.AddHours(8):O}&to={monday.AddHours(13):O}&slot=30");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var slots = await response.Content.ReadFromJsonAsync<List<AvailabilitySlotDto>>();
        slots.Should().NotBeNull();
        slots!.Select(s => s.Start.UtcDateTime.ToString("HH:mm"))
            .Should()
            .Equal("09:00", "09:30", "10:00", "10:30", "11:00", "11:30");
    }

    [Fact]
    public async Task GetDoctors_ReturnsSeededDoctor_ForAuthenticatedUser()
    {
        await ResetAndSeedAsync();
        await AuthenticateAsync(DbSeeder.SeedRegularUserEmail);

        var response = await _client!.GetAsync("/api/doctors");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var doctors = await response.Content.ReadFromJsonAsync<List<DoctorDto>>();
        doctors.Should().NotBeNull();
        doctors!.Should().ContainSingle(d => d.Id == DbSeeder.SeedDoctorId);
        doctors[0].WorkingSchedules.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAppointment_Succeeds_ForValidSlot()
    {
        await ResetAndSeedAsync();
        var start = new DateTimeOffset(new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc));

        var response = await _client!.PostAsJsonAsync("/api/appointments", AdminBook(start));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateAppointment_AsUser_UsesAuthenticatedPatientId()
    {
        await ResetAndSeedAsync();
        await AuthenticateAsync(DbSeeder.SeedRegularUserEmail);
        var start = new DateTimeOffset(new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc));

        var response = await _client!.PostAsJsonAsync("/api/appointments", UserBook(start));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<AppointmentResponseDto>();
        created!.PatientId.Should().Be(DbSeeder.SeedRegularUserId);
    }

    [Fact]
    public async Task ListAppointments_Succeeds_ForAdmin()
    {
        await ResetAndSeedAsync();
        var start = new DateTimeOffset(new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc));
        await _client!.PostAsJsonAsync("/api/appointments", AdminBook(start));

        var response = await _client.GetAsync("/api/appointments");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var appointments = await response.Content.ReadFromJsonAsync<List<AppointmentResponseDto>>();
        appointments.Should().NotBeNull();
        appointments!.Should().HaveCount(1);
        appointments[0].DoctorId.Should().Be(DbSeeder.SeedDoctorId);
        appointments[0].PatientId.Should().Be(DbSeeder.SeedPatientId);
    }

    [Fact]
    public async Task ListAppointments_ReturnsOwnHistory_ForRegularUser()
    {
        await ResetAndSeedAsync();
        var start = new DateTimeOffset(new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc));
        await _client!.PostAsJsonAsync("/api/appointments", AdminBook(start));

        await AuthenticateAsync(DbSeeder.SeedRegularUserEmail);
        var response = await _client.GetAsync("/api/appointments");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var appointments = await response.Content.ReadFromJsonAsync<List<AppointmentResponseDto>>();
        appointments.Should().NotBeNull();
        appointments!.Should().HaveCount(1);
        appointments[0].PatientId.Should().Be(DbSeeder.SeedRegularUserId);
    }

    [Fact]
    public async Task ListAppointments_ReturnsOwnAppointments_ForDoctor()
    {
        await ResetAndSeedAsync();
        var start = new DateTimeOffset(new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc));
        await _client!.PostAsJsonAsync("/api/appointments", AdminBook(start));

        await AuthenticateAsync(DbSeeder.SeedDoctorEmail);
        var response = await _client.GetAsync("/api/appointments");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var appointments = await response.Content.ReadFromJsonAsync<List<AppointmentResponseDto>>();
        appointments.Should().NotBeNull();
        appointments!.Should().HaveCount(1);
        appointments[0].DoctorId.Should().Be(DbSeeder.SeedDoctorId);
    }

    [Fact]
    public async Task Register_ThenAdminVerify_AllowsLogin()
    {
        await ResetAndSeedAsync();
        _client!.DefaultRequestHeaders.Authorization = null;

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto(
            "New Patient",
            "new.patient@healthcare.local",
            "Password123!"));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterResponseDto>();
        registered!.Status.Should().Be(UserAccountStatus.Pending);
        registered.VerificationCode.Should().HaveLength(4);

        var pendingLogin = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto(
            "new.patient@healthcare.local",
            "Password123!"));
        pendingLogin.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await AuthenticateAsync();
        var verifyResponse = await _client.PostAsJsonAsync(
            "/api/admin/registrations/verify",
            new VerifyRegistrationRequestDto(registered.VerificationCode));

        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await AuthenticateAsync("new.patient@healthcare.local");
        var listResponse = await _client.GetAsync("/api/appointments");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateAppointment_ReturnsUnauthorized_WithoutToken()
    {
        await ResetAndSeedAsync();
        _client!.DefaultRequestHeaders.Authorization = null;

        var start = new DateTimeOffset(new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc));
        var response = await _client.PostAsJsonAsync("/api/appointments", AdminBook(start));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAppointment_ReturnsConflict_WhenOverlapping()
    {
        await ResetAndSeedAsync();
        var start = new DateTimeOffset(new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc));
        await _client!.PostAsJsonAsync("/api/appointments", AdminBook(start));

        var overlapStart = new DateTimeOffset(new DateTime(2026, 7, 20, 9, 45, 0, DateTimeKind.Utc));
        var response = await _client.PostAsJsonAsync("/api/appointments", AdminBook(overlapStart, patientId: Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateAppointment_ReturnsBadRequest_OutsideWorkingHours()
    {
        await ResetAndSeedAsync();
        var start = new DateTimeOffset(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc));

        var response = await _client!.PostAsJsonAsync("/api/appointments", AdminBook(start));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelAppointment_Succeeds_BeforeCutoff()
    {
        await ResetAndSeedAsync();
        var start = new DateTimeOffset(new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc));
        var createResponse = await _client!.PostAsJsonAsync("/api/appointments", AdminBook(start));

        var created = await createResponse.Content.ReadFromJsonAsync<AppointmentResponseDto>();
        _factory!.SetUtcNow(new DateTime(2026, 7, 20, 7, 30, 0, DateTimeKind.Utc));

        var response = await _client.DeleteAsync($"/api/appointments/{created!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CancelAppointment_ReturnsConflict_AfterCutoff()
    {
        await ResetAndSeedAsync();
        var start = new DateTimeOffset(new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc));
        var createResponse = await _client!.PostAsJsonAsync("/api/appointments", AdminBook(start));

        var created = await createResponse.Content.ReadFromJsonAsync<AppointmentResponseDto>();
        _factory!.SetUtcNow(new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc));

        var response = await _client.DeleteAsync($"/api/appointments/{created!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateAppointment_OnlyOneSucceeds_UnderConcurrentBooking()
    {
        await ResetAndSeedAsync();
        var start = new DateTimeOffset(new DateTime(2026, 7, 20, 11, 0, 0, DateTimeKind.Utc));
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => _client!.PostAsJsonAsync("/api/appointments", AdminBook(start, patientId: Guid.NewGuid())))
            .ToArray();

        var responses = await Task.WhenAll(tasks);
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        successCount.Should().Be(1);
        conflictCount.Should().Be(19);
    }
}
