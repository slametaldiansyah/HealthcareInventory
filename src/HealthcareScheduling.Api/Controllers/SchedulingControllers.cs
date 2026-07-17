using HealthcareScheduling.Application.DTOs;
using HealthcareScheduling.Application.Features.Admin.Commands.CreateDoctor;
using HealthcareScheduling.Application.Features.Admin.Commands.CreateUser;
using HealthcareScheduling.Application.Features.Admin.Commands.VerifyRegistration;
using HealthcareScheduling.Application.Features.Admin.Queries.GetPendingRegistrations;
using HealthcareScheduling.Application.Features.Appointments.Commands.CancelAppointment;
using HealthcareScheduling.Application.Features.Appointments.Commands.CreateAppointment;
using HealthcareScheduling.Application.Features.Appointments.Queries.GetAppointments;
using HealthcareScheduling.Application.Features.Auth.Commands.Login;
using HealthcareScheduling.Application.Features.Auth.Commands.Register;
using HealthcareScheduling.Application.Features.Availability.Queries.GetDoctorAvailability;
using HealthcareScheduling.Application.Features.Doctors.Queries.GetDoctors;
using HealthcareScheduling.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareScheduling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponseDto>> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RegisterCommand(request.Name, request.Email, request.Password),
            cancellationToken);

        return CreatedAtAction(nameof(Login), result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return Ok(result);
    }
}

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("registrations")]
    [ProducesResponseType(typeof(IReadOnlyList<PendingRegistrationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<PendingRegistrationDto>>> GetPendingRegistrations(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPendingRegistrationsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("registrations/verify")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> VerifyRegistration(
        [FromBody] VerifyRegistrationRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new VerifyRegistrationCommand(request.Code),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("users")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> CreateUser(
        [FromBody] CreateUserByAdminRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateUserByAdminCommand(
                request.Name,
                request.Email,
                request.Password,
                request.ActivateImmediately),
            cancellationToken);

        return CreatedAtAction(nameof(CreateUser), new { id = result.Id }, result);
    }

    [HttpPost("doctors")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> CreateDoctor(
        [FromBody] CreateDoctorByAdminRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateDoctorByAdminCommand(
                request.Name,
                request.Email,
                request.Password,
                request.TimeZoneId,
                request.WorkingSchedules),
            cancellationToken);

        return CreatedAtAction(nameof(CreateDoctor), new { id = result.Id }, result);
    }
}

[ApiController]
[Route("api/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<DoctorDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDoctorsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/availability")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<AvailabilitySlotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AvailabilitySlotDto>>> GetAvailability(
        Guid id,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] int slot,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetDoctorAvailabilityQuery(id, from, to, slot),
            cancellationToken);

        return Ok(result);
    }
}

[ApiController]
[Authorize]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<AppointmentResponseDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAppointmentsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentResponseDto>> Create(
        [FromBody] CreateAppointmentRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateAppointmentCommand(
                request.DoctorId,
                request.Start,
                request.Duration,
                request.PatientId),
            cancellationToken);

        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelAppointmentCommand(id), cancellationToken);
        return NoContent();
    }
}
