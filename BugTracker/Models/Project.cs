using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string CreatorId { get; set; }

        [ForeignKey("CreatorId")]
        public virtual AppUser Creator { get; set; }

        [Required]
        public string ProjectManagerId { get; set; }

        [ForeignKey("ProjectManagerId")]
        public virtual AppUser ProjectManager { get; set; }

        [Required]
        public int ProjectTypeId { get; set; }

        [ForeignKey("ProjectTypeId")]
        public virtual ProjectType ProjectType { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
