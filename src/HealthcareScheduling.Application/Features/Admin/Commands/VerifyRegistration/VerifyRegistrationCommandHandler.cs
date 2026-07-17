using AutoMapper;
using HealthcareScheduling.Application.Common.Interfaces;
using HealthcareScheduling.Application.DTOs;
using HealthcareScheduling.Domain.Enums;
using HealthcareScheduling.Domain.Exceptions;
using HealthcareScheduling.Domain.Interfaces;
using MediatR;

namespace HealthcareScheduling.Application.Features.Admin.Commands.VerifyRegistration;

public class VerifyRegistrationCommandHandler : IRequestHandler<VerifyRegistrationCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMapper _mapper;

    public VerifyRegistrationCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(VerifyRegistrationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetPendingByVerificationCodeAsync(request.Code, cancellationToken)
            ?? throw new NotFoundException("No pending registration found for this verification code.");

        if (user.VerificationCodeExpiresAt is null
            || user.VerificationCodeExpiresAt < _dateTimeProvider.UtcNow)
        {
            throw new InvalidAppointmentException("Verification code has expired. Ask the user to register again.");
        }

        user.Status = UserAccountStatus.Active;
        user.VerificationCode = null;
        user.VerificationCodeExpiresAt = null;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
