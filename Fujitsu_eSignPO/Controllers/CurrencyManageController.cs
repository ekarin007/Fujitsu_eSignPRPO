using Fujitsu_eSignPO.Data;
using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static Fujitsu_eSignPO.Controllers.UserManageController;



namespace Fujitsu_eSignPO.Controllers
{
    public class CurrencyManageController : Controller
    {
        private readonly FgdtESignPoContext _eSignPrpoContext;
        private readonly IAccountService _accountSv;
        public CurrencyManageController(FgdtESignPoContext eSignPoContext, IAccountService accountSv)
        {
            _eSignPrpoContext = eSignPoContext;
            _accountSv = accountSv;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> getCurrency()
        {
            var response = await _eSignPrpoContext.TbCurrencies.ToListAsync();
            return Json(new { data = response });
        }
         
        public async Task<IActionResult> InsertUpdate(string currId = null)
        {
            var response = new CurrencyInsertUpdateModel();

            var getCurrById = await _eSignPrpoContext.TbCurrencies.Where(x => x.CurrencyCode == currId).FirstOrDefaultAsync();

            if (getCurrById == null)
            {
               
                return View(response);
            }

            response = new CurrencyInsertUpdateModel
            {
                sCurrId = getCurrById.CurrencyCode,
                sCurrName = getCurrById.CurrencyName
            };

            return View(response);
        }
        [HttpPost]
        public async Task<IActionResult> InsertUpdate(CurrencyInsertUpdateModel Request, string isEdit)
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

        public async Task<JsonResponse> insertUser(CurrencyInsertUpdateModel Request)
        {
            var response = new JsonResponse();         
            try
            {
                var insertUser = new TbCurrency
                {
                    CurrencyCode = generateCurrID(),
                    CurrencyName = Request.sCurrName

                };

                _eSignPrpoContext.TbCurrencies.Add(insertUser);

                var state = await _eSignPrpoContext.SaveChangesAsync() > 0;

                response.status = state;
                response.message = "Successfully added currency data to the database.";

            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.InnerException.Message;

            }

            return response;
        }

        public async Task<JsonResponse> updateUser(CurrencyInsertUpdateModel Request)
        {
            var response = new JsonResponse();

            try
            {
                var getCurr = await _eSignPrpoContext.TbCurrencies.Where(x => x.CurrencyCode == Request.sCurrId)
.FirstOrDefaultAsync();

                if (getCurr != null)
                {                  
                    getCurr.CurrencyName = Request?.sCurrName;
                    
                }


                var state = await _eSignPrpoContext.SaveChangesAsync() > 0;

                response.status = state;
                response.message = "Successfully updated currency data to the database.";

            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.InnerException.Message;

            }

            return response;
        }

        public async Task<IActionResult> deleteCurr(string data)
        {

            var response = new JsonResponse();
            try
            {

                var getCurr = await _eSignPrpoContext.TbCurrencies.Where(x => x.CurrencyCode == data).FirstOrDefaultAsync();

                if (getCurr != null)
                {
                    _eSignPrpoContext.TbCurrencies.Remove(getCurr);
                }

                var state = await _eSignPrpoContext.SaveChangesAsync() > 0;

                response.status = state;
                response.message = "Successfully deleted currency data to the database.";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.InnerException.Message;
                return BadRequest(response);
            }
        }

        public string generateCurrID()
        {

            string currId = "001";
            var getCurrId = _eSignPrpoContext.TbCurrencies.OrderByDescending(x => x.CurrencyCode).FirstOrDefault();

            if (getCurrId != null)
            {
                string lastIdNumber = getCurrId.CurrencyCode; // เช่น "E0005" → "0005"
                int newIdNumber = int.Parse(lastIdNumber) + 1;

                // สร้างรหัสใหม่โดยเติม 0 ด้านหน้าให้ครบ 4 หลัก
                currId =  newIdNumber.ToString("D3"); // "E0006"
            }

            return currId;
        }

        public class CurrencyInsertUpdateModel
        {
            public string sCurrId { get; set; }
            [Required(ErrorMessage = "Currency Name is required.")]
            public string sCurrName { get; set; }          
           
        }

    }
}
