using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  public class OwnerValidator : AbstractValidator<Owner> {
    private readonly Rppp01Context ctx;
    public OwnerValidator(Rppp01Context ctx) {
      this.ctx = ctx;

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
