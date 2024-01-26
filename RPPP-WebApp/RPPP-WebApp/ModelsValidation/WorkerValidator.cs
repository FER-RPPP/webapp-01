using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation
{  /// <summary>
   /// Validator class for the <see cref="WorkerValidator"/> entity using FluentValidation.
   /// </summary>
    public class WorkerValidator : AbstractValidator<Worker>
    {
        private readonly Rppp01Context ctx;
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerValidator"/> class.
        /// </summary>
        public WorkerValidator(Rppp01Context ctx)
        {
            this.ctx = ctx;

            RuleFor(o => o.Email)
              .NotEmpty().WithMessage("Email je obvezno polje");
            RuleFor(o => o.FirstName)
                .NotEmpty().WithMessage("Ime je obvezno polje");
            RuleFor(o => o.LastName)
                .NotEmpty().WithMessage("Prezime je obvezno polje");
            RuleFor(o => o.PhoneNumber)
                .NotEmpty().WithMessage("Broj telefona je obvezno polje");
            RuleFor(o => o.OrganizationId)
                .NotEmpty().WithMessage("Organizacija je obvezno polje");
        }

    }


}
