using Facilys.Components.Pages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public StatusInvoice Status { get; set; } = StatusInvoice.OnHold;
        public bool PartReturnedCustomer { get; set; } = false;
        public bool CustomerSuppliedPart { get; set; } = false;
        public Users? User { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;

        public InvoiceDto ToDto()
        {
            return new InvoiceDto
            {
                Id = this.Id,
                InvoiceNumber = this.InvoiceNumber,
                OrderNumber = this.OrderNumber,
                VehicleId = this.Vehicle?.Id,
                OtherVehicleId = this.OtherVehicle?.Id,
                Payment = this.Payment,
                TotalAmount = this.TotalAmount,
                Observations = this.Observations,
                RepairType = this.RepairType,
                Status = this.Status,
                PartReturnedCustomer = this.PartReturnedCustomer,
                CustomerSuppliedPart = this.CustomerSuppliedPart,
                UserId = this.User?.Id,
                DateAdded = this.DateAdded
            };
        }
    }

    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public Guid? VehicleId { get; set; }
        public Guid? OtherVehicleId { get; set; }
        public PaymentMethod Payment { get; set; }
        public float TotalAmount { get; set; }
        public string? Observations { get; set; }
        public string? RepairType { get; set; }
        public StatusInvoice Status { get; set; }
        public bool PartReturnedCustomer { get; set; }
        public bool CustomerSuppliedPart { get; set; }
        public Guid? UserId { get; set; }
        public DateTime DateAdded { get; set; }
    }


    public enum PaymentMethod
    {
        CashPayment = 0,
        BankCards = 1,
        BankTransfers = 2,
        OnlinePaymentSolutions = 3,
        MobilePayment = 4,
        PaymentByCheck = 5,
        Cryptocurrencies = 6,
        NotInformed = 7,
    }

    public enum StatusInvoice
    {
        Validate = 0,
        OnHold = 1,
        Canceled = 2,
        Delete = 3,
        Postponed = 4,
    }
}
