using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class UserProjects
    {
        [Key]
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        [Key]
        [Required]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project Project { get; set; }

        [Required]
        public int ProjectRoleId { get; set; }

        [ForeignKey("ProjectRoleId")]
        public ProjectRole ProjectRole { get; set; }
    }
}
