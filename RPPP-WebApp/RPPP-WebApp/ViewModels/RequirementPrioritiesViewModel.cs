namespace RPPP_WebApp.ViewModels
{
    public class RequirementPrioritiesViewModel
    {
        public IEnumerable<Model.RequirementPriority> RequirementPriorities { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
