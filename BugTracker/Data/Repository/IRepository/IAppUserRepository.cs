using System;
using BugTracker.Models;

namespace BugTracker.Data.Repository.IRepository
{
    public interface IAppUserRepository : IRepository<AppUser>
    {
        void Update(AppUser obj);
    }
}
