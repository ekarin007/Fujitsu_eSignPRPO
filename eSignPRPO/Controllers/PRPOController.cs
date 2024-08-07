using eSignPRPO.interfaces;
using eSignPRPO.Models.PRPO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using MimeKit;
using System.Data;
using ClosedXML.Excel;
using System.Globalization;
using eSignPRPO.Models;
using System.Diagnostics;
using eSignPRPO.Data;
using Microsoft.EntityFrameworkCore;

namespace eSignPRPO.Controllers
{
    [Authorize(Roles = "0,1,2,3,4,5,99")]
    public class PRPOController : Controller
    {
        private readonly IPRPOService _PRPOService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAccountService _accountService;
        private readonly IWorkflowService _workflowService;
        private readonly IConfiguration _config;
        private readonly ESignPrpoContext _eSignPrpoContext;
        public PRPOController(IPRPOService PRPOService, IWebHostEnvironment webHostEnvironment, IAccountService accountService, IWorkflowService workflowService, IConfiguration config, ESignPrpoContext eSignPrpoContext)
        {
            _PRPOService = PRPOService;
            _webHostEnvironment = webHostEnvironment;
            _accountService = accountService;
            _workflowService = workflowService;
            _config = config;
            _eSignPrpoContext = eSignPrpoContext;
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
            var response = new PRPOViewModel();

            var getSupplier = await _PRPOService.getSupplierData();

            ViewBag.Supplier = getSupplier;

            var getPR = await _PRPOService.getPrRequestByNo(gID);

            if (getPR == null)
            {
                return View(response);
            }

            var getPRItem = await _PRPOService.getPrRequestItemByNo(getPR?.SPrNo);

            if (getPRItem == null)
            {
                return View(response);
            }

            var getAttData = await _PRPOService.getAttachmentsData(gID);

            response = new PRPOViewModel
            {
                supplierName = $"{getPR?.SSupplierCode}|{getPR?.SSupplierName}",
                categoryOption = getPR?.SCategory,
                capexNo = getPR?.SCapexNo,
                assetOption = getPR?.STypeAsset,
                assetName = getPR?.SAssentName,
                wh = getPR?.SWh,
                refAssetNo = getPR?.SAssetNo,
                productsOption = getPR?.SProduct,
                reason = getPR?.SReason == null ? "" : getPR?.SReason.Replace("\n", "").Replace("\r", ""),
                totalAmount = getPR?.FSumAmtCurrency?.ToString("N"),
                totalAmountTHB = getPR?.FSumAmtThb?.ToString("N"),
                nStatus = getPR?.NStatus,
                rate = getPR?.FRate,
                bIsVat = getPR?.BIsVat,
                listPRPOItems = getPRItem.Select(x => new listPRPOItem
                {
                    no = x?.NNo.ToString(),
                    item = x?.SItem,
                    itemDesc = x?.SItemDesc,
                    currency = x.SCurrency,
                    amount = x?.FAmount?.ToString("N"),
                    costCenter = x?.SCostCenter,
                    glCode = x?.SGlCode,
                    qty = x?.NQty.ToString(),
                    requestDate = x?.DRequestDate?.ToString("yyyy-MM-dd"),
                    unitCost = x?.FUnitCost?.ToString("N"),
                    Uom = x?.SUom

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

        public async Task<IActionResult> ItemCodeAutocomplete(string term, string typeCategory)
        {

            var getItemCode = await _PRPOService.getItemCode(typeCategory);

            var autocompleteData = getItemCode.Select(x => $"{x.TItem},{x.TDsca}");

            var filteredData = autocompleteData.Where(item => item.ToLower().Contains(term.ToLower()));

            return Json(filteredData);
        }

        public async Task<IActionResult> getGlCodeData(string searchTerm)
        {
            var getGLData = await _PRPOService.getGlCode();

            var filteredOptions = getGLData;

            if (searchTerm != null)
            {
                filteredOptions = getGLData.Where(x => x.TDesc.Contains(searchTerm) || x.TLeac.Contains(searchTerm)).ToList();
            }

            return Json(filteredOptions.Select(x => new { id = x.TLeac, text = $"{x.TLeac}|{x.TDesc}" }));
        }

        public async Task<IActionResult> getCostCenterData(string searchTerm)
        {
            var getCostCenterData = await _PRPOService.getCostCenter();

            var filteredOptions = getCostCenterData;

            if (searchTerm != null)
            {
                filteredOptions = getCostCenterData.Where(x => x.TDimx.Contains(searchTerm) || x.TDesc.Contains(searchTerm)).ToList();
            }

            return Json(filteredOptions.Select(x => new { id = x.TDimx, text = $"{x.TDimx}|{x.TDesc}" }));
        }

        public async Task<IActionResult> getCurrency(string searchTerm)
        {
            var getCurrencyData = await _PRPOService.getDistinctCurrency();

            var filteredOptions = getCurrencyData;

            if (searchTerm != null)
            {
                filteredOptions = getCurrencyData.Where(x => x.Contains(searchTerm)).ToList();
            }

            return Json(filteredOptions.Select(x => new { id = x, text = $"{x}" }));
        }

        public async Task<IActionResult> GetItemPrice(string itemCode)
        {
            var getItemDesc = await _PRPOService.getItemDataByCode(itemCode);
            var getItemPrice = await _PRPOService.getItemPrice(itemCode);

            return Json(new { itemDesc = getItemDesc.TDsca, itemPrice = getItemPrice });
        }

        public async Task<IActionResult> getSupplierByCode(string supID)
        {
            var getSupplier = await _PRPOService.getSupplierByID(supID);

            return Ok(getSupplier);
        }

        public async Task<IActionResult> getRateByCurr(string curr)
        {
            var getRate = await _PRPOService.getRateByCurrency(curr);

            return Ok(getRate);
        }

        public async Task<IActionResult> getPrRecords()
        {
            var getPrRecords = await _PRPOService.getPrRecords();

            return Json(new { data = getPrRecords });


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
               // var getPRRequest = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPrNo == PRNo).FirstOrDefaultAsync();
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

        public async Task<IActionResult> isVat(string prNo, string isChecked)
        {

            var response = await _workflowService.isVat(prNo, isChecked);

            if (response)
            {
                return Ok(new { status = response });
            }

            return NotFound(new { status = response });
        }

        public async Task<IActionResult> convertPO(string PRNo, string Remark, int approveStatus)
        {

            var informationUser = _accountService.informationUser();

            var response = await _workflowService.convertPOFlow(informationUser, Remark, PRNo, approveStatus);

            if (response.Item1)
            {

                return Ok(new { msg = response.Item2 });
            }

            return NotFound(new { msg = response.Item2 });
        }

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

            var checkBeforeAck = response.flowPRs.Where(x => x.nRW_Steps == 5 && x.sRW_Status == "1").Count();

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

        public async Task<IActionResult> ExportAllPR(string datestart, string dateend)
        {
            try
            {
                XLWorkbook wbook2 = new XLWorkbook();

                var wb = wbook2.Worksheets.Add("Sheet 1");

                wb.PageSetup.PaperSize = XLPaperSize.A4Paper;
                wb.Range("A1:W1").Columns().Style.Fill.BackgroundColor = XLColor.BabyBlueEyes;

                wb.Cell("A1").Value = "PR No";
                wb.Cell("B1").Value = "User Create PR";
                wb.Cell("C1").Value = "PR Issued Date";
                wb.Cell("D1").Value = "Transaction Date";
                wb.Cell("E1").Value = "PO No.";
                wb.Cell("F1").Value = "User Create PO";
                wb.Cell("G1").Value = "PO Issued Date";
                wb.Cell("H1").Value = "Supplier Code";
                wb.Cell("I1").Value = "Supplier Name";
                wb.Cell("J1").Value = "Reference A";
                wb.Cell("K1").Value = "Capex No.";
                wb.Cell("L1").Value = "Asset Name";
                wb.Cell("M1").Value = "Ref. Asset";
                wb.Cell("N1").Value = "Location";
                wb.Cell("O1").Value = "Requisition Type";
                wb.Cell("P1").Value = "PO No";
                wb.Cell("Q1").Value = "PO Issued Date2";
                wb.Cell("R1").Value = "Supplier Code3";
                wb.Cell("S1").Value = "Supplier Name4";
                wb.Cell("T1").Value = "Reference A5";
                wb.Cell("U1").Value = "PO Status";
                wb.Cell("V1").Value = "Expect Delivery Date";
                wb.Cell("W1").Value = "Warehouse";

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
                wb.Cell("O1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("P1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("Q1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("R1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("S1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("T1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("U1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("V1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("W1").Style
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
                        wb.Cell("A" + (2 + i)).Value = getAllPR[i].prNo;
                        //wb.Cell("A" + (2 + i)).Style.DateFormat.Format = "dd-MM-yy";
                        wb.Cell("B" + (2 + i)).Value = getAllPR[i].userCreatePR;

                        wb.Cell("C" + (2 + i)).Value = $"'{getAllPR[i].prIssuedDate}";
                        //wb.Cell("C" + (2 + i)).SetDataType(XLDataType.Text);

                        wb.Cell("D" + (2 + i)).Value = $"'{getAllPR[i].transactionDate}";
                        wb.Cell("E" + (2 + i)).Value = getAllPR[i].poNo;
                        wb.Cell("F" + (2 + i)).Value = getAllPR[i].userCreatePO;
                        wb.Cell("G" + (2 + i)).Value = $"'{getAllPR[i].poIssuedDate}";
                        wb.Cell("H" + (2 + i)).Value = getAllPR[i].supplierCode;
                        wb.Cell("I" + (2 + i)).Value = getAllPR[i].supplierName;
                        wb.Cell("J" + (2 + i)).Value = getAllPR[i].ReferenceA;
                        wb.Cell("K" + (2 + i)).Value = getAllPR[i].capexNo;

                        wb.Cell("L" + (2 + i)).Value = $"'{getAllPR[i].assetName}";
                        //wb.Cell("L" + (2 + i)).SetDataType(XLDataType.Text);

                        wb.Cell("M" + (2 + i)).Value = getAllPR[i].refAsset;
                        wb.Cell("N" + (2 + i)).Value = getAllPR[i].location;
                        wb.Cell("O" + (2 + i)).Value = getAllPR[i].requisitionType;
                        wb.Cell("P" + (2 + i)).Value = getAllPR[i].poNo;
                        wb.Cell("Q" + (2 + i)).Value = $"'{getAllPR[i].poIssuedDate}";
                        wb.Cell("R" + (2 + i)).Value = getAllPR[i].supplierCode;
                        wb.Cell("S" + (2 + i)).Value = getAllPR[i].supplierName;
                        wb.Cell("T" + (2 + i)).Value = getAllPR[i].ReferenceA;
                        wb.Cell("U" + (2 + i)).Value = getAllPR[i].poStatus;
                        wb.Cell("V" + (2 + i)).Value = $"'{getAllPR[i].expectDeliveryDate}";
                        wb.Cell("W" + (2 + i)).Value = getAllPR[i].warehouse;
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
                        wb.Cell("O" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("P" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("Q" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("R" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("S" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("T" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("U" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("V" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("W" + (2 + i)).Style
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
                    "PR_ExportToExcel.xlsx");
                }
            }
            catch (Exception ex)
            {
                return NotFound("ERROR :" + ex.Message);
            }

        }
    }
}
