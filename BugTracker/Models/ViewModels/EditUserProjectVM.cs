using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class EditUserProjectVM
    {
        public UserProject UserProject { get; set; }
        public UserProject MyUserProject { get; set; }
        public IEnumerable<SelectListItem> ProjectRoles { get; set; }
    }
}
