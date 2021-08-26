using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
    }
}
