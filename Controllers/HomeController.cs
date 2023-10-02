using BugTracker.Models;
using BugTracker.Models.ViewModel;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BugTracker.Controllers
{
    public class HomeController : BTBaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBTProjectService _projectService;

        private readonly IBTTicketService _ticketService;
        private readonly IBTCompanyService _companyService;

        public HomeController(ILogger<HomeController> logger, IBTProjectService projectService, IBTTicketService ticketService, IBTCompanyService companyService)
        {
            _logger = logger;
            _projectService = projectService;
            _ticketService = ticketService;
            _companyService = companyService;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> DashBoard(DashboardViewModel dashboardViewModel)
        {
            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(_companyId);
            ViewData["ProjectTotalCount"] = projects.Count();

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(_companyId);
            ViewData["TicketsTotalCount"] = tickets.Count();

            List<BTUser> members = await _companyService.GetMembersAsync(_companyId);

            ViewData["TotalMembersCount"] = members.Count();

           

            dashboardViewModel.Projects = projects; 
            dashboardViewModel.Tickets = tickets;
           


            return View(dashboardViewModel);
        }

        [HttpGet]
        public IActionResult Landing()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}