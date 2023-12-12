using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  public class TransactionPurposeValidator : AbstractValidator<TransactionPurpose> {
    public TransactionPurposeValidator() {
      RuleFor(o => o.PurposeName)
        .NotEmpty().WithMessage("Svrha je obvezno polje");
    }
  }
}
