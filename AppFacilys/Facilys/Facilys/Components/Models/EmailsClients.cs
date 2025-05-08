using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facilys.Components.Models
{
    public class EmailsClients
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey("IdClient")]
        public Clients Client { get; set; }

        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Convertit l'entité vers un DTO JSON-friendly
        /// </summary>
        public EmailsClientsDto ToDto()
        {
            return new EmailsClientsDto
            {
                Id = this.Id,
                ClientId = this.Client?.Id ?? Guid.Empty,
                Email = this.Email
            };
        }
    }

    /// <summary>
    /// DTO pour EmailsClients compatible JSON / API
    /// </summary>
    public class EmailsClientsDto
    {
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }

        public string Email { get; set; } = string.Empty;
    }
}
