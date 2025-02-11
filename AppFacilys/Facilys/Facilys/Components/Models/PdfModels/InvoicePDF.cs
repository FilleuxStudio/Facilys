namespace Facilys.Components.Models.PdfModels
{
    public class InvoicePDF
    {
        public class Invoice
        {
            public string InvoiceNumber { get; set; } // Ex: L0280
            public DateTime Date { get; set; } // Ex: 05-02-2025
            public int Kilometers { get; set; } // Ex: 45665
            public Customer Customer { get; set; }
            public Vehicle Vehicle { get; set; }
            public List<ServiceItem> Services { get; set; }
            public decimal SubTotal => Services.Sum(s => s.Total);
            public decimal TaxRate => 0.20m; // Taux de TVA à 20%
            public decimal TaxAmount => SubTotal * TaxRate;
            public decimal TotalAmount => SubTotal + TaxAmount;
        }

        public class Customer
        {
            public string LastName { get; set; } // Ex: AMADO
            public string FirstName { get; set; } // Ex: MARIA
            public string Address { get; set; } // Ex: 7 RUE DU BAS DU PARC
            public string PostalCode { get; set; } // Ex: 60840
            public string City { get; set; } // Ex: NOINTEL
            public string Phone { get; set; } // Ex: 0682216128
        }

        public class Vehicle
        {
            public string Brand { get; set; } // Ex: CITROEN
            public string Model { get; set; } // Ex: C3 1.4 I 75
            public string Registration { get; set; } // Ex: EV-123-YT
            public string VIN { get; set; } // Ex: VF7FCKFVB27027891
            public string Type { get; set; } // Ex: MCT1102PH843
            public DateTime ReleaseDate { get; set; } // Ex: 12/05/2004
        }

        public class ServiceItem
        {
            public string Description { get; set; } // Ex: CREPINE DE BOITE AUTO
            public int Quantity { get; set; } // Ex: 1
            public decimal UnitPrice { get; set; } // Ex: 50,00
            public decimal Total => Quantity * UnitPrice;
        }
    }
}
