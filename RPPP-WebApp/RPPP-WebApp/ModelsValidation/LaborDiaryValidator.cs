using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  public class LaborDiaryValidator : AbstractValidator<LaborDiary> {

    public LaborDiaryValidator() {
      RuleFor(o => o.Date)
        .NotEmpty().WithMessage("Datum je obvezno polje");

      RuleFor(o => o.WorkId)
        .NotEmpty().WithMessage("Posao je obvezno polje");

      RuleFor(o => o.WorkerId)
        .NotEmpty().WithMessage("Radnik je obvezno polje");

      RuleFor(o => o.HoursSpent)
       .NotEmpty().WithMessage("Utrošeni sati su obvezno polje");

      RuleFor(o => o.LaborTypeId)
        .NotEmpty().WithMessage("Vrsta posla je obvezno polje");

      RuleFor(o => o.LaborDescription)
        .NotEmpty().WithMessage("Opis posla je obvezno polje");

    }
  }
}
