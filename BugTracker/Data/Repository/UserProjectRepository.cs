using System;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;

namespace BugTracker.Data.Repository
{
    public class UserProjectRepository : Repository<UserProject>, IUserProjectRepository
    {
        private readonly ApplicationDbContext _db;

        public UserProjectRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(UserProject obj)
        {
            _db.UsersProjects.Update(obj);
        }
    }
}
