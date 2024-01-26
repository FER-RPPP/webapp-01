using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation
{/// <summary>
 /// Validator class for the <see cref="ProjectPartnerValidator"/> entity using FluentValidation.
 /// </summary>
    public class ProjectPartnerValidator : AbstractValidator<ProjectPartner>
    {
        private readonly Rppp01Context ctx;
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectPartnerValidator"/> class.
        /// </summary>
        public ProjectPartnerValidator(Rppp01Context ctx)
        {
            this.ctx = ctx;

            RuleFor(o => o.ProjectId)
                .NotEmpty().WithMessage("Projekt je obvezno polje");
 
            RuleFor(o => o.RoleId)
                .NotEmpty().WithMessage("Uloga je obvezno polje");
        }
    }
}
