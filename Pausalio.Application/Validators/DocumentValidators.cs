using FluentValidation;
using Pausalio.Application.DTOs.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Validators
{
    public class AddDocumentDtoValidator : AbstractValidator<AddDocumentDto>
    {
        public AddDocumentDtoValidator()
        {
            RuleFor(x => x.DocumentNumber)
                .NotEmpty().WithMessage("Broj dokumenta je obavezan")
                .MaximumLength(50).WithMessage("Broj dokumenta ne može biti duži od 50 karaktera");

            RuleFor(x => x.FilePath)
                .NotEmpty().WithMessage("Putanja do fajla je obavezna")
                .MaximumLength(500).WithMessage("Putanja do fajla ne može biti duža od 500 karaktera");
        }
    }

    public class UpdateDocumentDtoValidator : AbstractValidator<UpdateDocumentDto>
    {
        public UpdateDocumentDtoValidator()
        {
            RuleFor(x => x.DocumentNumber)
                .NotEmpty().WithMessage("Broj dokumenta je obavezan")
                .MaximumLength(50).WithMessage("Broj dokumenta ne može biti duži od 50 karaktera");

            RuleFor(x => x.FilePath)
                .NotEmpty().WithMessage("Putanja do fajla je obavezna")
                .MaximumLength(500).WithMessage("Putanja do fajla ne može biti duža od 500 karaktera");
        }
    }
}