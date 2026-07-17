using FluentValidation;

namespace HealthcareScheduling.Application.Features.Admin.Commands.CreateUser;

public class CreateUserByAdminCommandValidator : AbstractValidator<CreateUserByAdminCommand>
{
    public CreateUserByAdminCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
