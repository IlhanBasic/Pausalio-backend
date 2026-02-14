using FluentValidation;
using Pausalio.Application.DTOs.BusinessInvite;
using Pausalio.Shared.Localization;
using System;

namespace Pausalio.Application.Validators
{
    public class AddBusinessInviteDtoValidator : AbstractValidator<AddBusinessInviteDto>
    {
        public AddBusinessInviteDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(_localizationHelper.EmailRequired)
                .EmailAddress().WithMessage(_localizationHelper.EmailInvalid)
                .MaximumLength(100).WithMessage(_localizationHelper.EmailMaxLength);
        }
    }

    public class UpdateBusinessInviteDtoValidator : AbstractValidator<UpdateBusinessInviteDto>
    {
        public UpdateBusinessInviteDtoValidator(ILocalizationHelper _localizationHelper)
        {
        }
    }

    public class AcceptInviteDtoValidator : AbstractValidator<AcceptInviteDto>
    {
        public AcceptInviteDtoValidator(ILocalizationHelper _localizationHelper)
        {
            RuleFor(x => x.InviteToken)
                .NotEmpty().WithMessage(_localizationHelper.InviteTokenRequired);
        }
    }
}
