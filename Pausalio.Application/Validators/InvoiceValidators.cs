using FluentValidation;
using Pausalio.Application.DTOs.Invoice;

namespace Pausalio.Application.Validators
{
    public class AddInvoiceDtoValidator : AbstractValidator<AddInvoiceDto>
    {
        public AddInvoiceDtoValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("ClientId je obavezan.");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Today)
                .When(x => x.DueDate.HasValue)
                .WithMessage("Rok dospeća ne može biti u prošlosti.");

            RuleFor(x => x.ExchangeRate)
                .GreaterThan(0).WithMessage("Kurs mora biti veći od 0.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Faktura mora sadržati bar jednu stavku.");
        }
    }

    public class UpdateInvoiceDtoValidator : AbstractValidator<UpdateInvoiceDto>
    {
        public UpdateInvoiceDtoValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("ClientId je obavezan.");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Today)
                .When(x => x.DueDate.HasValue)
                .WithMessage("Rok dospeća ne može biti u prošlosti.");

            RuleFor(x => x.ExchangeRate)
                .GreaterThan(0).WithMessage("Kurs mora biti veći od 0.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Faktura mora sadržati bar jednu stavku.");
        }
    }
}
