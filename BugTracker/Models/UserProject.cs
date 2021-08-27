using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class UserProject
    {
        [Key, Column(Order = 0)]
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        [Key, Column(Order = 1)]
        [Required]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [Required]
        public int ProjectRoleId { get; set; }

        [ForeignKey("ProjectRoleId")]
        public virtual ProjectRole ProjectRole { get; set; }
    }
}
