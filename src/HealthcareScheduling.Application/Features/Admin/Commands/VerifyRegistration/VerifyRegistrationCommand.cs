using HealthcareScheduling.Application.DTOs;
using MediatR;

namespace HealthcareScheduling.Application.Features.Admin.Commands.VerifyRegistration;

public record VerifyRegistrationCommand(string Code) : IRequest<UserDto>;
