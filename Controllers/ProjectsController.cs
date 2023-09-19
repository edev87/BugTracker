using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BugTracker.Services.Interfaces;
using BugTracker.Models.ViewModel;
using BugTracker.Data.Enums;
using BugTracker.Services;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace BugTracker.Controllers
{
    public class ProjectsController:BTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;


        public ProjectsController(ApplicationDbContext context, UserManager<BTUser> userManager, IImageService imageService, IBTProjectService projectService, IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _projectService = projectService;
            _rolesService = rolesService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            IEnumerable<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(_companyId);
            return View(projects);
        }

        // GET: Assign PM 

        [HttpGet]
        public async Task<IActionResult> AssignPM(int? id)
         {
            if(id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);

            if(project == null)
            {
                return NotFound();
            }

            //Get list of project managers
            IEnumerable<BTUser> projectManagers = await _rolesService
                                                    .GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), _companyId);

            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id);

            AssignPMViewModel viewModel = new()
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id),
                PMId = currentPM?.Id,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.PMId))
            {
                if (await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.ProjectId))
                {
                    return RedirectToAction(nameof(Details), new { id = viewModel.ProjectId });

                }
                else
                {
                    ModelState.AddModelError("PMId", " Error assigning PM.");
                }
            ModelState.AddModelError("PMId", "No Project Manager Chosen. Please select a PM.");

            }

            IEnumerable<BTUser> projectManagers = await _rolesService
                                                    .GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), _companyId);

            BTUser? currentPM = await _projectService.GetProjectManagerAsync(viewModel.ProjectId);

            viewModel.PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AssignProjectMembers(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);

            if (project == null)
            {
                return NotFound();
            }

            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), _companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), _companyId);

            List<BTUser> usersList = submitters.Concat(developers).ToList();

            IEnumerable<string> currentMembers = project.Members.Select(u => u.Id);

            ProjectMembersViewModel viewModel = new()
            {
                Project = project,
                UsersList = new MultiSelectList(usersList, "Id", "FullName", currentMembers)
            };




                return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignProjectMembers(ProjectMembersViewModel viewModel)
        {
            if(viewModel.SelectedMembers != null)
            {
                await _projectService.RemoveMembersFromProjectAsync(viewModel.Project?.Id, _companyId);
                await _projectService.AddMembersToProjectAsync(viewModel.SelectedMembers,
                    viewModel.Project?.Id, _companyId);

                return RedirectToAction(nameof(Details), new { id = viewModel.Project?.Id });

            }


            viewModel.Project = await _projectService.GetProjectByIdAsync(viewModel.Project?.Id, _companyId);

            //Handle the error
            ModelState.AddModelError("SelectedMemebers", "No Users chosen. Please select Users!");

            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), _companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), _companyId);

            List<BTUser> usersList = submitters.Concat(developers).ToList();

            IEnumerable<string> currentMembers = viewModel.Project!.Members.Select(u => u.Id);
            viewModel.UsersList = new MultiSelectList(usersList, "Id", "FullName", currentMembers);

            return View(viewModel);
        }



        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectDetailsAsync(id);

       
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Created,StartDate,EndDate,Selected,Archived,ProjectPriorityId")] Project project)
        {
            //do we provide projectpriorityId to?
            //get user from user manager and get companyId

            ModelState.Remove("CompanyId");

            if (ModelState.IsValid)
            {
                BTUser? user = await _userManager.GetUserAsync(User);
                //set Company Id get  from user
                project.CompanyId = user!.CompanyId;

                //Set Date
                project.Created = DateTime.Now;

                //Set the Image data if one has been choosen
                if (project.ImageFile != null)
                {
                    //Create the Image Service
                    //1.Convert the field to byte array and assign it to the ImageData
                    project.ImageData = await _imageService.ConvertFileToByteArrayAsync(project.ImageFile);
                    //2.Assign the ImageType based on the choosen file
                    project.ImageType = project.ImageFile.ContentType;
                    // contact.ImageData = contact.ImageFile
                }

                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageData,ImageType,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    BTUser? user = await _userManager.GetUserAsync(User);
                    //set Company Id get  from user
                    project.CompanyId = user!.CompanyId;

                    //project.Updated
                    project.StartDate = DateTime.Now;
                    project.EndDate = DateTime.Now;
                    if (project.ImageFile != null)
                    {
                        //Create the Image Service
                        //1.Convert the field to byte array and assign it to the ImageData
                        project.ImageData = await _imageService.ConvertFileToByteArrayAsync(project.ImageFile);
                       
                        //2.Assign the ImageType based on the choosen file
                        project.ImageType = project.ImageFile.ContentType;
                    }
                   
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }


                 _context.Update(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
          return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);

            if (project == null)
            {
                return NotFound();
            }

            project.Archived = true;
            await _projectService.UpdateProjectAsync(project);

            return RedirectToAction(nameof(Index));


        }

        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);

            if (project == null)
            {
                return NotFound();
            }
            project.Archived = false;
            await _projectService.UpdateProjectAsync(project);

            return RedirectToAction(nameof(Index));

        }
    }
}
