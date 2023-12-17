using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class WorkerViewsModel
    {
        public IEnumerable<WorkerViewModel> Workers { get; set; }

        public PagingInfo PagingInfo { get; set; }

        public WorkerFilter Filter { get; set; }
    }
}
