namespace RPPP_WebApp.ViewModels
{
    public class RequirementTasksViewModel
    {
        public IEnumerable<RequirementTaskViewModel> RequirementTasks { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public RequirementTaskFilter Filter { get; set; }
    }
}
