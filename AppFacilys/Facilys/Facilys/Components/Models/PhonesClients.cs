using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facilys.Components.Models
{
    public class PhonesClients
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("IdClient")]
        public Clients Client { get; set; }
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Convertit l'entité vers un DTO JSON-friendly
        /// </summary>
        public PhonesClientsDto ToDto()
        {
            return new PhonesClientsDto
            {
                Id = this.Id,
                ClientId = this.Client?.Id ?? Guid.Empty,
                Phone = this.Phone
            };
        }
    }

    /// <summary>
    /// DTO pour PhonesClients compatible JSON / API
    /// </summary>
    public class PhonesClientsDto
    {
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }

        public string Phone { get; set; } = string.Empty;
    }
}
