using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  /// <summary>
  /// Validator class for the <see cref="Owner"/> entity using FluentValidation.
  /// </summary>
  public class OwnerValidator : AbstractValidator<Owner> {
    private readonly Rppp01Context ctx;
    /// <summary>
    /// Initializes a new instance of the <see cref="OwnerValidator"/> class.
    /// </summary>
    /// <param name="ctx">The database context.</param>
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
