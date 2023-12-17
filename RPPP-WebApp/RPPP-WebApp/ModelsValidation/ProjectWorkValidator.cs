using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  public class ProjectWorkValidator : AbstractValidator<ProjectWork> {
    private readonly Rppp01Context ctx;

    public ProjectWorkValidator(Rppp01Context ctx) {
      this.ctx = ctx;

      RuleFor(o => o.Title)
        .NotEmpty().WithMessage("Naslov je obvezno polje");

      RuleFor(o => o.ProjectId)
        .NotEmpty().WithMessage("Projekt je obvezno polje");

      RuleFor(o => o.AssigneeId)
        .NotEmpty().WithMessage("Dodijeljeni radnik je obvezno polje");

      RuleFor(o => o.Description)
        .NotEmpty().WithMessage("Opis je obvezno polje");
    }
  }
}
