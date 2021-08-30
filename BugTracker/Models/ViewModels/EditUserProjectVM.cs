using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class EditUserProjectVM
    {
        public Project Project { get; set; }
        public UserProject UserProject { get; set; }
        public IEnumerable<SelectListItem> ProjectRoles { get; set; }
    }
}
