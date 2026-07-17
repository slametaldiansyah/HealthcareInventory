using HealthcareScheduling.Application.DTOs;
using MediatR;

namespace HealthcareScheduling.Application.Features.Admin.Commands.CreateUser;

public record CreateUserByAdminCommand(
    string Name,
    string Email,
    string Password,
    bool ActivateImmediately) : IRequest<UserDto>;
