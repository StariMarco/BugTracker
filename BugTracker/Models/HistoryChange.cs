using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class HistoryChange
    {
        [Key]
        public int Id { get; set; }

        public string Before { get; set; }

        public string After { get; set; }

        //
        // Foreign Keys
        //

        public int TicketId { get; set; }

        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        [Required]
        public int ActionTypeId { get; set; }

        [ForeignKey("ActionTypeId")]
        public virtual ActionType ActionType { get; set; }
    }
}
