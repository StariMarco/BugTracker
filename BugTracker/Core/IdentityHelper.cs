using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;

namespace BugTracker.Core
{
    public static class IdentityHelper
    {
        public static bool IsProjectManager(UserProject userProject, string userId, int projectId)
        {
            return userProject.UserId == userId
                && userProject.ProjectId == projectId
                && userProject.ProjectRoleId == WC.ProjectManagerId;
        }

        public static UserProject GetUserProject(IUserProjectRepository userProjectRepo, bool isAdmin, int projectId, string userId, string includeProperties = "Project")
        {
            Expression<Func<UserProject, bool>> filter = isAdmin ? ((UserProject u) => u.ProjectId == projectId) : ((UserProject u) => u.ProjectId == projectId && u.UserId == userId);
            var userProject = userProjectRepo.FirstOrDefault(filter, includeProperties);
            if (isAdmin) userProject.ProjectRoleId = 0;

            userProject.ProjectRole = new ProjectRole
            {
                Id = userProject.ProjectRoleId,
                Name = WC.ProjectRolesMap[userProject.ProjectRoleId]
            };

            return userProject;
        }
    }
}
