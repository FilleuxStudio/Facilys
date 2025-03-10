using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facilys.Components.Models
{
    public class ProfessionalCustomer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("IdClient")]
        public Clients Client { get; set; }
        [Required]
        public string NameCompany { get; set; } = string.Empty;
        [Required]
        public string Siret { get; set; } = string.Empty;
        public string TVANumber { get; set; } = string.Empty;
    }
}
