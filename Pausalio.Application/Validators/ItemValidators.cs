using FluentValidation;
using Pausalio.Application.DTOs.Item;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddItemDtoValidator : AbstractValidator<AddItemDto>
    {

        public AddItemDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_localizationHelper.ItemNameRequired)
                .MaximumLength(100).WithMessage(_localizationHelper.ItemNameMaxLength);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage(_localizationHelper.ItemDescriptionMaxLength);

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage(_localizationHelper.ItemUnitPriceNonNegative);
        }
    }

    public class UpdateItemDtoValidator : AbstractValidator<UpdateItemDto>
    {

        public UpdateItemDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_localizationHelper.ItemNameRequired)
                .MaximumLength(100).WithMessage(_localizationHelper.ItemNameMaxLength);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage(_localizationHelper.ItemDescriptionMaxLength);

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage(_localizationHelper.ItemUnitPriceNonNegative);
        }
    }
}
