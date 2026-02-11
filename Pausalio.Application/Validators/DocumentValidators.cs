using FluentValidation;
using Pausalio.Application.DTOs.Document;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Validators
{
    public class AddDocumentDtoValidator : AbstractValidator<AddDocumentDto>
    {

        public AddDocumentDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.DocumentNumber)
                .NotEmpty().WithMessage(_localizationHelper.DocumentNumberRequired)
                .MaximumLength(50).WithMessage(_localizationHelper.DocumentNumberMaxLength);

            RuleFor(x => x.FilePath)
                .NotEmpty().WithMessage(_localizationHelper.DocumentFilePathRequired)
                .MaximumLength(500).WithMessage(_localizationHelper.DocumentFilePathMaxLength);
        }
    }

    public class UpdateDocumentDtoValidator : AbstractValidator<UpdateDocumentDto>
    {

        public UpdateDocumentDtoValidator(ILocalizationHelper _localizationHelper)
        {

            RuleFor(x => x.DocumentNumber)
                .NotEmpty().WithMessage(_localizationHelper.DocumentNumberRequired)
                .MaximumLength(50).WithMessage(_localizationHelper.DocumentNumberMaxLength);

            RuleFor(x => x.FilePath)
                .NotEmpty().WithMessage(_localizationHelper.DocumentFilePathRequired)
                .MaximumLength(500).WithMessage(_localizationHelper.DocumentFilePathMaxLength);
        }
    }
}
