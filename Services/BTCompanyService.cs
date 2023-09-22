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

        public async Task<Company> GetCompanyInfoAsync(int? companyId)
        {
			if (companyId == null)
			{
				return new Company();
			}

			try
			{
				Company? company = await _context.Companies     //.Where(c => c.Id == companyId)
				   .Include(c => c.Projects)
				   .Include(c => c.Members)
				   .FirstOrDefaultAsync(u => u.Id == companyId);

				return company!;

			}
			catch (Exception)
			{

				throw;
			}

           
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
