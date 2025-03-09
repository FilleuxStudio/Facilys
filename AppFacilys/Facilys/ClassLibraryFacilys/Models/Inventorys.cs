using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ClassLibraryFacilys.Models
{
    public class Inventorys
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Reference { get; set; } = string.Empty;
        [Required]
        public string PartName { get; set; } = string.Empty;
        public string Mark { get; set; } = string.Empty;
        public string? Details { get; set; } = string.Empty;
        public string? Picture { get; set; } = string.Empty;
        public string? Type { get; set; } = string.Empty;
        public float? Price { get; set; } = 0.00f;
        public int? Quantity { get; set; } = 0;
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
