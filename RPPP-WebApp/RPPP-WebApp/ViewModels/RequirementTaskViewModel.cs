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
    }
}
