using DocumentFormat.OpenXml.InkML;
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

              var genVdrCode = await GenerateVendorCodeAsync(Request.vendorName);

                var insertUser = new TbVendor
                {
                    VendorCode = genVdrCode,
                    VendorName = Request.vendorName

                };

                _eSignPrpoContext.TbVendors.Add(insertUser);

                var getCustomer = await _eSignPrpoContext.TbCustomers.Where(x => x.SCusUsername == genVdrCode).FirstOrDefaultAsync();

                if (getCustomer == null)
                {
                    var insertCus = new TbCustomer
                    {
                        UCusId = Guid.NewGuid(),
                        SCusUsername = genVdrCode,
                        SCusName = Request.vendorName,
                        BActive = true,
                        DCreated = DateTime.Now,
                        SCreatedBy = "System Accountant",

                    };

                    _eSignPrpoContext.TbCustomers.Add(insertCus);
                }

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

        public async Task<string> GenerateVendorCodeAsync(string vendorName)
        {
            if (string.IsNullOrWhiteSpace(vendorName))
                throw new ArgumentException("Vendor name is required");

            string firstChar = vendorName.Substring(0, 1).ToUpper();

            int count = await _eSignPrpoContext.TbVendors.OrderByDescending(x=>x.VendorCode)
                .CountAsync(v => v.VendorCode.StartsWith(firstChar));

            // ถ้าไม่เจอเลย ให้เริ่มต้นที่ 1
            int sequenceNumber = count + 1;

            string newCode = $"{firstChar}{sequenceNumber.ToString("D5")}";
            return newCode;
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

                    var getCus = await _eSignPrpoContext.TbCustomers.Where(x => x.SCusUsername == getVdr.VendorCode).FirstOrDefaultAsync();

                    if(getCus != null)
                    {
                        getCus.SCusName = getVdr.VendorName;
                    }

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
            
            public string vendorCode { get; set; }
            [Required(ErrorMessage = "Vendor Name is required.")]
            public string vendorName { get; set; }

        }
    }
}

