using HealthcareScheduling.Application.DTOs;
using MediatR;

namespace HealthcareScheduling.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
