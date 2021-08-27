using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class ProjectRole
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Project Role")]
        public string Name { get; set; }
    }
}
