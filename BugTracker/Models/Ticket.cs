using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        //
        // Foreign Keys
        //

        // ProjectId
        [Required]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        // ReporterId
        [Required]
        public string ReporterId { get; set; }

        [ForeignKey("ReporterId")]
        public virtual AppUser Reporter { get; set; }

        // DeveloperId
        public string DeveloperId { get; set; }

        [ForeignKey("DeveloperId")]
        public virtual AppUser Developer { get; set; }

        // ReviewerId
        public string ReviewerId { get; set; }

        [ForeignKey("ReviewerId")]
        public virtual AppUser Reviewer { get; set; }

        // StatusId
        [Required]
        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public virtual TicketStatus Status { get; set; }

        // PriorityId
        [Required]
        public int PriorityId { get; set; }

        [ForeignKey("PriorityId")]
        public virtual TicketPriority Priority { get; set; }

        // TypeId
        [Required]
        public int TypeId { get; set; }

        [ForeignKey("TypeId")]
        public virtual TicketType Type { get; set; }
    }
}
