using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventCampusAPI.Models;

using FluentValidation;

namespace EventCampusAPI.Validators
{
public class RegisterRequestModelValidator : AbstractValidator<RegisterRequestModel>
{
    public RegisterRequestModelValidator()
    {

            RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir email adresi girin.")
            .Must(email => email.EndsWith(".edu.tr")).WithMessage("Sadece .edu.tr uzantılı email adresleri kabul edilir.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("İsim boş olamaz.")
            .MaximumLength(16).WithMessage("İsim en fazla 16 karakter olabilir.");

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Soyisim boş olamaz.")
            .MaximumLength(16).WithMessage("Soyisim en fazla 16 karakter olabilir.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
            .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Şifre en az bir özel karakter içermelidir.");

        RuleFor(x => x.UniversityId)
            .NotNull().WithMessage("Üniversite seçimi zorunludur.")
            .GreaterThan(0).WithMessage("Geçerli bir üniversite seçiniz.");
    }
}
}