using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class SelectUserProjectVM
    {
        public IEnumerable<AppUser> Users { get; set; }
        public UserProject UserProject { get; set; }
        public IEnumerable<SelectListItem> ProjectRoles { get; set; }
        public bool CanAddUser { get; set; }
    }
}
