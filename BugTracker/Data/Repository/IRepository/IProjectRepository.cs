using System;
using BugTracker.Models;
using Microsoft.AspNetCore.Hosting;

namespace BugTracker.Data.Repository.IRepository
{
    public interface IProjectRepository : IRepository<Project>
    {
        void Update(Project obj);

        void RemoveProject(IWebHostEnvironment web, ITicketRepository ticketRepo, Project obj);
    }
}
