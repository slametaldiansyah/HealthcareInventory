using FluentValidation;

namespace HealthcareScheduling.Application.Features.Admin.Commands.CreateDoctor;

public class CreateDoctorByAdminCommandValidator : AbstractValidator<CreateDoctorByAdminCommand>
{
    public CreateDoctorByAdminCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.TimeZoneId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WorkingSchedules).NotEmpty();
        RuleForEach(x => x.WorkingSchedules).ChildRules(schedule =>
        {
            schedule.RuleFor(s => s.StartTime).NotEmpty();
            schedule.RuleFor(s => s.EndTime).NotEmpty();
        });
    }
}
