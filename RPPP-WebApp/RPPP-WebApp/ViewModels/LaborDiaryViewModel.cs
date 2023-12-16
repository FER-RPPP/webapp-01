using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
  public class LaborDiaryViewModel {
        public Guid Id { get; set; }

        public string Work { get; set; }

        public string Worker { get; set; }

        public DateTime Date { get; set; }

        public decimal HoursSpent { get; set; }

        public string LaborType { get; set; }

        public string LaborDescription { get; set; }
    }
}
