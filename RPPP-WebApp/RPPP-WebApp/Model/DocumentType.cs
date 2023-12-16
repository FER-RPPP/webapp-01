using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class DocumentType
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Name cannot be empty")]
    public string Name { get; set; }

    public virtual ICollection<Document> Document { get; set; } = new List<Document>();
}
