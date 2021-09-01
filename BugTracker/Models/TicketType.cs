using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Type")]
        public string Name { get; set; }
    }
}
