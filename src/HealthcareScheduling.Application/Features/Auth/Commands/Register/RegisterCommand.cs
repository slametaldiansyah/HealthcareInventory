using HealthcareScheduling.Application.DTOs;
using MediatR;

namespace HealthcareScheduling.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Name, string Email, string Password)
    : IRequest<RegisterResponseDto>;
