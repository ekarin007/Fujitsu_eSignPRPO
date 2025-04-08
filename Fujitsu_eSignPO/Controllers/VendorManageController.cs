using Fujitsu_eSignPO.Data;
using Fujitsu_eSignPO.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static Fujitsu_eSignPO.Controllers.CurrencyManageController;
namespace Fujitsu_eSignPO.Controllers
{
    public class VendorManageController : Controller
    {
        private readonly FgdtESignPoContext _eSignPrpoContext;
        public VendorManageController(FgdtESignPoContext eSignPoContext)
        {
            _eSignPrpoContext = eSignPoContext;

        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> getVendor()
        {
            var response = await _eSignPrpoContext.TbVendors.ToListAsync();
            return Json(new { data = response });
        }

        public async Task<IActionResult> InsertUpdate(string vendorId = null)
        {
            var response = new VendorInsertUpdateModel();

            var getvdrById = await _eSignPrpoContext.TbVendors.Where(x => x.VendorCode == vendorId).FirstOrDefaultAsync();

            if (getvdrById == null)
            {

                return View(response);
            }

            response = new VendorInsertUpdateModel
            {
                vendorCode = getvdrById.VendorCode,
                vendorName = getvdrById.VendorName
            };

            return View(response);
        }


        [HttpPost]
        public async Task<IActionResult> InsertUpdate(VendorInsertUpdateModel Request, string isEdit)
        {
            var response = new JsonResponse();

            if (isEdit == "1")
            {
                response = await updateUser(Request);
            }
            else
            {
                response = await insertUser(Request);
            }

            if (!response.status)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        public async Task<JsonResponse> insertUser(VendorInsertUpdateModel Request)
        {
            var response = new JsonResponse();
            try
            {
                var getCurr = await _eSignPrpoContext.TbVendors.Where(x => x.VendorCode == Request.vendorCode)
.FirstOrDefaultAsync();

                if (getCurr != null)
                {
                    response.status = false;
                    response.message = $"Vendor Code {Request.vendorCode} already exists.";
                    return response;
                }


                var insertUser = new TbVendor
                {
                    VendorCode = Request.vendorCode,
                    VendorName = Request.vendorName

                };

                _eSignPrpoContext.TbVendors.Add(insertUser);

                var state = await _eSignPrpoContext.SaveChangesAsync() > 0;

                response.status = state;
                response.message = "Successfully added vendor data to the database.";

            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.InnerException.Message;

            }

            return response;
        }

        public async Task<JsonResponse> updateUser(VendorInsertUpdateModel Request)
        {
            var response = new JsonResponse();

            try
            {
                var getVdr = await _eSignPrpoContext.TbVendors.Where(x => x.VendorCode == Request.vendorCode)
.FirstOrDefaultAsync();

                if (getVdr != null)
                {
                    getVdr.VendorName = Request?.vendorName;

                }


                var state = await _eSignPrpoContext.SaveChangesAsync() > 0;

                response.status = state;
                response.message = "Successfully updated vendor data to the database.";

            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.InnerException.Message;

            }

            return response;
        }
        public async Task<IActionResult> deleteVendor(string data)
        {

            var response = new JsonResponse();
            try
            {

                var getVdr = await _eSignPrpoContext.TbVendors.Where(x => x.VendorCode == data).FirstOrDefaultAsync();

                if (getVdr != null)
                {
                    _eSignPrpoContext.TbVendors.Remove(getVdr);
                }

                var state = await _eSignPrpoContext.SaveChangesAsync() > 0;

                response.status = state;
                response.message = "Successfully deleted vendor data to the database.";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.InnerException.Message;
                return BadRequest(response);
            }
        }
        public class VendorInsertUpdateModel
        {
            [Required(ErrorMessage = "Vendor Code is required.")]
            public string vendorCode { get; set; }
            [Required(ErrorMessage = "Vendor Name is required.")]
            public string vendorName { get; set; }

        }
    }
}

