using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class DocumentType
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Document> Document { get; set; } = new List<Document>();
}
