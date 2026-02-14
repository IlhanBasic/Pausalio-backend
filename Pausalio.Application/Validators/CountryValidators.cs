using FluentValidation;
using Pausalio.Application.DTOs.Country;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddCountryDtoValidator : AbstractValidator<AddCountryDto>
    {
        public AddCountryDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(_localizationHelper.CountryNameRequired)
                .MaximumLength(100)
                .WithMessage(_localizationHelper.CountryNameTooLong);

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage(_localizationHelper.CountryCodeRequired)
                .MaximumLength(2)
                .WithMessage(_localizationHelper.CountryCodeInvalidLength);
        }
    }

    public class UpdateCountryDtoValidator : AbstractValidator<UpdateCountyDto>
    {
        public UpdateCountryDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(_localizationHelper.CountryNameRequired)
                .MaximumLength(100)
                .WithMessage(_localizationHelper.CountryNameTooLong);

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage(_localizationHelper.CountryCodeRequired)
                .MaximumLength(2)
                .WithMessage(_localizationHelper.CountryCodeInvalidLength);
        }
    }
}