using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InvoiceGenerator.Data;

namespace InvoiceGenerator.Models;

public class Invoice
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Invoice Number")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Partner Name")]
    public string PartnerName { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Partner Street")]
    public string? PartnerStreet { get; set; }

    [StringLength(250)]
    [Display(Name = "Partner ZIP, City, Country")]
    [RegularExpression(@".*,.*,.*", ErrorMessage = "Please use the format: ZIP, City, Country")]
    public string? PartnerLocation { get; set; }

    [Required]
    [StringLength(500)]
    [Display(Name = "Service Description")]
    public string ServiceDescription { get; set; } = string.Empty;

    [Range(1, 10000, ErrorMessage = "Hours must be between 1 and 10,000")]
    [Display(Name = "Hours")]
    public int ServiceHours { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be greater than 0")]
    [Display(Name = "Price per Hour")]
    public decimal ServiceSinglePrice { get; set; }

    [NotMapped]
    [Display(Name = "Total Amount")]
    public decimal TotalAmount => ServiceHours * ServiceSinglePrice;

    [StringLength(50)]
    [Display(Name = "Payment Method")]
    public string? PaymentMethod { get; set; } = "Main Bank";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    
    public string? UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }
}