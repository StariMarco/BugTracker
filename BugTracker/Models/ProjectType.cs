using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class ProjectType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Project Type")]
        public string Type { get; set; }
    }
}
