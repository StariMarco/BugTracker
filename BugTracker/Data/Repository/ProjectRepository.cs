using System;
using System.Linq;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace BugTracker.Data.Repository
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        private readonly ApplicationDbContext _db;

        public ProjectRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void RemoveProject(IConfiguration configuration, ITicketRepository ticketRepo, Project obj)
        {
            // Remove all the UserProjects from the db
            _db.UsersProjects.RemoveRange(_db.UsersProjects.Where(up => up.ProjectId == obj.Id));

            // Remove all the tickets of the project from the db
            var tickets = _db.Tickets.Where(t => t.ProjectId == obj.Id).ToList();
            tickets.ForEach(t => ticketRepo.RemoveTicket(configuration, t));

            // Remove the project from the db
            _db.Projects.Remove(obj);
        }

        public void Update(Project obj)
        {
            _db.Projects.Update(obj);
        }
    }
}
