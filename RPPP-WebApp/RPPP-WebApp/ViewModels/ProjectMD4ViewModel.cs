using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
  public class ProjectMD4ViewModel {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Owner { get; set; }

        public string Client { get; set; }

        public string Iban { get; set; }
    }
}
