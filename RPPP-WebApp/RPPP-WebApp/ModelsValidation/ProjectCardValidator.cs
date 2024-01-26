using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  /// <summary>
  /// Validator class for the <see cref="ProjectCard"/> entity using FluentValidation.
  /// </summary>
  public class ProjectCardValidator : AbstractValidator<ProjectCard> {
    private readonly Rppp01Context ctx;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectCardValidator"/> class.
    /// </summary>
    /// <param name="ctx">The database context.</param>
    public ProjectCardValidator(Rppp01Context ctx) {
      this.ctx = ctx;

      RuleFor(o => o.Iban)
        .NotEmpty().WithMessage("IBAN je obvezno polje");

      RuleFor(o => o.Balance)
        .NotEmpty().WithMessage("Saldo je obvezno polje");

      RuleFor(o => o.ActivationDate)
        .NotEmpty().WithMessage("Datum aktivacije je obvezno polje");

      RuleFor(o => o.Oib)
        .NotEmpty().WithMessage("Vlasnik je obvezno polje");
    }
  }
}
