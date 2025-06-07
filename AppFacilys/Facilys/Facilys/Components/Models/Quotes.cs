using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facilys.Components.Models
{
    public class Quotes
    {

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("IdClient")]
        public Clients Client { get; set; }
        public Guid IdClient { get; set; }
        [ForeignKey("IdVehicle")]
        public Vehicles? Vehicle { get; set; }
        public Guid? IdVehicle { get; set; } = Guid.Empty;
        [ForeignKey("IdOtherVehicle")]
        public OtherVehicles? OtherVehicle { get; set; }
        public Guid? IdOtherVehicle { get; set; } = Guid.Empty;
        [Required]
        public string QuoteNumber { get; set; } = string.Empty;
        public float? TotalAmount { get; set; } = 0.00f;
        public string? Observations { get; set; } = string.Empty;
        public StatusQuote Status { get; set; } = StatusQuote.waiting;
        public Users? User { get; set; }
        public DateTime? DateAccepted { get; set; } = DateTime.Now;
        [Required]
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }

    public enum StatusQuote
    {
        waiting = 0,
        accept = 1,
        refuse = 2
    }

    public class QuotesItems
    {

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("IdQuote")]
        public Quotes Quote { get; set; }
        public Guid IdQuote { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        [Required]
        public string? PartName { get; set; } = string.Empty;
        public string? PartBrand { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public float Price { get; set; } = 0.00f;
        public int Quantity { get; set; } = 0;
    }
}
