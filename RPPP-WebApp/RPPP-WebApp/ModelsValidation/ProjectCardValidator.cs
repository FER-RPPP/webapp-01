using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  public class ProjectCardValidator : AbstractValidator<ProjectCard> {

    public ProjectCardValidator() {
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
