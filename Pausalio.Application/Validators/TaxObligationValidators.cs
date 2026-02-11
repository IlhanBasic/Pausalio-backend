using FluentValidation;
using Pausalio.Application.DTOs.TaxObligation;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddTaxObligationDtoValidator : AbstractValidator<AddTaxObligationDto>
    {
        public AddTaxObligationDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Now)
                .WithMessage(_localizationHelper.TaxDueDateInPast);

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage(_localizationHelper.TaxTypeUnknown);

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0)
                .WithMessage(_localizationHelper.TaxTotalAmountGreaterThanZero);
        }
    }

    public class UpdateTaxObligationDtoValidator : AbstractValidator<UpdateTaxObligationDto>
    {

        public UpdateTaxObligationDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage(_localizationHelper.TaxStatusUnknown);

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Now)
                .WithMessage(_localizationHelper.TaxDueDateInPast);

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage(_localizationHelper.TaxTypeUnknown);

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0)
                .WithMessage(_localizationHelper.TaxTotalAmountGreaterThanZero);
        }
    }
}
