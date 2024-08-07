using eSignPRPO.Services.PRPO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace eSignPRPO.Controllers
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
