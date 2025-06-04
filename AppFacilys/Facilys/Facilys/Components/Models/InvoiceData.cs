namespace Facilys.Components.Models
{
    public class InvoiceData()
    {
        public List<QuotationLine> Lines { get; set; } = new List<QuotationLine>();
        public List<string> LineRef { get; set; } = [];
        public List<string> LineDesc { get; set; } = [];
        public List<float?> LineQt { get; set; } = [];
        public List<float?> LinePrice { get; set; } = [];
        public List<float?> LineDisc { get; set; } = [];
        public List<float?> LineMo { get; set; } = [];
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
}
