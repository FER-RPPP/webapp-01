using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation {
  public class TransactionTypeValidator : AbstractValidator<TransactionType> {
    public TransactionTypeValidator() {
      RuleFor(o => o.TypeName)
        .NotEmpty().WithMessage("Vrsta je obvezno polje");
    }
  }
}
