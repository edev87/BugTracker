using BugTracker.Data;
using BugTracker.Data.Enums;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.InteropServices;

namespace BugTracker.Services
{
    public class ProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public ProjectService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        #region Get Project Details
        public async Task<Project> GetProjectDetailsAsync(int? id)
        {
            if (id == null)
            {
                return new Project();
            }

            try
            {
                Project? project = await _context.Projects
                     .Include(p => p.Company)
                     .Include(p => p.ProjectPriority)
                     .Include(p => p.Tickets)
                        .ThenInclude(t => t.History)
                     .Include(p => p.Tickets)
                        .ThenInclude(t => t.DeveloperUser)
                     .Include(p => p.Tickets)
                        .ThenInclude(t => t.SubmitterUser)
                     .Include(p => p.Members)
                     .FirstOrDefaultAsync(m => m.Id == id);

                return project!;

            }
            catch (Exception)
            {

                throw;
            }


        }
        #endregion

        #region Get Project By Id
        public async Task<Project> GetProjectByIdAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = new();

                if (projectId != null && companyId != null)
                {
                    project = await _context.Projects
                        .Include(p => p.ProjectPriority)
                        .Include(p => p.Company)
                        .Include(p => p.Members)
                        .Include(p => p.Tickets)
                        .ThenInclude(t => t.Comments)
                        .ThenInclude(t => t.User)
                         .Include(p => p.Tickets)
                        .ThenInclude(p => p.Attachments)
                         .Include(p => p.Tickets)
                         .ThenInclude(t => t.History)
                         .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                }
                return project!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region Get Project Manager
        public async Task<BTUser> GetProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

                if (project != null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                        {
                            return member;
                        }
                    }
                }


                return null!;
            }
            catch (Exception)
            {

                throw;
            }

        }

        #endregion


        #region Add Project Manager
        public async Task<bool> AddProjectManagerAsync(string? userId, int? projectId)
        {
            try
            {
                if (userId != null && projectId != null)
                {
                    BTUser? currentPM = await GetProjectManagerAsync(projectId);
                    BTUser? selectedPM = await _context.Users.FindAsync(userId);

                    //Remove current PM
                    if (currentPM != null)
                    {
                        await RemoveProjectManagerAsync(projectId);
                    }

                    //Add new PM
                    try
                    {
                        await AddMemberToProjectAsync(selectedPM!, projectId);
                        return true;
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                }


                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion
        public async Task AddProjectAsync(Project project)
        {
			if (project == null) { return; }
			try
			{
				_context.Add(project);
				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}
		}

        public async Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                if (member != null & projectId != null)
                {
                    Project? project = await GetProjectByIdAsync(projectId, member!.CompanyId);

                    if (project != null)
                    {
                        bool isOnProject = project.Members.Any(m => m.Id == member.Id);

                        if (!isOnProject)
                        {
                            project.Members.Add(member);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddMembersToProjectAsync(IEnumerable<string>? userIds, int? projectId, int? companyId)
        {
            try
            {
                if(userIds != null)
                {
                    Project? project = await GetProjectByIdAsync(projectId, companyId);

                    foreach (string userId in userIds)
                    {
                        BTUser? btUser = await _context.Users.FindAsync(userId);

                        if(project != null && btUser != null)
                        {
                            bool IsOnProject = project.Members.Any(m => m.Id == userId);

                            if (!IsOnProject)
                            {
                                project.Members.Add(btUser);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task ArchiveProjectAsync(Project? project, int? companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int? companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects.Where(p => p.CompanyId == companyId)
                    .Include(p => p.Members)
                    .Include(p => p.Tickets)
                    .Include(p => p.ProjectPriority)
                    .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int? companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int? projectId, string? roleName, int? companyId)
        {
            try
            {
                List<BTUser> members = new();

                if(projectId != null && companyId != null && !string.IsNullOrEmpty(roleName))
                {
                Project? project = await GetProjectByIdAsync(projectId, companyId);

                    if(project != null)
                    {
                        foreach (BTUser member in project.Members) //loop over memebrs in the project
                    {
                        if(await _rolesService.IsUserInRoleAsync(member, roleName))
                        {
                            members.Add(member);
                        }
                    }

                        //Testing purposes
                    //List<string> developers = (await _rolesService.GetUsersInRoleAsync("Developer", companyId))?
                    //    .Select(u => u.Id).ToList()!;

                    //List<BTUser> users = project.Members.Where(m => developers.Contains(m.Id)).ToList();

                    }                   
                }

                return members;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>?> GetUserProjectsAsync(string? userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveMembersFromProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                if(member != null & projectId != null)
                {
                    Project? project = await GetProjectByIdAsync(projectId, member.CompanyId);

                    if(project != null)
                    {
                        bool isOnProject = project.Members.Any(m => m.Id == member.Id);

                        if (isOnProject)
                        {
                            project.Members.Remove(member);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        
                    }

                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int? projectId)
        {
            try
            {
                if(projectId != null)
                {
                    Project? project = await _context.Projects.Include(p => p.Members)
                                                            .FirstOrDefaultAsync(p => p.Id == projectId);

                    if(project != null)
                    {
                        foreach(BTUser member in project!.Members)
                        {
                            if(await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                            {
                                //Remove BTUser Project
                                await RemoveMembersFromProjectAsync(member, projectId);
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId)
        {
            throw new NotImplementedException();
        }

        public Task RestoreProjectAsync(Project? project, int? companyId)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateProjectAsync(Project? project)
        {

            if (project == null)
            {
                return;
            }

            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task RemoveMembersFromProjectAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, companyId);

                if(project != null)
                {
                    foreach (var member in project.Members)
                    {
                        if (!await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                                {
                        project.Members.Remove(member);
                                }       
                    }
                    await _context.SaveChangesAsync();
                }
                

            }
            catch (Exception)
            {

                throw;
            }
        }
    }

    }
