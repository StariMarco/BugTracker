using System;
using BugTracker.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace BugTracker.Data.Repository.IRepository
{
    public interface IProjectRepository : IRepository<Project>
    {
        void Update(Project obj);

        void RemoveProject(IConfiguration configuration, ITicketRepository ticketRepo, Project obj);
    }
}
