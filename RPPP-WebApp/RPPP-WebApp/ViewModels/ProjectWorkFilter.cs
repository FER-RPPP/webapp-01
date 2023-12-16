using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace RPPP_WebApp.ViewModels {
  public class ProjectWorkFilter {

    public string Title { get; set; }
    public string Assignee { get; set; }
    public string Project { get; set; }

  }
}
