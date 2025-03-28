namespace MVC.Features.Empty
{
    public class EmptyController : Controller
    {
        public IActionResult Index()
        {
            
            return View("/Features/Empty/Empty.cshtml");
        }
    }
}
