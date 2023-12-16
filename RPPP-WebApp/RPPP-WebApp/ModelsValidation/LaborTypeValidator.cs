using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  public class LaborTypeValidator : AbstractValidator<LaborType> {
    private readonly Rppp01Context ctx;
    public LaborTypeValidator(Rppp01Context ctx) {
      this.ctx = ctx;

      RuleFor(o => o.Type)
        .NotEmpty().WithMessage("Vrsta je obvezno polje")
        .Must(BeUniqueName).WithMessage("Vrsta s ovim nazivom već postoji");
    }

    private bool BeUniqueName(string name) {
      return !this.ctx.LaborType.Any(o => o.Type == name);
    }
  }
}
