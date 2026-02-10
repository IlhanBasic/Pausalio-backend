using FluentValidation;
using Pausalio.Application.DTOs.TaxObligation;

namespace Pausalio.Application.Validators
{
    public class AddTaxObligationDtoValidator : AbstractValidator<AddTaxObligationDto>
    {
        public AddTaxObligationDtoValidator()
        {
            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Now).WithMessage("Rok plaćanja ne sme biti u prošlosti.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Nepoznat tip poreske obaveze.");

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0).WithMessage("Ukupan iznos mora biti veći od 0.");
        }
    }

    public class UpdateTaxObligationDtoValidator : AbstractValidator<UpdateTaxObligationDto>
    {
        public UpdateTaxObligationDtoValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Nepoznat status poreske obaveze.");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Now).WithMessage("Rok plaćanja ne sme biti u prošlosti.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Nepoznat tip poreske obaveze.");

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0).WithMessage("Ukupan iznos mora biti veći od 0.");
        }
    }
}
