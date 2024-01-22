using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class ProjectDocumentsViewModel
    {
        public IEnumerable<DocumentViewModel> Document { get; set; }
        public Document NewDocument { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public Project Project { get; set; }
    }
}
