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

        public DateTime ClosedAt { get; set; }

        //
        // Foreign Keys
        //

        // ProjectId
        [Required]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project Project { get; set; }

        // ReporterId
        [Required]
        public int ReporterId { get; set; }

        [ForeignKey("ReporterId")]
        public AppUser Reporter { get; set; }

        // DeveloperId
        public int DeveloperId { get; set; }

        [ForeignKey("DeveloperId")]
        public AppUser Developer { get; set; }

        // ReviewerId
        public int ReviewerId { get; set; }

        [ForeignKey("ReviewerId")]
        public AppUser Reviewer { get; set; }

        // StatusId
        [Required]
        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public TicketStatus Status { get; set; }

        // PriorityId
        [Required]
        public int PriorityId { get; set; }

        [ForeignKey("PriorityId")]
        public TicketPriority Priority { get; set; }
    }
}
