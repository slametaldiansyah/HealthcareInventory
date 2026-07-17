using HealthcareScheduling.Application.Common.Interfaces;
using HealthcareScheduling.Application.DTOs;
using HealthcareScheduling.Application.Services;
using HealthcareScheduling.Domain.Entities;
using HealthcareScheduling.Domain.Enums;
using HealthcareScheduling.Domain.Exceptions;
using HealthcareScheduling.Domain.Interfaces;
using MediatR;

namespace HealthcareScheduling.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<RegisterResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new InvalidAppointmentException("Email is already registered.");
        }

        var code = await GenerateUniqueVerificationCodeAsync(cancellationToken);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.User,
            Status = UserAccountStatus.Pending,
            VerificationCode = code,
            VerificationCodeExpiresAt = _dateTimeProvider.UtcNow.AddHours(24)
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterResponseDto(user.Id, user.Email, user.Status, code);
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
