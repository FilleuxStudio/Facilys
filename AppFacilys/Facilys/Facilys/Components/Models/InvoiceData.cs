namespace Facilys.Components.Models
{
    public class InvoiceData()
    {
        public List<string> LineRef { get; set; } = [];
        public List<string> LineDesc { get; set; } = new();
        public List<float?> LineQt { get; set; } = new();
        public List<float?> LinePrice { get; set; } = new();
        public List<float?> LineDisc { get; set; } = new();
        public List<float?> LineMo { get; set; } = new();
        public string GeneralConditionInvoice { get; set; } = string.Empty;
        public string GeneralConditionOrder { get; set; } = string.Empty;
        public bool PartReturnedCustomer { get; set; } = false;
        public bool CustomerSuppliedPart { get; set; } = false;
        public float HT { get; set; } = 0.0f;
        public float TVA { get; set; } = 0.0f;
        public float TTC { get; set; } = 0.0f;
    }
}
