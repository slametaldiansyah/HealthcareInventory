using FluentValidation;
using HealthcareScheduling.Application.Services;

namespace HealthcareScheduling.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.Duration)
            .Must(SchedulingRules.IsValidDuration)
            .WithMessage("Duration must be 15, 30, or 60 minutes.");
    }
}
