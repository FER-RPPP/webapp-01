using RPPP_WebApp.Model;
using TaskStatus = RPPP_WebApp.Model.TaskStatus;

namespace RPPP_WebApp.ViewModels
{
    public class TaskStatusesViewModel
    {
        public IEnumerable<TaskStatus> TaskStatuses { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
