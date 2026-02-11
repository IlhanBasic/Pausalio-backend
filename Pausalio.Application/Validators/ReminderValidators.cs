using FluentValidation;
using Pausalio.Application.DTOs.Reminder;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddReminderDtoValidator : AbstractValidator<AddReminderDto>
    {

        public AddReminderDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(_localizationHelper.ReminderTitleRequired)
                .MaximumLength(100)
                .WithMessage(_localizationHelper.ReminderTitleMaxLength);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage(_localizationHelper.ReminderDescriptionMaxLength);

            RuleFor(x => x.ReminderType)
                .IsInEnum()
                .WithMessage(_localizationHelper.ReminderTypeUnknown);

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Now)
                .WithMessage(_localizationHelper.ReminderDueDateInPast);
        }
    }

    public class UpdateReminderDtoValidator : AbstractValidator<UpdateReminderDto>
    {

        public UpdateReminderDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(_localizationHelper.ReminderTitleRequired)
                .MaximumLength(100)
                .WithMessage(_localizationHelper.ReminderTitleMaxLength);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage(_localizationHelper.ReminderDescriptionMaxLength);

            RuleFor(x => x.ReminderType)
                .IsInEnum()
                .WithMessage(_localizationHelper.ReminderTypeUnknown);

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Now)
                .WithMessage(_localizationHelper.ReminderDueDateInPast);
        }
    }
}
