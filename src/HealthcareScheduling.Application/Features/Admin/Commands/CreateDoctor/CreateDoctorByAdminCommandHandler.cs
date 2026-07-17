using AutoMapper;
using HealthcareScheduling.Application.Common.Interfaces;
using HealthcareScheduling.Application.DTOs;
using HealthcareScheduling.Domain.Entities;
using HealthcareScheduling.Domain.Enums;
using HealthcareScheduling.Domain.Exceptions;
using HealthcareScheduling.Domain.Interfaces;
using MediatR;

namespace HealthcareScheduling.Application.Features.Admin.Commands.CreateDoctor;

public class CreateDoctorByAdminCommandHandler : IRequestHandler<CreateDoctorByAdminCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateDoctorByAdminCommandHandler(
        IUserRepository userRepository,
        IDoctorRepository doctorRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _doctorRepository = doctorRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(CreateDoctorByAdminCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new InvalidAppointmentException("Email is already registered.");
        }

        var schedules = new List<WorkingSchedule>();
        foreach (var item in request.WorkingSchedules)
        {
            if (!TimeOnly.TryParse(item.StartTime, out var start) || !TimeOnly.TryParse(item.EndTime, out var end))
            {
                throw new InvalidAppointmentException(
                    $"Invalid working schedule time '{item.StartTime}'–'{item.EndTime}'.");
            }

            if (end <= start)
            {
                throw new InvalidAppointmentException("Working schedule end time must be after start time.");
            }

            schedules.Add(new WorkingSchedule
            {
                Id = Guid.NewGuid(),
                DayOfWeek = item.DayOfWeek,
                StartTime = start,
                EndTime = end
            });
        }

        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            TimeZoneId = request.TimeZoneId.Trim(),
            WorkingSchedules = schedules
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.Doctor,
            Status = UserAccountStatus.Active,
            DoctorId = doctor.Id
        };

        await _doctorRepository.AddAsync(doctor, cancellationToken);
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
