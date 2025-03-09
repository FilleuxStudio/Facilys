using System.ComponentModel.DataAnnotations;

namespace ClassLibraryFacilys.Models
{
    public class SyncMetaData
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string TableName { get; set; } = string.Empty;
        [Required]
        public TimeSpan LastSyncTime { get; set; } = TimeSpan.Zero;
        public string SyncStatus { get; set; } = string.Empty;
    }
}
