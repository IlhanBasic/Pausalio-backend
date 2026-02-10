using FluentValidation;
using Pausalio.Application.DTOs.BankAccount;

namespace Pausalio.Application.Validators
{
    public class AddBankAccountDtoValidator : AbstractValidator<AddBankAccountDto>
    {
        public AddBankAccountDtoValidator()
        {
            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Naziv banke je obavezan.")
                .MaximumLength(100).WithMessage("Naziv banke ne sme imati više od 100 karaktera.");

            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("Broj računa je obavezan.")
                .Matches(@"^\d{10,20}$").WithMessage("Broj računa mora imati između 10 i 20 cifara.");

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage("Nepoznata valuta.");

            RuleFor(x => x.IBAN)
                .Matches(@"^[A-Z]{2}\d{2}[A-Z0-9]{1,30}$").WithMessage("Neispravan IBAN format.");

            RuleFor(x => x.SWIFT)
                .Matches(@"^[A-Z]{6}[A-Z0-9]{2,5}$").WithMessage("Neispravan SWIFT/BIC format.");
        }
    }

    public class UpdateBankAccountDtoValidator : AbstractValidator<UpdateBankAccountDto>
    {
        public UpdateBankAccountDtoValidator()
        {
            RuleFor(x => x.BankName)
                .MaximumLength(100).WithMessage("Naziv banke ne sme imati više od 100 karaktera.");

            RuleFor(x => x.AccountNumber)
                .Matches(@"^\d{10,20}$").When(x => !string.IsNullOrEmpty(x.AccountNumber))
                .WithMessage("Broj računa mora imati između 10 i 20 cifara.");

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage("Nepoznata valuta.");

            RuleFor(x => x.IBAN)
                .Matches(@"^[A-Z]{2}\d{2}[A-Z0-9]{1,30}$").When(x => !string.IsNullOrEmpty(x.IBAN))
                .WithMessage("Neispravan IBAN format.");

            RuleFor(x => x.SWIFT)
                .Matches(@"^[A-Z]{6}[A-Z0-9]{2,5}$").When(x => !string.IsNullOrEmpty(x.SWIFT))
                .WithMessage("Neispravan SWIFT/BIC format.");
        }
    }
}
