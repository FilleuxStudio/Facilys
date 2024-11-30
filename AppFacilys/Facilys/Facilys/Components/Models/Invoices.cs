using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Facilys.Components.Models
{
    public class Invoices
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;
        [Required]
        public string OrderNumber { get; set; } = string.Empty;
        [ForeignKey("IdVehicle")]
        public Vehicles? Vehicle { get; set; }
        [ForeignKey("IdOtherVehicle")]
        public OtherVehicles? OtherVehicle { get; set; }
        [Required]
        public PaymentMethod Payment { get; set; }
        public float TotalAmount { get; set; } = 0.00f;
        public string? Observations { get; set; } = string.Empty;
        public string? RepairType { get; set; } = string.Empty;
        public string Status { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }

    public enum PaymentMethod
    {
        CashPayment = 0,
        BankCards = 1,
        BankTransfers = 2,
        OnlinePaymentSolutions = 3,
        MobilePayment = 4,
        PaymentByCheck = 5,
        Cryptocurrencies = 6
    }
}
