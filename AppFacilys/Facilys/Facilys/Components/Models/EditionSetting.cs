using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Facilys.Components.Models
{
    public class EditionSetting
    {
        [Key]
        public Guid Id { get; set; } = Guid.Empty;
        [Required]
        public string StartNumberInvoice { get; set; } = string.Empty;
        [Required]
        public string PathSaveFile { get; set; } = string.Empty;
        [Required]
        public string PathSaveInvociePrepare { get; set; } = string.Empty;
        public string? Picture { get; set; } = string.Empty;
        [Required]
        public InvoiceTypeDesign TypeDesign { get; set; }
        public string? SentenceInformationBottom { get; set; } = string.Empty;
        public string? SentenceInformationTop { get; set; } = string.Empty;
        public string? SentenceBottom { get; set; } = string.Empty;
        public string? RepairOrderSentenceTop { get; set; } = string.Empty;
        public string? RepairOrderSentenceBottom { get; set; } = string.Empty;
        public float? TVA { get; set; } = 0.0f;
        [Required]
        public int PreloadedLine {  get; set; } = 2;
        public List<AssociationSettingReference>? AssociationSettingReferences { get; set; } = null;
    }

    public enum InvoiceTypeDesign
    {
        TypeA = 0,
        TypeB = 1,
        TypeC = 2,
    }

    public class AssociationSettingReference
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("IdInterestingReferences")]
        public InterestingReferences References { get; set; }
    }
}
