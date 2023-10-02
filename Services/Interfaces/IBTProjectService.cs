using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
    public interface IBTProjectService
    {
        public Task<Project> GetProjectDetailsAsync(int? id);

        public Task<Project> GetProjectByIdAsync(int? project, int? companyId);

        public Task<BTUser> GetProjectManagerAsync(int? projectId);

        public Task<bool> AddProjectManagerAsync(string? userId, int? projectId);

        public Task AddProjectAsync(Project project);
        public Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId);
        public Task AddMembersToProjectAsync(IEnumerable<string>? userIds, int? projectId, int? companyId);
        public Task ArchiveProjectAsync(Project? project, int? companyId);
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int? companyId);
        public Task<IEnumerable<Project>> GetArchivedProjectsByCompanyIdAsync(int? companyId);

       // public Task<IEnumerable<Project>> GetUnAssignedProjectsByCompanyIdAsync(int? companyId);
        public Task<List<BTUser>> GetProjectMembersByRoleAsync(int? projectId, string? roleName, int? companyId);

        public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync();
        public Task<List<Project>?> GetUserProjectsAsync(string? userId);
        public Task RemoveMembersFromProjectAsync(int? projectId, int? companyId);

        public Task RemoveProjectManagerAsync(int? projectId);
        public Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId);
        public Task RestoreProjectAsync(Project? project, int? companyId);
        public Task UpdateProjectAsync(Project? project);



    }
}
