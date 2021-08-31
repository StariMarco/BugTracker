using System;
using BugTracker.Models;

namespace BugTracker.Data.Repository.IRepository
{
    public interface IProjectRepository : IRepository<Project>
    {
        void Update(Project obj);
    }
}
