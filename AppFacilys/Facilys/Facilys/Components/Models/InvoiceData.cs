namespace Facilys.Components.Models
{
    public class InvoiceData()
    {
        public List<QuotationLine> QuotationLines { get; set; } = new List<QuotationLine>();
        public List<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
        public string GeneralConditionInvoice { get; set; } = string.Empty;
        public string GeneralConditionOrder { get; set; } = string.Empty;
        public bool PartReturnedCustomer { get; set; } = false;
        public bool CustomerSuppliedPart { get; set; } = false;
        public float HT { get; set; } = 0.0f;
        public float TVA { get; set; } = 0.0f;
        public float TTC { get; set; } = 0.0f;
    }

    public class QuotationLine
    {
        public Guid Id { get; } = Guid.NewGuid(); // Identifiant unique stable
        public string LineRef { get; set; } = "";
        public string LineDesc { get; set; } = "";
        public float? LineQt { get; set; } = 0.0f;
        public float? LinePrice { get; set; } = 0.0f;
        public float? LineDisc { get; set; } = 0.0f;
        public float? LineMo { get; set; } = 0.0f;
    }

    public class InvoiceLine
    {
        public Guid Id { get; } = Guid.NewGuid(); // Identifiant unique stable
        public string LineRef { get; set; } = "";
        public string LineDesc { get; set; } = "";
        public float? LineQt { get; set; } = 0.0f;
        public float? LinePrice { get; set; } = 0.0f;
        public float? LineDisc { get; set; } = 0.0f;
        public float? LineMo { get; set; } = 0.0f;
    }
}
