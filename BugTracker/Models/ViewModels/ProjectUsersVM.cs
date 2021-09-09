using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class ProjectUsersVM
    {
        public Project Project { get; set; }
        public IEnumerable<UserProject> UserProjects { get; set; }
        public IEnumerable<SelectListItem> ProjectRoles { get; set; }
        public bool CanEditUsers { get; set; }
    }
}
