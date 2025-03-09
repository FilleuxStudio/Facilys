using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ClassLibraryFacilys.Models
{
    public class MaintenanceAlert
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string TypeMaintenace { get; set; } = string.Empty;

        [ForeignKey("IdVehicle")]
        public Vehicles? Vehicle { get; set; }
        [ForeignKey("IdOtherVehicle")]
        public OtherVehicles? OtherVehicle { get; set; }
        [Required]
        public DateTime DateMake { get; set; } = DateTime.Now.AddYears(2);
        [Required]
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
