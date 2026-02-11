using FluentValidation;
using Pausalio.Application.DTOs.Invoice;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddInvoiceDtoValidator : AbstractValidator<AddInvoiceDto>
    {

        public AddInvoiceDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage(_localizationHelper.InvoiceClientIdRequired);

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Today)
                .When(x => x.DueDate.HasValue)
                .WithMessage(_localizationHelper.InvoiceDueDateNotInPast);

            RuleFor(x => x.ExchangeRate)
                .GreaterThan(0)
                .WithMessage(_localizationHelper.InvoiceExchangeRateGreaterThanZero);

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage(_localizationHelper.InvoiceItemsRequired);
        }
    }

    public class UpdateInvoiceDtoValidator : AbstractValidator<UpdateInvoiceDto>
    {

        public UpdateInvoiceDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage(_localizationHelper.InvoiceClientIdRequired);

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Today)
                .When(x => x.DueDate.HasValue)
                .WithMessage(_localizationHelper.InvoiceDueDateNotInPast);

            RuleFor(x => x.ExchangeRate)
                .GreaterThan(0)
                .WithMessage(_localizationHelper.InvoiceExchangeRateGreaterThanZero);

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage(_localizationHelper.InvoiceItemsRequired);
        }
    }
}
