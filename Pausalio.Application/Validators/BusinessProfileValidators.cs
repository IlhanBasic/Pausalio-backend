using FluentValidation;
using Pausalio.Application.DTOs.BusinessProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Validators
{
    public class AddBusinessProfileValidator : AbstractValidator<AddBusinessProfileDto>
    {
        public AddBusinessProfileValidator()
        {
            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage("Naziv firme je obavezan")
                .MaximumLength(100).WithMessage("Naziv firme ne može biti duži od 100 karaktera");

            RuleFor(x => x.PIB)
                .NotEmpty().WithMessage("PIB je obavezan")
                .Matches(@"^\d{9}$").WithMessage("PIB mora imati tačno 9 cifara");

            RuleFor(x => x.MB)
                .Matches(@"^\d{8}$").WithMessage("MB mora imati tačno 8 cifara")
                .When(x => !string.IsNullOrEmpty(x.MB));

            RuleFor(x => x.ActivityCode)
                .MaximumLength(5).WithMessage("Šifra delatnosti ne može biti duža od 5 karaktera")
                .When(x => !string.IsNullOrEmpty(x.ActivityCode));

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("Grad je obavezan")
                .MaximumLength(50).WithMessage("Grad ne može biti duži od 50 karaktera");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Adresa je obavezna")
                .MaximumLength(200).WithMessage("Adresa ne može biti duža od 200 karaktera");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email je obavezan")
                .EmailAddress().WithMessage("Email nije u validnom formatu")
                .MaximumLength(100).WithMessage("Email ne može biti duži od 100 karaktera");

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[0-9\s\-\(\)]{6,15}$").WithMessage("Telefon nije u validnom formatu")
                .MaximumLength(15).WithMessage("Telefon ne može biti duži od 15 karaktera")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Website)
                .MaximumLength(200).WithMessage("Website ne može biti duži od 200 karaktera")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).WithMessage("Website nije u validnom formatu")
                .When(x => !string.IsNullOrEmpty(x.Website));

            RuleFor(x => x.CompanyLogo)
                .MaximumLength(500).WithMessage("Logo putanja ne može biti duža od 500 karaktera")
                .When(x => !string.IsNullOrEmpty(x.CompanyLogo));
        }
    }

    public class UpdateBusinessProfileValidator : AbstractValidator<UpdateBusinessProfileDto>
    {
        public UpdateBusinessProfileValidator()
        {
            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage("Naziv firme je obavezan")
                .MaximumLength(100).WithMessage("Naziv firme ne može biti duži od 100 karaktera");

            RuleFor(x => x.PIB)
                .NotEmpty().WithMessage("PIB je obavezan")
                .Matches(@"^\d{9}$").WithMessage("PIB mora imati tačno 9 cifara");

            RuleFor(x => x.MB)
                .Matches(@"^\d{8}$").WithMessage("MB mora imati tačno 8 cifara")
                .When(x => !string.IsNullOrEmpty(x.MB));

            RuleFor(x => x.ActivityCode)
                .MaximumLength(5).WithMessage("Šifra delatnosti ne može biti duža od 5 karaktera")
                .When(x => !string.IsNullOrEmpty(x.ActivityCode));

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("Grad je obavezan")
                .MaximumLength(50).WithMessage("Grad ne može biti duži od 50 karaktera");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Adresa je obavezna")
                .MaximumLength(200).WithMessage("Adresa ne može biti duža od 200 karaktera");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email je obavezan")
                .EmailAddress().WithMessage("Email nije u validnom formatu")
                .MaximumLength(100).WithMessage("Email ne može biti duži od 100 karaktera");

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[0-9\s\-\(\)]{6,15}$").WithMessage("Telefon nije u validnom formatu")
                .MaximumLength(15).WithMessage("Telefon ne može biti duži od 15 karaktera")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Website)
                .MaximumLength(200).WithMessage("Website ne može biti duži od 200 karaktera")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).WithMessage("Website nije u validnom formatu")
                .When(x => !string.IsNullOrEmpty(x.Website));

            RuleFor(x => x.CompanyLogo)
                .MaximumLength(500).WithMessage("Logo putanja ne može biti duža od 500 karaktera")
                .When(x => !string.IsNullOrEmpty(x.CompanyLogo));
        }
    }
}