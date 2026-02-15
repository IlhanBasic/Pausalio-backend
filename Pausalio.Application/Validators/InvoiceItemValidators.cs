using FluentValidation;
using Pausalio.Application.DTOs.InvoiceItem;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddInvoiceItemDtoValidator : AbstractValidator<AddInvoiceItemDto>
    {

        public AddInvoiceItemDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_localizationHelper.InvoiceItemNameRequired)
                .MaximumLength(100).WithMessage(_localizationHelper.InvoiceItemNameMaxLength);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(_localizationHelper.InvoiceItemDescriptionMaxLength)
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage(_localizationHelper.InvoiceItemQuantityGreaterThanZero);

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage(_localizationHelper.InvoiceItemUnitPriceMinZero);
        }
    }

    public class UpdateInvoiceItemDtoValidator : AbstractValidator<UpdateInvoiceItemDto>
    {

        public UpdateInvoiceItemDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_localizationHelper.InvoiceItemNameRequired)
                .MaximumLength(100).WithMessage(_localizationHelper.InvoiceItemNameMaxLength);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(_localizationHelper.InvoiceItemDescriptionMaxLength)
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage(_localizationHelper.InvoiceItemQuantityGreaterThanZero);

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage(_localizationHelper.InvoiceItemUnitPriceMinZero);
        }
    }
}
