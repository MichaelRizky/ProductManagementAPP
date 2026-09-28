using Microsoft.AspNetCore.Mvc;

namespace ProductManagementAPP.Controllers
{
    public class HomeController : Controller
    {
        // GET: /
        public IActionResult Index()
        {
            return View();
        }
    }
}