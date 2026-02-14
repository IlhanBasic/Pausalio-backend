using FluentValidation;
using Pausalio.Application.DTOs.City;
using Pausalio.Shared.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Validators
{
    public class AddCityDtoValidator : AbstractValidator<AddCityDto>
    {
        public AddCityDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage(_=>_localizationHelper.PostalCodeRequired)
                .MaximumLength(10).WithMessage(_=>_localizationHelper.PostalCodeMaxLength);
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_=>_localizationHelper.CityNameRequired)
                .MaximumLength(100).WithMessage(_ => _localizationHelper.CityNameMaxLength);
        }
    }
    public class UpdateCityDtoValidator : AbstractValidator<AddCityDto>
    {
        public UpdateCityDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage(_ => _localizationHelper.PostalCodeRequired)
                .MaximumLength(10).WithMessage(_ => _localizationHelper.PostalCodeMaxLength);
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_ => _localizationHelper.CityNameRequired)
                .MaximumLength(100).WithMessage(_ => _localizationHelper.CityNameMaxLength);
        }
    }
}
