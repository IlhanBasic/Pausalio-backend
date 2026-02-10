using FluentValidation;
using Pausalio.Application.DTOs.Client;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Validators
{
    public class AddClientDtoValidator : AbstractValidator<AddClientDto>
    {
        public AddClientDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ime klijenta je obavezno")
                .MaximumLength(100).WithMessage("Ime klijenta ne može biti duže od 100 karaktera");

            RuleFor(x => x.PIB)
                .Matches(@"^\d{9}$").WithMessage("PIB mora imati tačno 9 cifara")
                .When(x => !string.IsNullOrEmpty(x.PIB) && x.ClientType == ClientType.Domestic);

            RuleFor(x => x.MB)
                .Matches(@"^\d{8}$").WithMessage("MB mora imati tačno 8 cifara")
                .When(x => !string.IsNullOrEmpty(x.MB) && x.ClientType == ClientType.Domestic);

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Adresa je obavezna")
                .MaximumLength(200).WithMessage("Adresa ne može biti duža od 200 karaktera");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("Grad je obavezan")
                .MaximumLength(50).WithMessage("Grad ne može biti duži od 50 karaktera");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email je obavezan")
                .EmailAddress().WithMessage("Email nije u validnom formatu")
                .MaximumLength(100).WithMessage("Email ne može biti duži od 100 karaktera");

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[0-9\s\-\(\)]{6,15}$").WithMessage("Telefon nije u validnom formatu")
                .MaximumLength(15).WithMessage("Telefon ne može biti duži od 15 karaktera")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Country)
                .MaximumLength(50).WithMessage("Država ne može biti duža od 50 karaktera")
                .When(x => !string.IsNullOrEmpty(x.Country));
        }
    }

    public class UpdateClientDtoValidator : AbstractValidator<UpdateClientDto>
    {
        public UpdateClientDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ime klijenta je obavezno")
                .MaximumLength(100).WithMessage("Ime klijenta ne može biti duže od 100 karaktera");

            RuleFor(x => x.PIB)
                .Matches(@"^\d{9}$").WithMessage("PIB mora imati tačno 9 cifara")
                .When(x => !string.IsNullOrEmpty(x.PIB) && x.ClientType == ClientType.Domestic);

            RuleFor(x => x.MB)
                .Matches(@"^\d{8}$").WithMessage("MB mora imati tačno 8 cifara")
                .When(x => !string.IsNullOrEmpty(x.MB) && x.ClientType == ClientType.Domestic);

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Adresa je obavezna")
                .MaximumLength(200).WithMessage("Adresa ne može biti duža od 200 karaktera");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("Grad je obavezan")
                .MaximumLength(50).WithMessage("Grad ne može biti duži od 50 karaktera");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email je obavezan")
                .EmailAddress().WithMessage("Email nije u validnom formatu")
                .MaximumLength(100).WithMessage("Email ne može biti duži od 100 karaktera");

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[0-9\s\-\(\)]{6,15}$").WithMessage("Telefon nije u validnom formatu")
                .MaximumLength(15).WithMessage("Telefon ne može biti duži od 15 karaktera")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Country)
                .MaximumLength(50).WithMessage("Država ne može biti duža od 50 karaktera")
                .When(x => !string.IsNullOrEmpty(x.Country));
        }
    }
}