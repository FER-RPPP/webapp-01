using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation
{
    public class ProjectPartnerValidator : AbstractValidator<ProjectPartner>
    {
        private readonly Rppp01Context ctx;
        public ProjectPartnerValidator(Rppp01Context ctx)
        {
            this.ctx = ctx;

            RuleFor(o => o.ProjectId)
                .NotEmpty().WithMessage("Projekt je obvezno polje");
            RuleFor(o => o.WorkerId)
                .NotEmpty().WithMessage("Radnik je obvezno polje");
            RuleFor(o => o.RoleId)
                .NotEmpty().WithMessage("Uloga je obvezno polje");
        }
    }
}
