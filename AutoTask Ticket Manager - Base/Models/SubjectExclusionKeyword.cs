﻿namespace AutoTaskTicketManager_Base.Models
{
    public class SubjectExclusionKeyword
    {
        public int Id { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
