using BugTracker.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Controllers
{

    [Controller]
    public abstract class BTBaseController : Controller
    {
        protected int _companyId => User.Identity!.GetCompanyId();
    }
}
