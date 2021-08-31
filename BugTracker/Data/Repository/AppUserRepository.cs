using System;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;

namespace BugTracker.Data.Repository
{
    public class AppUserRepository : Repository<AppUser>, IAppUserRepository
    {
        private readonly ApplicationDbContext _db;

        public AppUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(AppUser obj)
        {
            _db.AppUsers.Update(obj);
        }
    }
}
