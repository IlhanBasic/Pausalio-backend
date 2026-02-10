using FluentValidation;
using Pausalio.Application.DTOs.InvoiceItem;

namespace Pausalio.Application.Validators
{
    public class AddInvoiceItemDtoValidator : AbstractValidator<AddInvoiceItemDto>
    {
        public AddInvoiceItemDtoValidator()
        {
            RuleFor(x => x.InvoiceId)
                .NotEmpty().WithMessage("InvoiceId je obavezan.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Naziv stavke je obavezan.")
                .MaximumLength(100).WithMessage("Naziv stavke ne sme imati više od 100 karaktera.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Opis ne sme imati više od 500 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Količina mora biti veća od 0.");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Cena po jedinici mora biti najmanje 0.");
        }
    }

    public class UpdateInvoiceItemDtoValidator : AbstractValidator<UpdateInvoiceItemDto>
    {
        public UpdateInvoiceItemDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Naziv stavke je obavezan.")
                .MaximumLength(100).WithMessage("Naziv stavke ne sme imati više od 100 karaktera.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Opis ne sme imati više od 500 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Količina mora biti veća od 0.");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Cena po jedinici mora biti najmanje 0.");
        }
    }
}
