﻿
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using BugTracker.Extensions;

namespace BugTracker.Controllers
{

    [Authorize(Roles = "Admin")]
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyService _companyService;
        private readonly IDataProtector _provider;
        private readonly IEmailSender _emailService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTInviteService _inviteService;
        public InvitesController(ApplicationDbContext context, IBTProjectService projectService,
            IBTCompanyService companyService, IDataProtectionProvider dataProtectionProvider, 
            IEmailSender emailSender, UserManager<BTUser> userManager, IBTInviteService inviteService)
        {
            _context = context;
            _projectService = projectService;
            _companyService = companyService;
           _provider = dataProtectionProvider.CreateProtector("BugTracker");
            _emailService = emailSender;
            _userManager = userManager;
            _inviteService = inviteService;
        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invites.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor).Include(i => i.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // GET: Invites/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();

            ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId), "Id", "Name");

            return View();
        }

        //POST: Invites/Create
        //To protect from overposting attacks, enable the specific properties you want to bind to.
        //For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

       [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,InviteDate,JoinDate,CompanyToken,CompanyId,ProjectId,InvitorId,InviteeId,InviteeEmail,InviteeFirstName,InviteeLastName,Message,IsValid")] Invite invite)
        {
            ModelState.Remove("InvitorId");
            int companyId = User.Identity!.GetCompanyId();


            if (ModelState.IsValid)
            {
                try
                {

                    //encrypt code for invite
                    Guid guid = Guid.NewGuid();

                    string token = _provider.Protect(guid.ToString());
                    string email = _provider.Protect(invite.InviteeEmail!);
                    string company = _provider.Protect(companyId.ToString());

                    string? callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email, company }, protocol: Request.Scheme);

                    string body = $@"{invite.Message} <br />
                       Please join my Company. <br />
                       Click the following link to join our team. <br />
                       <a href=""{callbackUrl}"">COLLABORATE</a>";

                    string? destination = invite.InviteeEmail;

                    Company btCompany = await _companyService.GetCompanyInfoAsync(companyId);

                    string? subject = $" Nova Tracker: {btCompany.Name} Invite";

                    await _emailService.SendEmailAsync(destination!, subject, body);


                    // Save invite in the DB
                    invite.CompanyToken = guid;
                    invite.CompanyId = companyId;
                    invite.InviteDate = DateTime.Now;
                    invite.InvitorId = _userManager.GetUserId(User);
                    invite.IsValid = true;

                    // Add Invite service method for "AddNewInviteAsync"
                    await _inviteService.AddNewInviteAsync(invite);

                    return RedirectToAction("Index", "Home");

                    // TODO: Possibly use SWAL message

                }
                catch (Exception)
                {

                    throw;
                }

            }

            ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId), "Id", "Name");
            return View(invite);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessInvite(string token, string email, string company)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(company))
            {
                return NotFound();
            }

            Guid companyToken = Guid.Parse(_provider.Unprotect(token));
            string? inviteeEmail = _provider.Unprotect(email);
            int companyId = int.Parse(_provider.Unprotect(company));

            try
            {
                Invite? invite = await _inviteService.GetInviteAsync(companyToken, inviteeEmail, companyId);

                if (invite != null)
                {
                    return View(invite);
                }

                return NotFound();
            }
            catch (Exception)
            {

                throw;
            }

        }

        // GET: Invites/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites.FindAsync(id);
            if (invite == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", invite.ProjectId);
            return View(invite);
        }

        // POST: Invites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InviteDate,JoinDate,CompanyToken,CompanyId,ProjectId,InvitorId,InviteeId,InviteeEmail,InviteeFirstName,InviteeLastName,Message,IsValid")] Invite invite)
        {
            if (id != invite.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InviteExists(invite.Id))
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
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", invite.ProjectId);
            return View(invite);
        }

        // GET: Invites/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // POST: Invites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Invites == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Invites'  is null.");
            }
            var invite = await _context.Invites.FindAsync(id);
            if (invite != null)
            {
                _context.Invites.Remove(invite);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InviteExists(int id)
        {
          return (_context.Invites?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
