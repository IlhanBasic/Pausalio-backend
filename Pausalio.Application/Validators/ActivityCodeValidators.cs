using FluentValidation;
using Pausalio.Application.DTOs.ActivityCode;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddActivityCodeDtoValidator : AbstractValidator<AddActivityCodeDto>
    {
        public AddActivityCodeDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage(_localizationHelper.ActivityCodeRequired)
                .MaximumLength(20)
                .WithMessage(_localizationHelper.ActivityCodeTooLong);

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage(_localizationHelper.ActivityCodeDescriptionRequired)
                .MaximumLength(200)
                .WithMessage(_localizationHelper.ActivityCodeDescriptionTooLong);
        }
    }

    public class UpdateActivityCodeDtoValidator : AbstractValidator<UpdateActivityCodeDto>
    {
        public UpdateActivityCodeDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage(_localizationHelper.ActivityCodeRequired)
                .MaximumLength(20)
                .WithMessage(_localizationHelper.ActivityCodeTooLong);

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage(_localizationHelper.ActivityCodeDescriptionRequired)
                .MaximumLength(200)
                .WithMessage(_localizationHelper.ActivityCodeDescriptionTooLong);
        }
    }
}