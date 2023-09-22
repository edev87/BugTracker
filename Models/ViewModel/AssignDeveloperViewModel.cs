using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModel
{
    public class AssignDeveloperViewModel
    {
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public SelectList? DevList { get; set; }
        public string? PMId { get; set; }

        public virtual Project? Project { get; set; }

        public virtual Ticket? Ticket { get; set; }
    }
}
