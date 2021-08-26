using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Status")]
        public string Name { get; set; }
    }
}
