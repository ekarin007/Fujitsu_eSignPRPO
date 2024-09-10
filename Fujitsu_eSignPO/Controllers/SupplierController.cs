using Fujitsu_eSignPO.Services.PRPO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Fujitsu_eSignPO.Controllers
{
    
    public class SupplierController : Controller
    {
       
        [Authorize(Roles = "99")]
        public IActionResult WorkList()
        {
            return View();
        }

        public IActionResult History()
        {
            return View();
        }
        

    }
}
