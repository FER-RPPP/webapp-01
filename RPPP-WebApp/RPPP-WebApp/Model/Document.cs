using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class Document
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Name cannot be empty")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Format cannot be empty")]
    public string Format { get; set; }

    [Required(ErrorMessage = "Please select a Project")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "Please select a Document Type")]
    public Guid DocumentTypeId { get; set; }

    public virtual DocumentType DocumentType { get; set; }

    public virtual Project Project { get; set; }
}
