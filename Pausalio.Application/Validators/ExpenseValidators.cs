using FluentValidation;
using Pausalio.Application.DTOs.Expense;

namespace Pausalio.Application.Validators
{
    public class AddExpenseDtoValidator : AbstractValidator<AddExpenseDto>
    {
        public AddExpenseDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Naziv troška je obavezan.")
                .MaximumLength(100).WithMessage("Naziv troška ne sme imati više od 100 karaktera.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Iznos mora biti veći od 0.");
        }
    }

    public class UpdateExpenseDtoValidator : AbstractValidator<UpdateExpenseDto>
    {
        public UpdateExpenseDtoValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Nepoznat status troška.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Naziv troška je obavezan.")
                .MaximumLength(100).WithMessage("Naziv troška ne sme imati više od 100 karaktera.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Iznos mora biti veći od 0.");
        }
    }
}
