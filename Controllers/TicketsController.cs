using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BugTracker.Services.Interfaces;
using System.Text.RegularExpressions;
using BugTracker.Models.ViewModel;
using BugTracker.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using BugTracker.Extensions;
using BugTracker.Services;

namespace BugTracker.Controllers
{
    public class TicketsController : BTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
		private readonly IBTFileService _fileService;
		private readonly IBTTicketService _ticketService;
        private readonly IBTTicketHistoryService _historyService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTRolesService _rolesService;




        public TicketsController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTProjectService projectService, IBTFileService fileService, IBTTicketService ticketService, IBTTicketHistoryService historyService, IBTNotificationService bTNotificationService, IBTRolesService rolesSerivce)
        {
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
            _fileService = fileService;
            _ticketService = ticketService;
            _historyService = historyService;
            _notificationService = bTNotificationService;
            _rolesService = rolesSerivce;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {

            //var applicationDbContext = _context.Tickets
            //    .Include(t => t.DeveloperUser).Include(t => t.Project).
            //    Include(t => t.SubmitterUser).Include(t => t.TicketPriority)
            //    .Include(t => t.TicketStatus).Include(t => t.TicketType);

            IEnumerable<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(_companyId);

            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> AssignTicket(int? id)
        {
            if( id == null)
            {
                return NotFound();
            }


            AssignTicketViewModel viewModel = new();

            viewModel.Ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);

            string? currentDeveloper = viewModel.Ticket?.DeveloperUserId;

            viewModel.Developers = new SelectList(await _projectService
            .GetProjectMembersByRoleAsync(viewModel.Ticket?.ProjectId,
                                          nameof(BTRoles.Developer),
                                          _companyId), "Id", "FullName", currentDeveloper);

            


            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicket(AssignTicketViewModel viewModel)
        {
            if(viewModel.DeveloperId != null && viewModel.Ticket?.Id != null)
            {

                //TODO: Add History (will add for create and edit as well)

                string? userId = _userManager.GetUserId(User);

                //Get AsNoTracking
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket?.Id, _companyId); 

                //Ticket Assignment
                try
                {
                    await _ticketService.AssignTicketAsync(viewModel.Ticket?.Id, viewModel.DeveloperId);

                    // Add Ticket History
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket?.Id, _companyId);

                    await _historyService.AddHistoryAsync(oldTicket, newTicket, userId);


                    return RedirectToAction(nameof(Details), new {id = viewModel.Ticket?.Id});

                }
                catch (Exception)
                {

                    throw;
                }

                

                //TODO: Add Notification (will add for create and edit as well)
            }

            if (viewModel.Ticket?.Id == null)
            {
                return NotFound();
            }


            viewModel.Ticket = await _ticketService.GetTicketByIdAsync(viewModel.Ticket?.Id, _companyId);

            string? currentDeveloper = viewModel.Ticket?.DeveloperUserId;

            viewModel.Developers = new SelectList(await _projectService
            .GetProjectMembersByRoleAsync(viewModel.Ticket?.ProjectId,
                                          nameof(BTRoles.Developer),
                                          _companyId), "Id", "FullName", currentDeveloper);


            return View(viewModel);
        }


        [Authorize(Roles = "Admin")]
        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .Include(t => t.History)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }
        [Authorize(Roles = "Admin")]
        //POST
        [HttpPost]
        public async Task<IActionResult> AddTicketComment([Bind("UserId, TicketId, Comment")] TicketComment ticketComment)
        {
            
            if (ModelState.IsValid)
            {
                ticketComment.UserId =  _userManager.GetUserId(User);
                ticketComment.Created = DateTime.Now;
                

                

                _context.Add(ticketComment);
                await _context.SaveChangesAsync();

           //     return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            }
            return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            // return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
		{
			string statusMessage;

            ModelState.Remove("BTUserId");

			if (ModelState.IsValid && ticketAttachment.FormFile != null)
			{
				ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
				ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
				ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

				ticketAttachment.Created = DateTime.Now;
				ticketAttachment.BTUserId = _userManager.GetUserId(User);

				await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
				statusMessage = "Success: New attachment added to Ticket.";
			}
			else
			{
				statusMessage = "Error: Invalid data.";

			}

			return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
		}

		public async Task<IActionResult> ShowFile(int id)
		{
			TicketAttachment? ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
			string fileName = ticketAttachment?.FileName!;
			byte[] fileData = ticketAttachment?.FileData!;
			string ext = Path.GetExtension(fileName).Replace(".", "");

			Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
			return File(fileData, $"application/{ext}");
		}

        // GET: Assign Dev 
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignDeveloper(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);

            if (project == null)
            {
                return NotFound();
            }

            //Get list of developers
            IEnumerable<BTUser> developers = await _rolesService
                                                    .GetUsersInRoleAsync(nameof(BTRoles.Developer), _companyId);

            //   BTUser? currentPM = await _ticketService.GetTicketDeveloperAsync(id);

            //AssignPMViewModel viewModel = new()
            //{
            //    ProjectId = project.Id,
            //    ProjectName = project.Name,
            //    PMList = new SelectList(developers, "Id", "FullName", currentPM?.Id),
            //    PMId = currentPM?.Id,
            //};
            return View();

          //  return View(viewModel);
        }
        [Authorize(Roles = "Admin,ProjectManager")]
        //POST Assign Dev
        [HttpPost]
        public async Task<IActionResult> AssignDeveloper(AssignTicketViewModel viewModel)
        {
            string? currentUserId = _userManager.GetUserId(User);

            //Get AsNoTrcking
            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket?.Id,_companyId);
            
            //Add Ticket History
            Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket?.Id, _companyId);
        
            await _historyService.AddHistoryAsync(oldTicket, newTicket, currentUserId);

            // Add Ticket Notifcation
            await _notificationService.NewDeveloperNotificationAsync(viewModel.Ticket?.Id, viewModel.DeveloperId, currentUserId);
            return View(viewModel);
        }
		// GET: Tickets/Create
		public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,ProjectManager, DeveloperUser, SubmitterUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Archived,ProjectId,ArchivedByProject,TicketTypeId,TicketStatusId,TicketPriorityId")] Ticket ticket)
        {

            ModelState.Remove("SubmitterUserId");
          

            if (ModelState.IsValid)
            {
                ticket.SubmitterUserId = _userManager.GetUserId(User);
                ticket.Created = DateTime.Now;
                
                //call the ticket service
                await _ticketService.AddTicketAsync(ticket);

                //Add History record
                int companyId = User.Identity!.GetCompanyId();
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                await _historyService.AddHistoryAsync(null!,newTicket,ticket.SubmitterUserId);

                await _notificationService.NewTicketNotificationAsync(ticket.Id, ticket.SubmitterUserId);


                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Id", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Id", ticket.TicketTypeId);
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Archived,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string userId = _userManager.GetUserId(User)!;

                    Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);

                    ticket!.SubmitterUserId = _userManager.GetUserId(User);
                    ticket.Created = DateTime.Now;

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();

                    //Add History
                    Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);
                    await _historyService.AddHistoryAsync(oldTicket, newTicket, userId);


                    
                 
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
          return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        //[HttpGet]
        //[Authorize(Roles = "Admin,ProjectManager")]
        //public IActionResult Archive()
        //{
           

        //    return View();


        //}

        [HttpGet]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Archive( int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Archived = true;
            await _ticketService.UpdateTicketAsync(ticket);

           return View(ticket);


        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> ArchiveConfirmed(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Archived = true;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction(nameof(Details));


        }

        // GET: Tickets
        public async Task<IActionResult> ArchivedTickets()
        {

            //var applicationDbContext = _context.Tickets
            //    .Include(t => t.DeveloperUser).Include(t => t.Project).
            //    Include(t => t.SubmitterUser).Include(t => t.TicketPriority)
            //    .Include(t => t.TicketStatus).Include(t => t.TicketType);

            IEnumerable<Ticket> tickets = await _ticketService.GetAllArchivedTicketsByCompanyIdAsync(_companyId);

            return View(tickets);
        }

        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);


            if (ticket == null)
            {
                return NotFound();
            }
            ticket.Archived = false;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction(nameof(Index));

        }
    }
}
