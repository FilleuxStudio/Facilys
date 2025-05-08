using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facilys.Components.Models
{
    public class Vehicles
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("IdClient")]
        public Clients Client { get; set; }
        [Required]
        public string Immatriculation { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        [Required]
        public string Mark { get; set; } = string.Empty;
        [Required]
        public string Model { get; set; } = string.Empty;
        [Required]
        public string VIN { get; set; } = string.Empty;
        public string AdditionalInformation { get; set; } = string.Empty;
        [Required]
        public DateTime CirculationDate { get; set; } = DateTime.Now;
        public int KM { get; set; } = 0;
        public StatusData StatusDataView { get; set; } = StatusData.Valid;
        [Required]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        public VehicleDto ToDto()
        {
            return new VehicleDto
            {
                Id = Id,
                ClientId = Client?.Id ?? Guid.Empty,
                Immatriculation = Immatriculation,
                Type = Type,
                Mark = Mark,
                Model = Model,
                VIN = VIN,
                AdditionalInformation = AdditionalInformation,
                CirculationDate = CirculationDate,
                KM = KM,
                StatusDataView = StatusDataView,
                DateAdded = DateAdded
            };
        }
    }

    public class OtherVehicles
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("IdClient")]
        public Clients Client { get; set; }
        [Required]
        public string SerialNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        [Required]
        public string Mark { get; set; } = string.Empty;
        [Required]
        public string Model { get; set; } = string.Empty;
        public StatusData StatusDataView { get; set; } = StatusData.Valid;
        public string? AdditionalInformation { get; set; } = string.Empty;
        [Required]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        public OtherVehicleDto ToDto()
        {
            return new OtherVehicleDto
            {
                Id = Id,
                ClientId = Client?.Id ?? Guid.Empty,
                SerialNumber = SerialNumber,
                Type = Type,
                Mark = Mark,
                Model = Model,
                StatusDataView = StatusDataView,
                AdditionalInformation = AdditionalInformation,
                DateAdded = DateAdded
            };
        }
    }

    public enum StatusData
    {
        Valid = 0,
        Delete = 1,
    }

    public class VehicleDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }

        public string Immatriculation { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Mark { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public string AdditionalInformation { get; set; } = string.Empty;
        public DateTime CirculationDate { get; set; }
        public int KM { get; set; }
        public StatusData StatusDataView { get; set; }
        public DateTime DateAdded { get; set; }
    }

    public class OtherVehicleDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }

        public string SerialNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Mark { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public StatusData StatusDataView { get; set; }
        public string? AdditionalInformation { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }
    }
}
