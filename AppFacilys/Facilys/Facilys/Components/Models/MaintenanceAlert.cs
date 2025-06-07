using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facilys.Components.Models
{
    public class MaintenanceAlert
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string TypeMaintenace { get; set; } = string.Empty;

        [ForeignKey("IdVehicle")]
        public Vehicles? Vehicle { get; set; }
        public Guid? IdVehicle { get; set; } = Guid.Empty;
        [ForeignKey("IdOtherVehicle")]
        public OtherVehicles? OtherVehicle { get; set; }
        public Guid? IdOtherVehicle { get; set; } = Guid.Empty;
        [Required]
        public DateTime DateMake { get; set; } = DateTime.Now.AddYears(2);
        [Required]
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
