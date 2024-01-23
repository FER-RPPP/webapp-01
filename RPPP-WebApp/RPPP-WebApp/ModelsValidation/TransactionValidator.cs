using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  /// <summary>
  /// Validator class for the <see cref="Transaction"/> entity using FluentValidation.
  /// </summary>
  public class TransactionValidator : AbstractValidator<Transaction> {

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionValidator"/> class.
    /// </summary>
    public TransactionValidator() {
      RuleFor(o => o.Iban)
        .NotEmpty().WithMessage("Pošiljatelj je obvezno polje");

      RuleFor(o => o.Recipient)
        .NotEmpty().WithMessage("Primatelj je obvezno polje");

      RuleFor(o => o.Amount)
       .NotEmpty().WithMessage("Iznos je obvezno polje");

      RuleFor(o => o.Date)
        .NotEmpty().WithMessage("Datum je obvezno polje");

      RuleFor(o => o.TypeId)
        .NotEmpty().WithMessage("Vrsta transakcije je obvezno polje");

      RuleFor(o => o.PurposeId)
        .NotEmpty().WithMessage("Svrha transakcije je obvezno polje");

    }
  }
}
