using FluentValidation;
using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Shared.Localization;
using System;

namespace Pausalio.Application.Validators
{
    public class AddBusinessProfileDtoValidator : AbstractValidator<AddBusinessProfileDto>
    {
        public AddBusinessProfileDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage(_localizationHelper.BusinessNameRequired)
                .MaximumLength(150).WithMessage(_localizationHelper.BusinessNameMaxLength);

            RuleFor(x => x.PIB)
                .NotEmpty().WithMessage(_localizationHelper.PIBRequired)
                .Matches(@"^\d{9}$").WithMessage(_localizationHelper.PIBLength);

            RuleFor(x => x.MB)
                .Matches(@"^\d{8}$").When(x => !string.IsNullOrEmpty(x.MB))
                .WithMessage(_localizationHelper.MBLength);

            RuleFor(x => x.ActivityCodeId)
                .NotEmpty().WithMessage(_localizationHelper.ActivityCodeRequired);

            RuleFor(x => x.City)
                .NotEmpty().WithMessage(_localizationHelper.CityRequired)
                .MaximumLength(100).WithMessage(_localizationHelper.CityMaxLength);

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage(_localizationHelper.AddressRequired)
                .MaximumLength(200).WithMessage(_localizationHelper.AddressMaxLength);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(_localizationHelper.EmailRequired)
                .EmailAddress().WithMessage(_localizationHelper.EmailInvalid)
                .MaximumLength(100).WithMessage(_localizationHelper.EmailMaxLength);

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[0-9\s\-\(\)]{6,15}$").When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage(_localizationHelper.PhoneInvalid)
                .MaximumLength(15).WithMessage(_localizationHelper.PhoneMaxLength);

            RuleFor(x => x.Website)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Website))
                .WithMessage(_localizationHelper.WebsiteMaxLength)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.Website))
                .WithMessage(_localizationHelper.WebsiteInvalid);

            RuleFor(x => x.CompanyLogo)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.CompanyLogo))
                .WithMessage(_localizationHelper.CompanyLogoMaxLength);
        }
    }

    public class UpdateBusinessProfileDtoValidator : AbstractValidator<UpdateBusinessProfileDto>
    {
        public UpdateBusinessProfileDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage(_localizationHelper.BusinessNameRequired)
                .MaximumLength(150).WithMessage(_localizationHelper.BusinessNameMaxLength);

            RuleFor(x => x.PIB)
                .NotEmpty().WithMessage(_localizationHelper.PIBRequired)
                .Matches(@"^\d{9}$").WithMessage(_localizationHelper.PIBLength);

            RuleFor(x => x.MB)
                .Matches(@"^\d{8}$").When(x => !string.IsNullOrEmpty(x.MB))
                .WithMessage(_localizationHelper.MBLength);

            RuleFor(x => x.ActivityCodeId)
                .NotEmpty().WithMessage(_localizationHelper.ActivityCodeRequired);

            RuleFor(x => x.City)
                .NotEmpty().WithMessage(_localizationHelper.CityRequired)
                .MaximumLength(100).WithMessage(_localizationHelper.CityMaxLength);

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage(_localizationHelper.AddressRequired)
                .MaximumLength(200).WithMessage(_localizationHelper.AddressMaxLength);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(_localizationHelper.EmailRequired)
                .EmailAddress().WithMessage(_localizationHelper.EmailInvalid)
                .MaximumLength(100).WithMessage(_localizationHelper.EmailMaxLength);

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[0-9\s\-\(\)]{6,15}$").When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage(_localizationHelper.PhoneInvalid)
                .MaximumLength(15).WithMessage(_localizationHelper.PhoneMaxLength);

            RuleFor(x => x.Website)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Website))
                .WithMessage(_localizationHelper.WebsiteMaxLength)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.Website))
                .WithMessage(_localizationHelper.WebsiteInvalid);

            RuleFor(x => x.CompanyLogo)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.CompanyLogo))
                .WithMessage(_localizationHelper.CompanyLogoMaxLength);
        }
    }
}
