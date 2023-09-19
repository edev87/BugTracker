using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTCompanyService:IBTCompanyService
    {
        private readonly ApplicationDbContext _context;
        public BTCompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Company> GetCompanyInfoAsync(int? companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Invite>> GetInvitesAsync(int? companyId)
        {
            throw new NotImplementedException();
        }

        public async  Task<List<BTUser>> GetMembersAsync(int? companyId)
        {
			try
			{
				List<BTUser>? members = new();

				members = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();

				return members;
			}
			catch (Exception)
			{

				throw;
			}
        }

        public Task<List<Project>> GetProjectsAsync(int? companyId)
        {
            throw new NotImplementedException();
        }
    }
}
