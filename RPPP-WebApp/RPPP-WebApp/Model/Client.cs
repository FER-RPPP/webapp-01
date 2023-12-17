using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace RPPP_WebApp.Model;


public partial class Client
{
    public Guid Id { get; set; }

    [Required]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "OIB must be exactly 11 digits.")]
    public string Oib { get; set; }

    [Required]
    [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "IBAN must contain only uppercase letters and numbers.")]
    public string Iban { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    [Required]
    [RegularExpression(@"^\p{Lu}\p{Ll}+$", ErrorMessage = "First name must start with an uppercase letter and be followed by lowercase letters.")]
    public string FirstName { get; set; }

    [Required]
    [RegularExpression(@"^\p{Lu}\p{Ll}+$", ErrorMessage = "Last name must start with an uppercase letter and be followed by lowercase letters.")]
    public string LastName { get; set; }

    public virtual ICollection<Project> Project { get; set; } = new List<Project>();
}
