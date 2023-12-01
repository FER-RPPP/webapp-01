using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  public class OwnerValidator : AbstractValidator<Owner> {
    public OwnerValidator() {
      RuleFor(o => o.Oib)
        .NotEmpty().WithMessage("OIB je obvezno polje")
        .Length(11).WithMessage("OIB mora sadržavati 11 znakova");

      RuleFor(o => o.Name)
        .NotEmpty().WithMessage("Ime je obvezno polje");

      RuleFor(o => o.Surname)
        .NotEmpty().WithMessage("Prezime je obvezno polje");
    }
  }
}
