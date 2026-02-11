using FluentValidation;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddPaymentDtoValidator : AbstractValidator<AddPaymentDto>
    {

        public AddPaymentDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.PaymentType)
                .IsInEnum()
                .WithMessage(_localizationHelper.PaymentTypeUnknown);

            RuleFor(x => x.EntityId)
                .NotEmpty()
                .WithMessage(_localizationHelper.PaymentEntityIdRequired);

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage(_localizationHelper.PaymentAmountGreaterThanZero);

            RuleFor(x => x.Currency)
                .IsInEnum()
                .WithMessage(_localizationHelper.PaymentUnknownCurrency);

            RuleFor(x => x.ExchangeRate)
                .GreaterThan(0)
                .When(x => x.ExchangeRate.HasValue)
                .WithMessage(_localizationHelper.PaymentExchangeRateGreaterThanZero);

            RuleFor(x => x.ReferenceNumber)
                .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.ReferenceNumber))
                .WithMessage(_localizationHelper.PaymentReferenceMaxLength);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage(_localizationHelper.PaymentDescriptionMaxLength);
        }
    }

    public class UpdatePaymentDtoValidator : AbstractValidator<UpdatePaymentDto>
    {

        public UpdatePaymentDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.ReferenceNumber)
                .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.ReferenceNumber))
                .WithMessage(_localizationHelper.PaymentReferenceMaxLength);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage(_localizationHelper.PaymentDescriptionMaxLength);
        }
    }
}
