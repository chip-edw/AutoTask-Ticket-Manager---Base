using System.ComponentModel.DataAnnotations;

namespace AutoTaskTicketManager_Base.Models
{
    public class SenderAssignments
    {
        [Key]
        public string AT_Resource_Id { get; set; }

        public string Resource_Name { get; set; }

        public string Resource_Email { get; set; }

        public string Resource_Role { get; set; }

        public bool Resource_Active { get; set; }

    }
}
