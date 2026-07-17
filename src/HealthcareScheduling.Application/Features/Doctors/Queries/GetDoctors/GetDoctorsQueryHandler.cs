using HealthcareScheduling.Application.DTOs;
using HealthcareScheduling.Domain.Interfaces;
using MediatR;

namespace HealthcareScheduling.Application.Features.Doctors.Queries.GetDoctors;

public class GetDoctorsQueryHandler : IRequestHandler<GetDoctorsQuery, IReadOnlyList<DoctorDto>>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorsQueryHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<IReadOnlyList<DoctorDto>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
    {
        var doctors = await _doctorRepository.GetAllWithSchedulesAsync(cancellationToken);

        return doctors
            .Select(d => new DoctorDto(
                d.Id,
                d.Name,
                d.TimeZoneId,
                d.WorkingSchedules
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .Select(s => new DoctorWorkingScheduleDto(
                        s.DayOfWeek,
                        s.StartTime.ToString("HH:mm"),
                        s.EndTime.ToString("HH:mm")))
                    .ToList()))
            .ToList();
    }
}
