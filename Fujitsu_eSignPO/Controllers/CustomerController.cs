using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;

namespace Fujitsu_eSignPO.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly IPRPOService _PRPOService;
        private readonly ICustomerService _customerService;
        public CustomerController(IPRPOService pRPOService, ICustomerService customerService)
        {
            _PRPOService = pRPOService;
            _customerService = customerService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> getCustomer()
        {
            var response = await _customerService.getCustomer();
            return Json(new { data = response });
        }
        public async Task<IActionResult> InsertUpdate(string supplierID = null)
        {
            var response = new CustomerInsertUpdateModel();

            var getCusBySupID = await _customerService.getCustomerBySupID(supplierID);

            if (getCusBySupID == null)
            {
                var getSupplier = await _PRPOService.getVendorData();
                ViewBag.Supplier = getSupplier;

                return View(response);
            }

            response = new CustomerInsertUpdateModel
            {
                cusUserName = $"{getCusBySupID?.SCusUsername}|{getCusBySupID?.SCusName}",
                cusPassword = getCusBySupID?.SCusPassword,
                cusMail = getCusBySupID?.SCusEmail,
                cusActive = getCusBySupID.BActive == true ? "true" : "false",
            };

            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> InsertUpdate(CustomerInsertUpdateModel Request, string isEdit)
        {
            var response = new Tuple<bool, string>(false, string.Empty);

            if (isEdit == "1")
            {
                response = await _customerService.updateCustomer(Request);
            }
            else
            {
                response = await _customerService.insertCustomer(Request);
            }

            if (!response.Item1)
            {
                return NotFound(new { status = response.Item1, msg = response.Item2 });
            }

            return Ok(new { status = response.Item1, msg = response.Item2 });
        }

        public async Task<IActionResult> deleteCustomer(string data)
        {
          
            var response = await _customerService.deleteCustomer(data);

            if (!response.Item1)
            {
                return NotFound(new { status = response.Item1, msg = response.Item2 });
            }

            return Ok(new { status = response.Item1, msg = response.Item2 });
        }
    }
}
