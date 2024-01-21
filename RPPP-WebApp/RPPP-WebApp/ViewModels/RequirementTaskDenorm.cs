namespace RPPP_WebApp.ViewModels
{
    public class RequirementTaskDenorm
    {
        public Guid ProjectRequirementId { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime ActualStartDate { get; set; }
        public DateTime ActualEndDate { get; set; }
        public string TaskStatus { get; set; }
        public string ProjectWorkTitle { get; set; }
        public string ProjectName { get; set; }

    }
}
