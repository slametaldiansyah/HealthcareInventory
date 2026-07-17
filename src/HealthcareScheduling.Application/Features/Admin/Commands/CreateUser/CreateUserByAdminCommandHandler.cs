using AutoMapper;
using HealthcareScheduling.Application.Common.Interfaces;
using HealthcareScheduling.Application.DTOs;
using HealthcareScheduling.Application.Services;
using HealthcareScheduling.Domain.Entities;
using HealthcareScheduling.Domain.Enums;
using HealthcareScheduling.Domain.Exceptions;
using HealthcareScheduling.Domain.Interfaces;
using MediatR;

namespace HealthcareScheduling.Application.Features.Admin.Commands.CreateUser;

public class CreateUserByAdminCommandHandler : IRequestHandler<CreateUserByAdminCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMapper _mapper;

    public CreateUserByAdminCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(CreateUserByAdminCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new InvalidAppointmentException("Email is already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.User
        };

        if (request.ActivateImmediately)
        {
            user.Status = UserAccountStatus.Active;
        }
        else
        {
            user.Status = UserAccountStatus.Pending;
            user.VerificationCode = await GenerateUniqueVerificationCodeAsync(cancellationToken);
            user.VerificationCodeExpiresAt = _dateTimeProvider.UtcNow.AddHours(24);
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }

    private async Task<string> GenerateUniqueVerificationCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var code = VerificationCodeGenerator.GenerateFourDigitCode();
            if (!await _userRepository.ExistsPendingVerificationCodeAsync(code, cancellationToken))
            {
                return code;
            }
        }

        throw new InvalidAppointmentException("Unable to generate a unique verification code. Please try again.");
    }
}
