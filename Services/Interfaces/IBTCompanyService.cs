using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
    public interface IBTCompanyService
    {
        public Task<Company> GetCompanyInfoAsync(int? companyId);

        public Task<List<BTUser>> GetMembersAsync(int? companyId);

        public Task<List<Project>> GetProjectsAsync(int? companyId);

        public Task<List<Invite>> GetInvitesAsync(int? companyId);
    }
}

