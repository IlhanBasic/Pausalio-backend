using FluentValidation;
using Pausalio.Application.DTOs.UserBusinessProfile;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddUserBusinessProfileDtoValidator 
        : AbstractValidator<AddUserBusinessProfileDto>
    {
        public AddUserBusinessProfileDtoValidator(
            ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(_localizationHelper.UserIdRequired);

            RuleFor(x => x.BusinessProfileId)
                .NotEmpty()
                .WithMessage(_localizationHelper.BusinessProfileIdRequired);

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage(_localizationHelper.UserBusinessRoleRequired);
        }
    }

    public class UpdateUserBusinessProfileDtoValidator 
        : AbstractValidator<UpdateUserBusinessProfileDto>
    {
        public UpdateUserBusinessProfileDtoValidator(
            ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage(_localizationHelper.UserBusinessRoleRequired);
        }
    }
}
