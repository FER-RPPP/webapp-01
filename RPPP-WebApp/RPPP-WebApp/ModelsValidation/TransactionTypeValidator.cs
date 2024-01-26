using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  /// <summary>
  /// Validator class for the <see cref="TransactionType"/> entity using FluentValidation.
  /// </summary>
  public class TransactionTypeValidator : AbstractValidator<TransactionType> {
    private readonly Rppp01Context ctx;
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionTypeValidator"/> class.
    /// </summary>
    /// <param name="ctx">The database context.</param>
    public TransactionTypeValidator(Rppp01Context ctx) {
      this.ctx = ctx;

      RuleFor(o => o.TypeName)
        .NotEmpty().WithMessage("Vrsta je obvezno polje")
        .Must(BeUniqueName).WithMessage("Vrsta s ovim nazivom već postoji");
    }

    private bool BeUniqueName(string name) {
      return !this.ctx.TransactionType.Any(o => o.TypeName == name);
    }
  }
}
