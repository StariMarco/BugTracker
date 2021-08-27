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
        public Ticket Ticket { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project Project { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        [Required]
        public int ActionTypeId { get; set; }

        [ForeignKey("ActionTypeId")]
        public ActionType ActionType { get; set; }
    }
}
