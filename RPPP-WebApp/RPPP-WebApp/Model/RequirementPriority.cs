﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class RequirementPriority
{
    public Guid Id { get; set; }
    
    [Required]
    [RegularExpression(@"^[a-zA-Z]{1,20}$", ErrorMessage = "Type may only include alphabetic characters and may not be longer than 20 characters!")]
    public string Type { get; set; }

    public virtual ICollection<ProjectRequirement> ProjectRequirement { get; set; } = new List<ProjectRequirement>();
}
