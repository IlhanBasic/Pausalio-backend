using FluentValidation;
using Pausalio.Application.DTOs.Payment;

namespace Pausalio.Application.Validators
{
    public class AddPaymentDtoValidator : AbstractValidator<AddPaymentDto>
    {
        public AddPaymentDtoValidator()
        {
            RuleFor(x => x.PaymentType)
                .IsInEnum().WithMessage("Nepoznat tip uplate.");

            RuleFor(x => x.EntityId)
                .NotEmpty().WithMessage("EntityId je obavezan.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Iznos mora biti veći od 0.");

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage("Nepoznata valuta.");

            RuleFor(x => x.ExchangeRate)
                .GreaterThan(0).WithMessage("Kurs mora biti veći od 0.")
                .When(x => x.ExchangeRate.HasValue);

            RuleFor(x => x.ReferenceNumber)
                .MaximumLength(50).WithMessage("Referentni broj ne sme imati više od 50 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Opis ne sme imati više od 500 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }

    public class UpdatePaymentDtoValidator : AbstractValidator<UpdatePaymentDto>
    {
        public UpdatePaymentDtoValidator()
        {
            RuleFor(x => x.ReferenceNumber)
                .MaximumLength(50).WithMessage("Referentni broj ne sme imati više od 50 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Opis ne sme imati više od 500 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
