using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  public class TransactionPurposeValidator : AbstractValidator<TransactionPurpose> {
    private readonly Rppp01Context ctx;
    public TransactionPurposeValidator(Rppp01Context ctx) {
      this.ctx = ctx;

      RuleFor(o => o.PurposeName)
        .NotEmpty().WithMessage("Svrha je obvezno polje")
        .Must(BeUniqueName).WithMessage("Svrha s ovim nazivom već postoji");
    }

    private bool BeUniqueName(string name) {
      return !this.ctx.TransactionPurpose.Any(o => o.PurposeName == name);
    }
  }
}
