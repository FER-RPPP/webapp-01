using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  public class TransactionValidator : AbstractValidator<Transaction> {

    public TransactionValidator() {
      RuleFor(o => o.Iban)
        .NotEmpty().WithMessage("Pošiljatelj je obvezno polje");

      RuleFor(o => o.Recipient)
        .NotEmpty().WithMessage("Primatelj je obvezno polje");

      RuleFor(o => o.Amount)
       .NotEmpty().WithMessage("Iznos je obvezno polje");

      RuleFor(o => o.Date)
        .NotEmpty().WithMessage("Datum je obvezno polje");

      RuleFor(o => o.Type)
        .NotEmpty().WithMessage("Vrsta transakcije je obvezno polje");

      RuleFor(o => o.Purpose)
        .NotEmpty().WithMessage("Svrha transakcije je obvezno polje");

    }
  }
}
