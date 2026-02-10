using FluentValidation;
using Pausalio.Application.DTOs.UserBusinessProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Validators
{
    public class AddUserBusinessProfileDtoValidator : AbstractValidator<AddUserBusinessProfileDto>
    {
        public AddUserBusinessProfileDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("ID korisnika je obavezan");

            RuleFor(x => x.BusinessProfileId)
                .NotEmpty().WithMessage("ID business profila je obavezan");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Uloga korisnika je obavezna");
        }

    }
    public class UpdateUserBusinessProfileDtoValidator : AbstractValidator<UpdateUserBusinessProfileDto>
    {
        public UpdateUserBusinessProfileDtoValidator()
        {
            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Uloga korisnika je obavezna");
        }

    }
}
