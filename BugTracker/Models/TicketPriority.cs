using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketPriority
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Priority")]
        public string Name { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int OrderValue { get; set; }
    }
}
