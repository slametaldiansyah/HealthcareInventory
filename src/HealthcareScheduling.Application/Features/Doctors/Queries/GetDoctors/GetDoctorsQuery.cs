using HealthcareScheduling.Application.DTOs;
using MediatR;

namespace HealthcareScheduling.Application.Features.Doctors.Queries.GetDoctors;

public record GetDoctorsQuery : IRequest<IReadOnlyList<DoctorDto>>;
