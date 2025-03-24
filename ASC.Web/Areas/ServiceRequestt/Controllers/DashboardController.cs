using ASC.Solution.Configuration;
using ASC.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequestt")]
    public class DashboardController : BaseController
    {
        private IOptions<ApplicationSettings> _settings;
        public DashboardController(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }
        public IActionResult DashBoard()
        {
            return View();
        }
    }
}