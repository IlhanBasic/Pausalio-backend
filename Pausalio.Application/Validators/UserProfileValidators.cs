using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddUserProfileDtoValidator : AbstractValidator<AddUserProfileDto>
    {
        public AddUserProfileDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(_ => _localizationHelper.UserFirstNameRequired)
                .MaximumLength(50).WithMessage(_ => _localizationHelper.UserFirstNameMaxLength);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(_ => _localizationHelper.UserLastNameRequired)
                .MaximumLength(50).WithMessage(_ => _localizationHelper.UserLastNameMaxLength);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(_ => _localizationHelper.UserEmailRequired)
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage(_ => _localizationHelper.UserEmailInvalid)
                .MaximumLength(100).WithMessage(_ => _localizationHelper.UserEmailMaxLength);

            RuleFor(x => x.PasswordHash)
                .NotEmpty().WithMessage(_ => _localizationHelper.UserPasswordRequired)
                .MaximumLength(255).WithMessage(_ => _localizationHelper.UserPasswordMaxLength);

            RuleFor(x => x.ProfilePicture)
                .MaximumLength(500).WithMessage(_ => _localizationHelper.UserProfilePictureMaxLength)
                .When(x => !string.IsNullOrEmpty(x.ProfilePicture));

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[0-9\s\-\(\)]{6,15}$").WithMessage(_ => _localizationHelper.UserPhoneInvalid)
                .MaximumLength(15).WithMessage(_ => _localizationHelper.UserPhoneInvalid)
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.City)
                .MaximumLength(50).WithMessage(_ => _localizationHelper.UserCityMaxLength)
                .When(x => !string.IsNullOrEmpty(x.City));

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage(_ => _localizationHelper.UserAddressMaxLength)
                .When(x => !string.IsNullOrEmpty(x.Address));
        }
    }
    public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
    {
        public UpdateUserProfileDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(_ => _localizationHelper.UserFirstNameRequired)
                .MaximumLength(50).WithMessage(_ => _localizationHelper.UserFirstNameMaxLength);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(_ => _localizationHelper.UserLastNameRequired)
                .MaximumLength(50).WithMessage(_ => _localizationHelper.UserLastNameMaxLength);

            RuleFor(x => x.ProfilePicture)
                .MaximumLength(500).WithMessage(_ => _localizationHelper.UserProfilePictureMaxLength)
                .When(x => !string.IsNullOrEmpty(x.ProfilePicture));

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[0-9\s\-\(\)]{6,15}$").WithMessage(_ => _localizationHelper.UserPhoneInvalid)
                .MaximumLength(15).WithMessage(_ => _localizationHelper.UserPhoneMaxLength)
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.City)
                .MaximumLength(50).WithMessage(_ => _localizationHelper.UserCityMaxLength)
                .When(x => !string.IsNullOrEmpty(x.City));

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage(_ => _localizationHelper.UserAddressMaxLength)
                .When(x => !string.IsNullOrEmpty(x.Address));
        }
    }
}