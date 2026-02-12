using FluentValidation;
using Pausalio.Application.DTOs.Client;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddClientDtoValidator : AbstractValidator<AddClientDto>
    {
        public AddClientDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_localizationHelper.ClientNameRequired)
                .MaximumLength(100).WithMessage(_localizationHelper.ClientNameMaxLength);

            RuleFor(x => x.PIB)
                .Matches(@"^\d{9}$")
                .When(x => !string.IsNullOrEmpty(x.PIB) && x.ClientType == ClientType.Domestic)
                .WithMessage(_localizationHelper.ClientPIBLength);

            RuleFor(x => x.MB)
                .Matches(@"^\d{8}$")
                .When(x => !string.IsNullOrEmpty(x.MB) && x.ClientType == ClientType.Domestic)
                .WithMessage(_localizationHelper.ClientMBLength);

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage(_localizationHelper.ClientAddressRequired)
                .MaximumLength(200).WithMessage(_localizationHelper.ClientAddressMaxLength);

            RuleFor(x => x.City)
                .NotEmpty().WithMessage(_localizationHelper.ClientCityRequired)
                .MaximumLength(50).WithMessage(_localizationHelper.ClientCityMaxLength);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(_localizationHelper.ClientEmailRequired)
                .EmailAddress().WithMessage(_localizationHelper.ClientEmailInvalid)
                .MaximumLength(100).WithMessage(_localizationHelper.ClientEmailMaxLength);

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[0-9\s\-\(\)]{6,15}$")
                .When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage(_localizationHelper.ClientPhoneInvalid)
                .MaximumLength(15).WithMessage(_localizationHelper.ClientPhoneMaxLength);

            RuleFor(x => x.CountryId)
                .Must(id => id != Guid.Empty)
                .When(x => x.CountryId.HasValue)
                .WithMessage(_localizationHelper.ClientCountryInvalid);
        }
    }

    public class UpdateClientDtoValidator : AbstractValidator<UpdateClientDto>
    {
        public UpdateClientDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_localizationHelper.ClientNameRequired)
                .MaximumLength(100).WithMessage(_localizationHelper.ClientNameMaxLength);

            RuleFor(x => x.PIB)
                .Matches(@"^\d{9}$")
                .When(x => !string.IsNullOrEmpty(x.PIB) && x.ClientType == ClientType.Domestic)
                .WithMessage(_localizationHelper.ClientPIBLength);

            RuleFor(x => x.MB)
                .Matches(@"^\d{8}$")
                .When(x => !string.IsNullOrEmpty(x.MB) && x.ClientType == ClientType.Domestic)
                .WithMessage(_localizationHelper.ClientMBLength);

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage(_localizationHelper.ClientAddressRequired)
                .MaximumLength(200).WithMessage(_localizationHelper.ClientAddressMaxLength);

            RuleFor(x => x.City)
                .NotEmpty().WithMessage(_localizationHelper.ClientCityRequired)
                .MaximumLength(50).WithMessage(_localizationHelper.ClientCityMaxLength);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(_localizationHelper.ClientEmailRequired)
                .EmailAddress().WithMessage(_localizationHelper.ClientEmailInvalid)
                .MaximumLength(100).WithMessage(_localizationHelper.ClientEmailMaxLength);

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[0-9\s\-\(\)]{6,15}$")
                .When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage(_localizationHelper.ClientPhoneInvalid)
                .MaximumLength(15).WithMessage(_localizationHelper.ClientPhoneMaxLength);

            RuleFor(x => x.CountryId)
                .Must(id => id != Guid.Empty)
                .When(x => x.CountryId.HasValue)
                .WithMessage(_localizationHelper.ClientCountryInvalid);
        }
    }
}
