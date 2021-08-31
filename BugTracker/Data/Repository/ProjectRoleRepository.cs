using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Data.Repository
{
    public class ProjectRoleRepository : Repository<ProjectRole>, IProjectRoleRepository
    {
        private readonly ApplicationDbContext _db;

        public ProjectRoleRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ProjectRole obj)
        {
            _db.ProjectRoles.Update(obj);
        }

        public IEnumerable<SelectListItem> GetAllSelectListItems()
        {
            return _db.ProjectRoles.Select((ProjectRole i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
        }
    }
}
