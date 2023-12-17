namespace RPPP_WebApp.ViewModels
{
    public class ProjectPartnerViewModel
    {
        public Guid Id { get; set; }

        public string Project { get; set; }

        public string Worker { get; set; }

        public string Role { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }
}