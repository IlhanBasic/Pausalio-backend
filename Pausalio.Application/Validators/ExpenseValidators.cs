using FluentValidation;
using Pausalio.Application.DTOs.Expense;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddExpenseDtoValidator : AbstractValidator<AddExpenseDto>
    {

        public AddExpenseDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_localizationHelper.ExpenseNameRequired)
                .MaximumLength(100).WithMessage(_localizationHelper.ExpenseNameMaxLength);

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage(_localizationHelper.ExpenseAmountGreaterThanZero);
        }
    }

    public class UpdateExpenseDtoValidator : AbstractValidator<UpdateExpenseDto>
    {

        public UpdateExpenseDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage(_localizationHelper.ExpenseStatusInvalid);

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_localizationHelper.ExpenseNameRequired)
                .MaximumLength(100).WithMessage(_localizationHelper.ExpenseNameMaxLength);

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage(_localizationHelper.ExpenseAmountGreaterThanZero);
        }
    }
}
