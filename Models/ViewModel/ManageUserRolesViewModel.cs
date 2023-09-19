using BugTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModel
{
    public class ManageUserRolesViewModel
    {
        public BTUser? BTUser { get; set; }

        public MultiSelectList? Roles { get; set; }

        public IEnumerable<string>? SelectedRoles { get; set; }


    }
}
