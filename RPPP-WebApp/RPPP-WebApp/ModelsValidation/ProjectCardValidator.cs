using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  public class ProjectCardValidator : AbstractValidator<ProjectCard> {
    private readonly Rppp01Context ctx;

    public ProjectCardValidator(Rppp01Context ctx) {
      this.ctx = ctx;

      RuleFor(o => o.Iban)
        .NotEmpty().WithMessage("IBAN je obvezno polje")
        .Must(BeUniqueIban).WithMessage("Projektna kartica s ovim Iban-om već postoji");

      RuleFor(o => o.Balance)
        .NotEmpty().WithMessage("Saldo je obvezno polje");

      RuleFor(o => o.ActivationDate)
        .NotEmpty().WithMessage("Datum aktivacije je obvezno polje");

      RuleFor(o => o.Oib)
        .NotEmpty().WithMessage("Vlasnik je obvezno polje");
    }

    private bool BeUniqueIban(string iban) {
      return !this.ctx.ProjectCard.Any(o => o.Iban == iban);
    }
  }
}
