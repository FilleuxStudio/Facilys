using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facilys.Components.Models
{
    public class HistoryPart
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("IdInvoice")]
        public Invoices Invoice { get; set; }
        [ForeignKey("IdVehicle")]
        public Vehicles? Vehicle { get; set; }
        [ForeignKey("IdOtherVehicle")]
        public OtherVehicles? OtherVehicle { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        [Required]
        public string? PartName { get; set; } = string.Empty;
        public string? PartBrand { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        [Required]
        public float Price { get; set; } = 0.00f;
        [Required]
        public int Quantity { get; set; } = 0;
        public int? KMMounted { get; set; } = 0;
    }
}
