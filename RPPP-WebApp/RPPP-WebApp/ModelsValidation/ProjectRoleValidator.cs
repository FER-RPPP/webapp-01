using FluentValidation;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ModelsValidation
{/// <summary>
 /// Validator class for the <see cref="ProjectRoleValidator"/> entity using FluentValidation.
 /// </summary>
    public class ProjectRoleValidator : AbstractValidator<ProjectRole>
    {
        private readonly Rppp01Context ctx;
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectRoleValidator"/> class.
        /// </summary>
        public ProjectRoleValidator(Rppp01Context ctx)
        {
            this.ctx = ctx;

            RuleFor(o => o.Name)
              .NotEmpty().WithMessage("Naziv uloge je obvezno polje")
              .Must(BeUniqueName).WithMessage("Uloga s ovim nazivom već postoji");
        }

        private bool BeUniqueName(string name)
        {
            return !this.ctx.ProjectRole.Any(o => o.Name == name);
        }
    }
}
