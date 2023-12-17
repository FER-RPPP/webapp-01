using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation
{
    public class OrganizationValidator : AbstractValidator<Organization>
    {
        private readonly Rppp01Context ctx;
        public OrganizationValidator(Rppp01Context ctx)
        {
            this.ctx = ctx;

            RuleFor(o => o.Name)
              .NotEmpty().WithMessage("Naziv organizacije je obvezno polje")
              .Must(BeUniqueName).WithMessage("Organizacija s ovim nazivom već postoji");
        }

        private bool BeUniqueName(string name)
        {
            return !this.ctx.Organization.Any(o => o.Name == name);
        }
    }
}
