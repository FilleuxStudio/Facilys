using System.ComponentModel.DataAnnotations;

namespace Facilys.Components.Models
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

        /// <summary>
        /// Convertit l'entité Clients en DTO pour API
        /// </summary>
        public ClientsDto ToDto()
        {
            return new ClientsDto
            {
                Id = this.Id,
                Lname = this.Lname,
                Fname = this.Fname,
                Address = this.Address,
                City = this.City,
                PostalCode = this.PostalCode,
                Type = (int)this.Type,
                AdditionalInformation = this.AdditionalInformation,
                DateCreated = this.DateCreated
            };
        }

    }

    public enum TypeClient
    {
        Client = 0,
        ClientProfessional = 1,
    }

    /// <summary>
    /// DTO pour Clients compatible avec JSON / NodeJS
    /// </summary>
    public class ClientsDto
    {
        public Guid Id { get; set; }

        public string Lname { get; set; } = string.Empty;

        public string Fname { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string PostalCode { get; set; } = string.Empty;

        public int Type { get; set; }

        public string AdditionalInformation { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }
    }
}
