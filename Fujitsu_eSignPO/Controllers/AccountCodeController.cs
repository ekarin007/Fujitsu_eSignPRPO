using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models.AccountCode;
using Fujitsu_eSignPO.Models.Customer;
using Microsoft.AspNetCore.Mvc;


namespace Fujitsu_eSignPO.Controllers
{
    public class AccountCodeController : Controller
    {

        
        private readonly IAccountCodeService _accountCodeService;
        public AccountCodeController( IAccountCodeService accountCodeService)
        {           
            _accountCodeService = accountCodeService;
        }


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> getAccountCode()
        {
            var response = await _accountCodeService.getAccountCode();
            return Json(new { data = response });
        }

        public async Task<IActionResult> deleteAccountCode(string guid)
        {

            var parseGuid = Guid.Parse(guid);
            var response = await _accountCodeService.deleteAccountCode(parseGuid);

            if (!response.Item1)
            {
                return NotFound(new { status = response.Item1, msg = response.Item2 });
            }

            return Ok(new { status = response.Item1, msg = response.Item2 });
        }

        public async Task<IActionResult> InsertUpdate(string uAccId = null)
        {
            Guid? parseGuid = uAccId != null ? Guid.Parse(uAccId) : null;
            var response = new AccCodeInsertUpdateModel();

            var getNormalCode = await _accountCodeService.getNormalCode();
            ViewBag.mainCode = getNormalCode.Select(x=>x.MainCode).Distinct().ToList();
            
            ViewBag.subCode1 = getNormalCode.Select(x => x.AccountName).Distinct().ToList();
          
            ViewBag.subCode2 = getNormalCode.Select(x => x.Section).Distinct().ToList();




            var getAccCodeId = await _accountCodeService.GetAccountCodeByGuid(parseGuid);



            //if (getAccCodeId == null)
            //{
            //    var getSupplier = await _PRPOService.getVendorData();
            //    ViewBag.Supplier = getSupplier;

            //    return View(response);
            //}

            if (getAccCodeId != null)
            {
                response = new AccCodeInsertUpdateModel
                {
                    accId = getAccCodeId.UAcGuid,
                    mainCode = getAccCodeId?.MainCode,
                    subCode1 = getAccCodeId?.SubCode1,
                    subCode2 = getAccCodeId?.SubCode2,
                    budget= getAccCodeId?.Budget,
                    balance= getAccCodeId?.Balance,
                    active = getAccCodeId.Active == true ? "true" : "false",
                };

            }

            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> InsertUpdate(AccCodeInsertUpdateModel Request, string isEdit)
        {
            var response = new Tuple<bool, string>(false, string.Empty);

            if (isEdit == "1")
            {
                response = await _accountCodeService.updateAccountCode(Request);
            }
            else
            {
                response = await _accountCodeService.insertAccountCode(Request);
            }

            if (!response.Item1)
            {
                return NotFound(new { status = response.Item1, msg = response.Item2 });
            }

            return Ok(new { status = response.Item1, msg = response.Item2 });
        }


        public async Task<IActionResult> subCode1Data(string searchTerm, string mainCode)
        {

            var getSubCode1Data = await _accountCodeService.getSubCode1(mainCode);

            var filteredOptions = getSubCode1Data;

            if (searchTerm != null)
            {
                filteredOptions = getSubCode1Data.Where(x => x.Contains(searchTerm) || x.Contains(searchTerm)).ToList();
            }

            return Json(filteredOptions.Select(x => new { id = x, text = x }));
        }

        public async Task<IActionResult> subCode2Data(string searchTerm, string subCode1)
        {

            var getSubCode2Data = await _accountCodeService.getSubCode2(subCode1);

            var filteredOptions = getSubCode2Data;

            if (searchTerm != null)
            {
                filteredOptions = getSubCode2Data.Where(x => x.Contains(searchTerm) || x.Contains(searchTerm)).ToList();
            }

            return Json(filteredOptions.Select(x => new { id = x, text = x }));
        }

    }
}
