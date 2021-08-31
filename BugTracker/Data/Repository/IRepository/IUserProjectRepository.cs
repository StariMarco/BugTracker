using System;
using BugTracker.Models;

namespace BugTracker.Data.Repository.IRepository
{
    public interface IUserProjectRepository : IRepository<UserProject>
    {
        void Update(UserProject obj);
    }
}
