using FluentValidation;
using Pausalio.Application.DTOs.BankAccount;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddBankAccountDtoValidator : AbstractValidator<AddBankAccountDto>
    {
        public AddBankAccountDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage(_localizationHelper.BankNameRequired)
                .MaximumLength(100).WithMessage(_localizationHelper.BankNameMaxLength);

            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage(_localizationHelper.AccountNumberRequired)
                .Matches(@"^\d{10,20}$").WithMessage(_localizationHelper.AccountNumberLength);

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage(_localizationHelper.UnknownCurrency);

            RuleFor(x => x.IBAN)
                .Matches(@"^[A-Z]{2}\d{2}[A-Z0-9]{1,30}$").WithMessage(_localizationHelper.InvalidIBAN);

            RuleFor(x => x.SWIFT)
                .Matches(@"^[A-Z]{6}[A-Z0-9]{2,5}$").WithMessage(_localizationHelper.InvalidSWIFT);
        }
    }

    public class UpdateBankAccountDtoValidator : AbstractValidator<UpdateBankAccountDto>
    {
        public UpdateBankAccountDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.BankName)
                .MaximumLength(100).WithMessage(_localizationHelper.BankNameMaxLength);

            RuleFor(x => x.AccountNumber)
                .Matches(@"^\d{10,20}$").When(x => !string.IsNullOrEmpty(x.AccountNumber))
                .WithMessage(_localizationHelper.AccountNumberLength);

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage(_localizationHelper.UnknownCurrency);

            RuleFor(x => x.IBAN)
                .Matches(@"^[A-Z]{2}\d{2}[A-Z0-9]{1,30}$").When(x => !string.IsNullOrEmpty(x.IBAN))
                .WithMessage(_localizationHelper.InvalidIBAN);

            RuleFor(x => x.SWIFT)
                .Matches(@"^[A-Z]{6}[A-Z0-9]{2,5}$").When(x => !string.IsNullOrEmpty(x.SWIFT))
                .WithMessage(_localizationHelper.InvalidSWIFT);
        }
    }
}
