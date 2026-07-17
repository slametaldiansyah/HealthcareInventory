using FluentValidation;

namespace OrderInventory.Application.Features.Orders.Commands.PayOrder;

public class PayOrderCommandValidator : AbstractValidator<PayOrderCommand>
{
    public PayOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.PaymentExternalId).NotEmpty().MaximumLength(128);
    }
}
