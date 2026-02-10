using FluentValidation;
using Pausalio.Application.DTOs.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Validators
{
    public class AddItemDtoValidator : AbstractValidator<AddItemDto>
    {
        public AddItemDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Naziv stavke je obavezan")
                .MaximumLength(100).WithMessage("Naziv stavke ne može biti duži od 100 karaktera");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Opis ne može biti duži od 500 karaktera")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Cena po jedinici ne može biti negativna");
        }
    }

    public class UpdateItemDtoValidator : AbstractValidator<UpdateItemDto>
    {
        public UpdateItemDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Naziv stavke je obavezan")
                .MaximumLength(100).WithMessage("Naziv stavke ne može biti duži od 100 karaktera");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Opis ne može biti duži od 500 karaktera")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Cena po jedinici ne može biti negativna");
        }
    }
}