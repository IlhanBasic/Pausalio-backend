using FluentValidation;
using Pausalio.Application.DTOs.UserProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Validators
{
    public class AddUserProfileDtoValidator : AbstractValidator<AddUserProfileDto>
    {
        public AddUserProfileDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ime je obavezno")
                .MaximumLength(50).WithMessage("Ime ne može biti duže od 50 karaktera");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Prezime je obavezno")
                .MaximumLength(50).WithMessage("Prezime ne može biti duže od 50 karaktera");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email je obavezan")
                .EmailAddress().WithMessage("Email nije u validnom formatu")
                .MaximumLength(100).WithMessage("Email ne može biti duži od 100 karaktera");

            RuleFor(x => x.PasswordHash)
                .NotEmpty().WithMessage("Password hash je obavezan")
                .MaximumLength(255).WithMessage("Password hash ne može biti duži od 255 karaktera");

            RuleFor(x => x.ProfilePicture)
                .MaximumLength(500).WithMessage("Slika profila ne može biti duža od 500 karaktera")
                .When(x => !string.IsNullOrEmpty(x.ProfilePicture));

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[0-9\s\-\(\)]{6,15}$").WithMessage("Telefon nije u validnom formatu")
                .MaximumLength(15).WithMessage("Telefon ne može biti duži od 15 karaktera")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.City)
                .MaximumLength(50).WithMessage("Grad ne može biti duži od 50 karaktera")
                .When(x => !string.IsNullOrEmpty(x.City));

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("Adresa ne može biti duža od 200 karaktera")
                .When(x => !string.IsNullOrEmpty(x.Address));
        }
    }
}