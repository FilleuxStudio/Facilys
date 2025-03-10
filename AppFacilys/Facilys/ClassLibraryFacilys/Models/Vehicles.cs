﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassLibraryFacilys.Models
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
    }

    public enum StatusData
    {
        Valid = 0,
        Delete = 1,
    }
}
