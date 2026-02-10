using FluentValidation;
using Pausalio.Application.DTOs.Reminder;

namespace Pausalio.Application.Validators
{
    public class AddReminderDtoValidator : AbstractValidator<AddReminderDto>
    {
        public AddReminderDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Naslov je obavezan.")
                .MaximumLength(100).WithMessage("Naslov ne sme imati više od 100 karaktera.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Opis ne sme imati više od 500 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.ReminderType)
                .IsInEnum().WithMessage("Nepoznat tip podsetnika.");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Now).WithMessage("Rok dospeća ne može biti u prošlosti.");
        }
    }

    public class UpdateReminderDtoValidator : AbstractValidator<UpdateReminderDto>
    {
        public UpdateReminderDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Naslov je obavezan.")
                .MaximumLength(100).WithMessage("Naslov ne sme imati više od 100 karaktera.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Opis ne sme imati više od 500 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.ReminderType)
                .IsInEnum().WithMessage("Nepoznat tip podsetnika.");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Now).WithMessage("Rok dospeća ne može biti u prošlosti.");
        }
    }
}
