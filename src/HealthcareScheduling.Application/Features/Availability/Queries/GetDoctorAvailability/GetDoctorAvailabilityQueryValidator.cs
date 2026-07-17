using FluentValidation;
using HealthcareScheduling.Application.Services;

namespace HealthcareScheduling.Application.Features.Availability.Queries.GetDoctorAvailability;

public class GetDoctorAvailabilityQueryValidator : AbstractValidator<GetDoctorAvailabilityQuery>
{
    public GetDoctorAvailabilityQueryValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.From).LessThan(x => x.To)
            .WithMessage("'from' must be earlier than 'to'.");
        RuleFor(x => x.SlotMinutes)
            .Must(SchedulingRules.IsValidDuration)
            .WithMessage("Slot duration must be 15, 30, or 60 minutes.");
    }
}
