using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models.PRPO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using MimeKit;
using System.Data;
using ClosedXML.Excel;
using System.Globalization;
using Fujitsu_eSignPO.Models;
using System.Diagnostics;
using Fujitsu_eSignPO.Data;
using Microsoft.EntityFrameworkCore;
using AspNetCore;
using MailKit.Search;
using AspNetCore.Reporting;

namespace Fujitsu_eSignPO.Controllers
{
    [Authorize(Roles = "0,1,2,3,4,5,99")]
    public class PRPOController : Controller
    {
        private readonly IPRPOService _PRPOService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAccountService _accountService;
        private readonly IWorkflowService _workflowService;
        private readonly IConfiguration _config;
        private readonly FgdtESignPoContext _eSignPrpoContext;
        private readonly ILogger<PRPOController> _logger;
        public PRPOController(IPRPOService PRPOService, IWebHostEnvironment webHostEnvironment, IAccountService accountService, IWorkflowService workflowService, IConfiguration config, FgdtESignPoContext eSignPrpoContext, ILogger<PRPOController> logger)
        {
            _PRPOService = PRPOService;
            _webHostEnvironment = webHostEnvironment;
            _accountService = accountService;
            _workflowService = workflowService;
            _config = config;
            _eSignPrpoContext = eSignPrpoContext;
            _logger = logger;
        }
        public IActionResult WorkList()
        {
            var information = _accountService.informationUser();

            return View(information);
        }

        [Authorize(Roles = "4")]
        public IActionResult POWorkList()
        {
            var information = _accountService.informationUser();

            return View(information);
        }

        public async Task<IActionResult> CreateOrEdit(Guid gID)
        {
            CultureInfo culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            var response = new PRPOViewModel();

            response.poDate = DateTime.Now;

            var getVendor = await _PRPOService.getVendorData();
            ViewBag.Vendor = getVendor;

            var getDeparment = await _PRPOService.getDepData();
            ViewBag.departments = getDeparment;

            var getCurr = await _PRPOService.getCurrData();
            ViewBag.curr = getCurr;

            var getMainCode = await _PRPOService.getMainCode();
            ViewBag.mainCode = getMainCode;



            var getPR = await _PRPOService.getPrRequestByNo(gID);

            if (getPR == null)
            {
                return View(response);
            }

            var getSubCode1 = await _PRPOService.getSubCode1(getPR?.SMainCode);
            ViewBag.subCode1 = getSubCode1;

            var getSubCode2 = await _PRPOService.getSubCode2(getPR?.SSubCode1);
            ViewBag.subCode2 = getSubCode2;

            var getPRItem = await _PRPOService.getPrRequestItemByNo(getPR?.SPoNo);

            if (getPRItem == null)
            {
                return View(response);
            }

            var getAttData = await _PRPOService.getAttachmentsData(gID);

            var getBB = await _PRPOService.getBudgetBalance(getPR?.SMainCode, getPR?.SSubCode1, getPR?.SSubCode2);

            var getEmailVC = await _PRPOService.getVendorEmail(getPR?.SVendorCode);

            response = new PRPOViewModel
            {
                vendorName = $"{getPR?.SVendorCode}",
                refQuatation = getPR?.SRefQuotation,
                department = getPR?.SDepartment,
                email = getEmailVC,
                shippingDate = getPR?.DShippingDate,
                poDate = getPR?.DPoDate,
                currency = getPR?.SCurrency,
                mainCode = getPR?.SMainCode,
                subCode1 = getPR?.SSubCode1,
                subCode2 = getPR?.SSubCode2,
                balance = getBB?.Balance,
                budget = getBB?.Budget,
                reason = getPR?.SReason == null ? "" : getPR?.SReason.Replace("\n", "").Replace("\r", ""),
                totalAmount = getPR?.FSumAmtCurrency?.ToString("#,##0.00"),
                totalAmountTHB = getPR?.FSumAmtThb?.ToString("#,##0.00"),
                nStatus = getPR?.NStatus,
                rate = getPR?.FRate,
                vatOption = getPR?.SVatType,
                listPRPOItems = getPRItem.Select(x => new listPRPOItem
                {
                    no = x?.NNo.ToString(),
                    // partNo = x?.SPartNo.Replace("\n", "\\n").Replace("\r", "\\r"),
                    //partName = x?.SPartName.Replace("\n","\\n").Replace("\r","\\r"),
                    partNo = x?.SPartNo,
                    partName = x?.SPartName,
                    vatType = x.SVatType,
                    unitPrice = x.FUnitPrice?.ToString("#,##0.00"),
                    qty = x.NQty?.ToString(),
                    amount = x?.FAmount?.ToString("#,##0.00"),
                    uPoItemId = x?.UPrItemId

                }).ToList(),
                fileUploads = getAttData.Select(x => new fileUpload
                {
                    sAttach_Name = x.SAttachName,
                    sAttach_File_Size = x?.FAttachFileSize?.ToString("0.00"),
                    sAttach_File_Type = x?.SAttachFileType,
                    uPrId = x?.UPrId

                }).ToList()

            };

            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrEdit(PRPOViewModel prpoRequest, string listPRPOItem, string gID, string isEdit, string isReSubmit)
        {
            Guid guid = Guid.Parse(gID);
            var ListPRPO = JsonSerializer.Deserialize<List<listPRPOItem>>(listPRPOItem);

            var requestPR = new Tuple<bool, string>(false, string.Empty);

            if (int.Parse(isEdit) != 1)
            {
                requestPR = await _PRPOService.InsertPR(prpoRequest, ListPRPO, guid);
            }
            else
            {
                requestPR = await _PRPOService.UpdatePR(prpoRequest, ListPRPO, guid, isReSubmit);
            }
            if (!requestPR.Item1)
            {
                return NotFound(new { status = requestPR.Item1, msg = requestPR.Item2 });
            }

            return Ok(new { status = requestPR.Item1, msg = requestPR.Item2 });

        }

        public async Task<IActionResult> mainCodeData(string searchTerm)
        {

            var getMainCodeData = await _PRPOService.getMainCode();

            var filteredOptions = getMainCodeData;

            if (searchTerm != null)
            {
                filteredOptions = getMainCodeData.Where(x => x.ToLower().Contains(searchTerm.ToLower()) || x.ToLower().Contains(searchTerm.ToLower())).ToList();
            }

            return Json(filteredOptions.Select(x => new { id = x, text = x }));
        }

        public async Task<IActionResult> vendorData(string searchTerm)
        {

            var getvendorData = await _PRPOService.getVendorData();

            var filteredOptions = getvendorData;

            if (searchTerm != null)
            {
                filteredOptions = getvendorData.Where(x => x.VendorCode.ToLower().Contains(searchTerm.ToLower()) || x.VendorName.ToLower().Contains(searchTerm.ToLower())).ToList();
            }

            return Json(filteredOptions.Select(x => new { id = x.VendorCode, text = x.VendorName }));
        }
        public async Task<IActionResult> subCode1Data(string searchTerm, string mainCode)
        {

            var getSubCode1Data = await _PRPOService.getSubCode1(mainCode);

            var filteredOptions = getSubCode1Data;

            if (searchTerm != null)
            {
                filteredOptions = getSubCode1Data.Where(x => x.ToLower().Contains(searchTerm.ToLower()) || x.ToLower().Contains(searchTerm.ToLower())).ToList();
            }

            return Json(filteredOptions.Select(x => new { id = x, text = x }));
        }

        public async Task<IActionResult> subCode2Data(string searchTerm, string subCode1)
        {

            var getSubCode2Data = await _PRPOService.getSubCode2(subCode1);

            var filteredOptions = getSubCode2Data;

            if (searchTerm != null)
            {
                filteredOptions = getSubCode2Data.Where(x => x.ToLower().Contains(searchTerm.ToLower()) || x.ToLower().Contains(searchTerm.ToLower())).ToList();
            }

            return Json(filteredOptions.Select(x => new { id = x, text = x }));
        }

        [HttpPost]
        public async Task<IActionResult> getBudgetBalance(string mainCode, string subCode1, string subCode2)
        {
            var getBB = await _PRPOService.getBudgetBalance(mainCode, subCode1, subCode2);

            if (getBB == null)
            {
                return Json(new { budget = 0, balance = 0 });
            }

            return Json(new { budget = getBB.Budget, balance = getBB.Balance });
        }


        public async Task<IActionResult> getPrRecords()

        {
            var getPrRecords = await _PRPOService.getPrRecords();

            return Json(new { data = getPrRecords});


        }

        public async Task<IActionResult> getPoRecords()
        {
            var getPoRecords = await _PRPOService.getPoRecords();

            return Json(new { data = getPoRecords });


        }

        public async Task<IActionResult> UploadFiles(List<IFormFile> files, string queryString)
        {
            try
            {
                Guid guid = Guid.Parse(queryString);

                string pathFile = $"{this._webHostEnvironment.WebRootPath}\\uploadfile\\";

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var filePath = Path.Combine(pathFile, $"{guid}_{file.FileName}");
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }

                var insertFiles = await _PRPOService.InsertAttachment(files, guid);

                if (insertFiles)
                {
                    var attList = await _PRPOService.getAttachmentsData(guid);
                    return Ok(new { msg = "Files uploaded successfully.", attList });
                }
                else
                {
                    return NotFound("Files uploaded failed.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex?.Message);
            }
        }

        public async Task<IActionResult> DeleteFile(string fileName, string queryString)
        {
            Guid guid = Guid.Parse(queryString);

            string pathFile = $"{this._webHostEnvironment.WebRootPath}\\uploadfile\\";

            var filePath = Path.Combine(pathFile, $"{guid}_{fileName}");

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);

                var delFile = await _PRPOService.DeleteFile(fileName, guid);
                if (!delFile)
                {
                    return NotFound("File deleted failed.");
                }

                var attList = await _PRPOService.getAttachmentsData(guid);

                return Ok(new { msg = "File deleted successfully.", attList });

            }
            else
            {
                return NotFound("File not found.");
            }
        }

        [Route("/PRPO/ApprovePR/{PRNo}")]
        public async Task<IActionResult> approvePR(string PRNo)
        {
            var response = await _PRPOService.getPRAllDetail(PRNo);
            response.reason = "";
            return View(response);
        }

        [HttpGet]
        public async Task<IActionResult> getPRDetail(string PRNo)
        {
            var response = await _PRPOService.getPRAllDetail(PRNo);

            return Json(response);
        }

        public IActionResult ViewFile(string fileName)
        {
            string pathFile = $"{this._webHostEnvironment.WebRootPath}\\uploadfile\\";
            var filePath = Path.Combine(pathFile, fileName);

            if (System.IO.File.Exists(filePath))
            {

                var fileContent = System.IO.File.ReadAllBytes(filePath);

                var contentType = MimeTypes.GetMimeType(fileName);

                return File(fileContent, contentType);
            }

            // If the file doesn't exist, you can handle the error and return an appropriate response
            return NotFound();
        }

        public async Task<IActionResult> approveRejectPR(string PRNo, string Remark, int approveStatus)
        {

            var informationUser = _accountService.informationUser();

            var response = await _workflowService.approveRejectFlow(informationUser, Remark, PRNo, approveStatus);

            if (response)
            {
                // var getPRRequest = await _eSignPrpoContext.TbPrRequests.Where(x => x.sPoNo == PRNo).FirstOrDefaultAsync();
                // if (getPRRequest.NStatus == 6)
                // {
                //     RunExecute();
                // }
                return Ok(new { msg = PRNo });
            }

            return NotFound(new { msg = PRNo });
        }

        public async Task<IActionResult> approveReprocessPR(string PRNo, string Remark, int approveStatus)
        {

            var informationUser = _accountService.informationUser();

            var response = await _workflowService.approveReprocessFlow(informationUser, Remark, PRNo, approveStatus);

            if (response)
            {
                return Ok(new { msg = PRNo });
            }

            return NotFound(new { msg = PRNo });
        }

        public async Task<IActionResult> cancelPO(string PRNo, string Remark)
        {

            var informationUser = _accountService.informationUser();

            var response = await _workflowService.cancelFlow(informationUser, Remark, PRNo);

            if (response)
            {
                return Ok(new { msg = PRNo });
            }

            return NotFound(new { msg = PRNo });
        }

        public async Task<IActionResult> cancelInvoice(string PRNo, string Remark)
        {

            var informationUser = _accountService.informationUser();

            var response = await _workflowService.cancelFlowInvoice(informationUser, Remark, PRNo);

            if (response)
            {
                return Ok(new { msg = PRNo });
            }

            return NotFound(new { msg = PRNo });
        }
        //public async Task<IActionResult> convertPO(string PRNo, string Remark, int approveStatus)
        //{

        //    var informationUser = _accountService.informationUser();

        //    var response = await _workflowService.convertPOFlow(informationUser, Remark, PRNo, approveStatus);

        //    if (response.Item1)
        //    {

        //        return Ok(new { msg = response.Item2 });
        //    }

        //    return NotFound(new { msg = response.Item2 });
        //}

        private void RunExecute()
        {

            var filename = $"{this._webHostEnvironment.WebRootPath}\\executeFile{_config.GetValue<string>("pathExeFile")}";

            Process p = new Process();
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = filename;
            p.StartInfo.WorkingDirectory = $"{this._webHostEnvironment.WebRootPath}\\executeFile{_config.GetValue<string>("pathDir")}";

            p.Start();
            p.WaitForExit();


        }
        [HttpPost]
        public async Task<IActionResult> confirmEdit(Guid prItemId, string itemDesc, string qty, double? amount)
        {
            var response = await _PRPOService.updatePRItem(prItemId, itemDesc, qty, amount);

            return Ok();

        }

        [Route("/PRPO/SupplierReview/{PRNo}")]
        public async Task<IActionResult> supplierReview(string PRNo)
        {
            var response = new ApproverPRDetailResponse();

            response = await _PRPOService.getPRAllDetail(PRNo);

            var checkBeforeAck = response.flowPRs.Where(x => x.nRW_Steps == 2 && x.sRW_Status == "1").Count();

            if (checkBeforeAck == 0)
            {
                return RedirectToAction("accessDenied", "account");
            }

            return View(response);
        }


        public IActionResult History()
        {
            return View();
        }

        public async Task<IActionResult> getHistory(string dateStart, string dateEnd)
        {

            var getPoHistory = await _PRPOService.getPOHistory(dateStart, dateEnd);

            return Json(new { data = getPoHistory });
        }

        public async Task<IActionResult> PrintAsync(string prNo)
        {
            var response = await _workflowService.generateFile(prNo);

            return File(response, "application/pdf");
        }
        [HttpPost]
        public async Task<IActionResult> getWH(string category, string products)
        {
            var response = await _PRPOService.getWH(category, products);
            return Json(new { data = response });
        }

        [HttpPost]
        public async Task<IActionResult> getVendorEmail(string vendorCode)
        {
            var response = await _PRPOService.getVendorEmail(vendorCode);
            return Json(new { data = response });
        }

        public async Task<IActionResult> ExportAllPR(string datestart, string dateend)
        {
            try
            {
                XLWorkbook wbook2 = new XLWorkbook();

                var wb = wbook2.Worksheets.Add("Sheet 1");

                wb.PageSetup.PaperSize = XLPaperSize.A4Paper;
                wb.Range("A1:N1").Columns().Style.Fill.BackgroundColor = XLColor.BabyBlueEyes;

                wb.Cell("A1").Value = "PO No.";
                wb.Cell("B1").Value = "User Create PO";
                wb.Cell("C1").Value = "User Department";
                wb.Cell("D1").Value = "Vendor Name";
                wb.Cell("E1").Value = "Currency";
                wb.Cell("F1").Value = "Rate";
                wb.Cell("G1").Value = "PO Status";
                wb.Cell("H1").Value = "Total Amount Currency";
                wb.Cell("I1").Value = "Total Amount THB";
                wb.Cell("J1").Value = "PO Created Date";
                wb.Cell("K1").Value = "Date of invoice";
                wb.Cell("L1").Value = "Main Code";
                wb.Cell("M1").Value = "Sub Code 1";
                wb.Cell("N1").Value = "Sub Code 2";


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
                wb.Cell("F1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("G1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("H1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("I1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("J1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("K1").Style
   .Border.SetTopBorder(XLBorderStyleValues.Medium)
   .Border.SetRightBorder(XLBorderStyleValues.Medium)
   .Border.SetBottomBorder(XLBorderStyleValues.Medium)
   .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("L1").Style
   .Border.SetTopBorder(XLBorderStyleValues.Medium)
   .Border.SetRightBorder(XLBorderStyleValues.Medium)
   .Border.SetBottomBorder(XLBorderStyleValues.Medium)
   .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("M1").Style
   .Border.SetTopBorder(XLBorderStyleValues.Medium)
   .Border.SetRightBorder(XLBorderStyleValues.Medium)
   .Border.SetBottomBorder(XLBorderStyleValues.Medium)
   .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("N1").Style
   .Border.SetTopBorder(XLBorderStyleValues.Medium)
   .Border.SetRightBorder(XLBorderStyleValues.Medium)
   .Border.SetBottomBorder(XLBorderStyleValues.Medium)
   .Border.SetLeftBorder(XLBorderStyleValues.Medium);


                #endregion

                wb.RangeUsed().SetAutoFilter();
                wb.Columns().AdjustToContents();

                DateTime dateStart = DateTime.ParseExact(datestart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime dateEnd = DateTime.ParseExact(dateend, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                var getAllPR = await _PRPOService.getAllPrModel(dateStart, dateEnd);

                if (getAllPR.Count > 0)
                {
                    for (int i = 0; i <= (getAllPR.Count - 1); i++)
                    {
                        wb.Cell("A" + (2 + i)).Value = getAllPR[i].poNo;
                        //wb.Cell("A" + (2 + i)).Style.DateFormat.Format = "dd-MM-yy";
                        wb.Cell("B" + (2 + i)).Value = getAllPR[i].createdName;

                        wb.Cell("C" + (2 + i)).Value = getAllPR[i].department;
                        //wb.Cell("C" + (2 + i)).SetDataType(XLDataType.Text);

                        wb.Cell("D" + (2 + i)).Value = getAllPR[i].vendorName;
                        wb.Cell("E" + (2 + i)).Value = getAllPR[i].curr;
                        wb.Cell("F" + (2 + i)).Value = getAllPR[i].rate;
                        wb.Cell("G" + (2 + i)).Value = getAllPR[i].status;
                        wb.Cell("H" + (2 + i)).Value = getAllPR[i].sumAmtCurr;
                        wb.Cell("I" + (2 + i)).Value = getAllPR[i].sumAmtTHB;
                        wb.Cell("J" + (2 + i)).Value = $"'{getAllPR[i].createDate}";
                        wb.Cell("K" + (2 + i)).Value = $"'{getAllPR[i].dateOfInvoice}";
                        wb.Cell("L" + (2 + i)).Value = getAllPR[i].mainCode;
                        wb.Cell("M" + (2 + i)).Value = getAllPR[i].subCode1;
                        wb.Cell("N" + (2 + i)).Value = getAllPR[i].subCode2;

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
                        wb.Cell("F" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("G" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("H" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("I" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("J" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("K" + (2 + i)).Style
  .Border.SetTopBorder(XLBorderStyleValues.Medium)
  .Border.SetRightBorder(XLBorderStyleValues.Medium)
  .Border.SetBottomBorder(XLBorderStyleValues.Medium)
  .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("L" + (2 + i)).Style
  .Border.SetTopBorder(XLBorderStyleValues.Medium)
  .Border.SetRightBorder(XLBorderStyleValues.Medium)
  .Border.SetBottomBorder(XLBorderStyleValues.Medium)
  .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("M" + (2 + i)).Style
  .Border.SetTopBorder(XLBorderStyleValues.Medium)
  .Border.SetRightBorder(XLBorderStyleValues.Medium)
  .Border.SetBottomBorder(XLBorderStyleValues.Medium)
  .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("N" + (2 + i)).Style
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
                    "PO_ExportToExcel.xlsx");
                }
            }
            catch (Exception ex)
            {
                return NotFound("ERROR :" + ex.Message);
            }

        }

        //[HttpGet("rdlc-report-preview")]
        public IActionResult GetRdlcReportPreview(PRPOViewModel prpoRequest, string listPRPOItem)
        {
            var ListPRPO = JsonSerializer.Deserialize<List<listPRPOItem>>(listPRPOItem);
            string reportPath = $"{this._webHostEnvironment.WebRootPath}\\Reports\\PO_Report.rdlc";
            //_logger.LogInformation("Check path : " + reportPath);

            var vendorName = _PRPOService.getVendorName(prpoRequest?.vendorName);


            LocalReport localReport = new LocalReport(reportPath);

            string mimTypes = "";
            int extension = (int)(DateTime.Now.Ticks >> 10);

            DataTable dt1 = new DataTable("ResponsePOReport");
            dt1.Columns.Add("poNo");
            dt1.Columns.Add("datePo");
            dt1.Columns.Add("reference");
            dt1.Columns.Add("department");
            dt1.Columns.Add("vendorName");
            dt1.Columns.Add("shippingDate");
            dt1.Columns.Add("non_Vat");
            dt1.Columns.Add("total_Exclude_Vat");
            dt1.Columns.Add("vat_7");
            dt1.Columns.Add("totalSum_Vat");
            dt1.Columns.Add("prepareBy");
            dt1.Columns.Add("prepareBy_FullName");
            dt1.Columns.Add("remark");

            var sumNon_Vat = ListPRPO.Where(x => x.vatType == "N").Sum(x => double.Parse(x.amount.Replace(",", "")));
            var sumEx_Vat = ListPRPO.Where(x => x.vatType == "E").Sum(x => double.Parse(x.amount.Replace(",", "")));
            var sumIn_Vat = ListPRPO.Where(x => x.vatType == "I").Sum(x => CalculateAmountBeforeVat(double.Parse(x.amount.Replace(",", ""))));

            var sumEx_In_Vat = sumEx_Vat + sumIn_Vat;
            var vat_7 = CalculateVat(sumEx_In_Vat);

            var TotalSum_VAT = sumNon_Vat + sumEx_In_Vat + vat_7;

            dt1.Rows.Add(
                "",
                prpoRequest?.poDate?.ToString("dd-MM-yyyy"),
                prpoRequest?.refQuatation,
                prpoRequest?.department,
                vendorName,
                prpoRequest?.shippingDate?.ToString("dd-MM-yyyy"),
                $"{(sumNon_Vat == 0 ? "-" : sumNon_Vat.ToString("#,##0.00"))}",
                $"{sumEx_In_Vat.ToString("#,##0.00")}",
               $"{vat_7.ToString("#,##0.00")}",
               $"{TotalSum_VAT.ToString("#,##0.00")}"
               , ""
               , ""
               , prpoRequest.reason
                //prpoRequest?.createdBy,
                //prpoRequest?.createdBy

                );

            DataTable dt2 = new DataTable("POItem");
            dt2.Columns.Add("no");
            dt2.Columns.Add("partNo");
            dt2.Columns.Add("partName");
            dt2.Columns.Add("unitPrice");
            dt2.Columns.Add("qty");
            dt2.Columns.Add("amount");


            var i = 1;
            foreach (var itemPo in ListPRPO)
            {

                var doubleParse_unitPrice = double.Parse(itemPo?.unitPrice);
                var doubleParse_amount = double.Parse(itemPo?.amount);
                dt2.Rows.Add(
                    $"{i}",
                    itemPo?.partNo,
                    itemPo?.partName,
                    doubleParse_unitPrice.ToString("#,##0.00"),
                    itemPo?.qty,
                     doubleParse_amount.ToString("#,##0.00")
                    );

                i++;
            }

            //for (int j = 15; j >= i; j--)
            //{
            //    dt2.Rows.Add(
            //        "",
            //        "",
            //        "",
            //        "",
            //       "",
            //        "-"
            //        );

            //}






            localReport.AddDataSource("DataSet1", dt1);
            localReport.AddDataSource("DataSet2", dt2);




            var result = localReport.Execute(RenderType.Pdf, extension, null, mimTypes);


            // Return the PDF report
            return File(result.MainStream, "application/pdf");
        }

        static double CalculateAmountBeforeVat(double totalAmount)
        {
            // สูตร: ยอดเงินก่อน VAT = ยอดเงินรวม VAT / (1 + (VAT / 100))
            var result = (decimal)totalAmount / (1 + (7m / 100));
            return (double)result;
        }
        static double CalculateVat(double amountBeforeVat)
        {
            // สูตร: ยอด VAT = ยอดเงินก่อน VAT * (VAT / 100)
            var result = (decimal)amountBeforeVat * (7m / 100);
            return (double)result;
        }

    }
}
