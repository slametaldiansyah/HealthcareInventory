using HealthcareScheduling.Application.DTOs;
using MediatR;

namespace HealthcareScheduling.Application.Features.Admin.Commands.CreateDoctor;

public record CreateDoctorByAdminCommand(
    string Name,
    string Email,
    string Password,
    string TimeZoneId,
    IReadOnlyList<WorkingScheduleRequestDto> WorkingSchedules) : IRequest<UserDto>;
