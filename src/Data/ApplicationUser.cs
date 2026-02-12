using System.ComponentModel.DataAnnotations;
using InvoiceGenerator.Models;
using Microsoft.AspNetCore.Identity;

namespace InvoiceGenerator.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Company Name")]
    public string? CompanyName { get; set; }
    
    public List<Invoice> Invoices { get; set; } = new();
}