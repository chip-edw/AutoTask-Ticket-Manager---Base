using System.ComponentModel.DataAnnotations;

namespace AutoTaskTicketManager_Base.Models
{
    public class CustomerSettings
    {
        [Key]
        public int AutotaskId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public bool EnableEmail { get; set; }
        public bool AutoAssign { get; set; }
    }
}
