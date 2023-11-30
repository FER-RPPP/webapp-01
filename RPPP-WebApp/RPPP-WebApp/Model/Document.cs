using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Document
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Format { get; set; }

    public Guid ProjectId { get; set; }

    public Guid DocumentTypeId { get; set; }

    public virtual DocumentType DocumentType { get; set; }

    public virtual Project Project { get; set; }
}
