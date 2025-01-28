using System.ComponentModel.DataAnnotations;

namespace Facilys.Components.Models
{
    public class CompanySettings
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string NameCompany { get; set; } = string.Empty;
        [Required]
        public string Logo { get; set; } = string.Empty;
        [Required]
        public string TVA { get; set; } = string.Empty;
        [Required]
        public string Siret { get; set; } = string.Empty;
        [Required]
        public string RIB { get; set; } = string.Empty;
        [Required]
        public string HeadOfficeAddress { get; set; } = string.Empty;
        public string? BillingAddress {  get; set; } = string.Empty;
        [Required]
        public string LegalStatus { get; set; } = string.Empty;
        [Required]
        public string RMNumber { get; set; } = string.Empty;
        public string? RCS { get; set; } = string.Empty;
        public float? RegisteredCapital { get; set; } = 0.00f;
        public string? CodeNAF { get; set; } = string.Empty;
        public string? ManagerName { get; set; } = string.Empty;
        [Required]
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
    }
}
