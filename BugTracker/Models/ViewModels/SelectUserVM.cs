using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class SelectUserVM
    {
        public IEnumerable<AppUser> Users { get; set; }
        public UserProject UserProject { get; set; }
        public IEnumerable<SelectListItem> ProjectRoles { get; set; }
    }
}
