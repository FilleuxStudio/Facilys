using System.ComponentModel.DataAnnotations;

namespace ClassLibraryFacilys.Models
{
    public class Clients
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Lname { get; set; } = string.Empty;
        [Required]
        public string Fname { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        [Required]
        public string PostalCode { get; set; } = string.Empty;
        [Required]
        public TypeClient Type { get; set; } = TypeClient.Client;
        [Required]
        public string AdditionalInformation { get; set; } = string.Empty;
        [Required]
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }

    public enum TypeClient
    {
        Client = 0,
        ClientProfessional = 1,
    }
}
