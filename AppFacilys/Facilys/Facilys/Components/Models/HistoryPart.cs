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
        public Guid IdInvoice { get; set; }
        [ForeignKey("IdVehicle")]
        public Vehicles? Vehicle { get; set; }
        public Guid? IdVehicle { get; set; } = null;
        [ForeignKey("IdOtherVehicle")]
        public OtherVehicles? OtherVehicle { get; set; }
        public Guid? IdOtherVehicle { get; set; } = null;
        public string PartNumber { get; set; } = string.Empty;
        [Required]
        public string? PartName { get; set; } = string.Empty;
        public string? PartBrand { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public float Discount { get; set; } = 0.0f;
        [Required]
        public float Price { get; set; } = 0.00f;
        [Required]
        public float Quantity { get; set; } = 0.0f;
        public int? KMMounted { get; set; } = 0;

        public HistoryPartDto ToDto()
        {
            return new HistoryPartDto
            {
                Id = this.Id,
                InvoiceId = this.Invoice?.Id ?? Guid.Empty,
                VehicleId = this.Vehicle?.Id,
                OtherVehicleId = this.OtherVehicle?.Id,
                PartNumber = this.PartNumber,
                PartName = this.PartName,
                PartBrand = this.PartBrand,
                Description = this.Description,
                Discount = this.Discount,
                Price = this.Price,
                Quantity = this.Quantity,
                KMMounted = this.KMMounted
            };
        }
    }

    public class HistoryPartDto
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid? VehicleId { get; set; }
        public Guid? OtherVehicleId { get; set; }

        public string PartNumber { get; set; } = string.Empty;
        public string? PartName { get; set; } = string.Empty;
        public string? PartBrand { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;

        public float Discount { get; set; } = 0.0f;
        public float Price { get; set; } = 0.0f;
        public float Quantity { get; set; } = 0.0f;
        public int? KMMounted { get; set; }
    }
}
