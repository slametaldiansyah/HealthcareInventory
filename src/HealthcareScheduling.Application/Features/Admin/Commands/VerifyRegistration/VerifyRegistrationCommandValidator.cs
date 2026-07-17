using FluentValidation;

namespace HealthcareScheduling.Application.Features.Admin.Commands.VerifyRegistration;

public class VerifyRegistrationCommandValidator : AbstractValidator<VerifyRegistrationCommand>
{
    public VerifyRegistrationCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(4)
            .Matches(@"^\d{4}$")
            .WithMessage("Verification code must be a 4-digit number.");
    }
}
