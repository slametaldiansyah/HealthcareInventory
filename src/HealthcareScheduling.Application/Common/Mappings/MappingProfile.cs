using AutoMapper;
using HealthcareScheduling.Application.DTOs;
using HealthcareScheduling.Domain.Entities;

namespace HealthcareScheduling.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ConstructUsing(s => new UserDto(s.Id, s.Name, s.Email, s.Role, s.Status, s.DoctorId));

        CreateMap<Appointment, AppointmentResponseDto>()
            .ConstructUsing(s => new AppointmentResponseDto(
                s.Id,
                s.DoctorId,
                s.PatientId,
                new DateTimeOffset(s.StartUtc, TimeSpan.Zero),
                new DateTimeOffset(s.EndUtc, TimeSpan.Zero),
                s.Status));
    }
}
