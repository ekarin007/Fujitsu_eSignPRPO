using ClosedXML.Excel;
using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models.AccountCode;
using Fujitsu_eSignPO.Models.Customer;
using Fujitsu_eSignPO.Services.PRPO;
using Microsoft.AspNetCore.Mvc;


namespace Fujitsu_eSignPO.Controllers
{
    public class AccountCodeController : Controller
    {


        private readonly IAccountCodeService _accountCodeService;
        public AccountCodeController(IAccountCodeService accountCodeService)
        {
            _accountCodeService = accountCodeService;
        }


        public async Task<IActionResult> Index()
        {
            var getNormalCode = await _accountCodeService.getNormalCode();
            ViewBag.mainCode = getNormalCode.Select(x => x.MainCode).Distinct().ToList();

            ViewBag.subCode1 = getNormalCode.Select(x => x.AccountName).Distinct().ToList();

            return View();
        }

        public async Task<IActionResult> getAccountCode()
        {
            var response = await _accountCodeService.getAccountCode();
            return Json(new { data = response });
        }



        public async Task<IActionResult> getAccountCodeByMainCode(string MC)
        {
            var response = await _accountCodeService.getAccCodeByMCandSC1(MC);
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
            ViewBag.mainCode = getNormalCode.Select(x => x.MainCode).Distinct().ToList();

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
                    budget = getAccCodeId?.Budget,
                    balance = getAccCodeId?.Balance,
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

        public async Task<IActionResult> ExportAccCode(string mc)
        {
            try
            {
                XLWorkbook wbook2 = new XLWorkbook();

                var wb = wbook2.Worksheets.Add("Account Code");

                wb.PageSetup.PaperSize = XLPaperSize.A4Paper;
                wb.Range("A1:E1").Columns().Style.Fill.BackgroundColor = XLColor.BabyBlueEyes;

                wb.Cell("A1").Value = "Main Code";
                wb.Cell("B1").Value = "Sub Code 1";
                wb.Cell("C1").Value = "Sub Code 2";
                wb.Cell("D1").Value = "Budget";
                wb.Cell("E1").Value = "Balance";



                #region worksheets style
                wb.Cell("A1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("B1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("C1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("D1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("E1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);


                #endregion

                wb.RangeUsed().SetAutoFilter();
                wb.Columns().AdjustToContents();


                var getAccCode = await _accountCodeService.getAccCodeByMCandSC1(mc);

                if (getAccCode.Count > 0)
                {
                    for (int i = 0; i <= (getAccCode.Count - 1); i++)
                    {
                        wb.Cell("A" + (2 + i)).Value = getAccCode[i].MainCode;
                        //wb.Cell("A" + (2 + i)).Style.DateFormat.Format = "dd-MM-yy";
                        wb.Cell("B" + (2 + i)).Value = getAccCode[i].SubCode1;

                        wb.Cell("C" + (2 + i)).Value = getAccCode[i].SubCode2;
                        //wb.Cell("C" + (2 + i)).SetDataType(XLDataType.Text);

                        wb.Cell("D" + (2 + i)).Value = getAccCode[i]?.Budget?.ToString("#,##0.00");
                        wb.Cell("E" + (2 + i)).Value = getAccCode[i]?.Balance?.ToString("#,##0.00");

                        //wb.Cell("V" + (2 + i)).Value = "Gross Weight / Unit";

                        #region worksheets style
                        wb.Cell("A" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("B" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("C" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("D" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("E" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);


                        #endregion
                    }


                }

                wb.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);


                using (MemoryStream memoryStream = new MemoryStream())
                {
                    wbook2.SaveAs(memoryStream);
                    var content = memoryStream.ToArray();

                    return File(
                    content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "AccountCode_Export.xlsx");
                }
            }
            catch (Exception ex)
            {
                return NotFound("ERROR :" + ex.Message);
            }

        }

    }
}
