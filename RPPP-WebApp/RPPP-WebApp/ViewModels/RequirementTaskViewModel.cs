using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class RequirementTaskViewModel
    {
        public Guid Id { get; set; }

        public DateTime PlannedStartDate { get; set; }

        public DateTime PlannedEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        public String TaskStatus { get; set; }

        public String RequirementDescription { get; set; }

        public String ProjectWork { get; set; }

        public static RequirementTaskViewModel FromRequirementTask(RequirementTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            return new RequirementTaskViewModel
            {
                Id = task.Id,
                PlannedStartDate = task.PlannedStartDate,
                PlannedEndDate = task.PlannedEndDate,
                ActualStartDate = task.ActualStartDate,
                ActualEndDate = task.ActualEndDate,
                TaskStatus = task.TaskStatus?.ToString() ?? "Unknown", // Assuming TaskStatus is a string
                RequirementDescription = task.ProjectRequirement?.Description ?? "Unknown", // Assuming this is a string property in RequirementTask
                ProjectWork = task.ProjectWork?.Description ?? "Unknown" // Assuming this is a string property in RequirementTask
            };
        }
    }

    
}
