using System;
using System.Collections.Generic;
using BugTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Data.Repository.IRepository
{
    public interface IProjectRoleRepository : IRepository<ProjectRole>
    {
        void Update(ProjectRole obj);

        public IEnumerable<SelectListItem> GetAllSelectListItems();
    }
}
