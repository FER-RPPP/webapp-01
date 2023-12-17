using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  public class ProjectMD4Validator : AbstractValidator<Project> {
    private readonly Rppp01Context ctx;
    public ProjectMD4Validator(Rppp01Context ctx) {
      this.ctx = ctx;

      RuleFor(o => o.Name)
        .NotEmpty().WithMessage("Ime je obvezno polje");

      RuleFor(o => o.Type)
        .NotEmpty().WithMessage("Vrsta je obvezno polje");

      RuleFor(o => o.OwnerId)
        .NotEmpty().WithMessage("Vlasnik je obvezno polje");

      RuleFor(o => o.ClientId)
        .NotEmpty().WithMessage("Klijent je obvezno polje");

      RuleFor(o => o.CardId)
        .NotEmpty().WithMessage("IBAN je obvezno polje");
    }
  }
}
