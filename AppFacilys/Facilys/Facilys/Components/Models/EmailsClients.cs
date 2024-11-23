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
    }
}
