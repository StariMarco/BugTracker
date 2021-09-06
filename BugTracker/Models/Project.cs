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
        [DisplayName("Project type")]
        public string Type { get; set; }

        [Required]
        public string CreatorId { get; set; }

        [ForeignKey("CreatorId")]
        public virtual AppUser Creator { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public override string ToString()
        {
            string result = "Id: " + Id;
            result += "\nTitle: " + Title;
            result += "\nDescription: " + Description;
            result += "\nType: " + Type;
            result += "\nCreatorId: " + CreatorId;
            result += "\nCreatedAt: " + CreatedAt;

            return result;
        }
    }
}
