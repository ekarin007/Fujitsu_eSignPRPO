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
using System;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using System.Net.NetworkInformation;
using DocumentFormat.OpenXml.InkML;
using Azure;
using System.Security.Cryptography;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Office.CustomUI;
using System.Text.RegularExpressions;
using Fujitsu_eSignPO.Services.PRPO;

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

        public async Task<IActionResult> getPOItem(string guid)
        {
            var _guid = Guid.Parse(guid);
            var resp = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.UFkPrid == _guid).ToListAsync();

            return Json(new { data = resp });
        }

        public async Task<IActionResult> AddUpdateItem([FromBody] listPOItem itemRow)
        {
            var resp = new JsonResponse();
            if (itemRow.uPoItemId.IsNullOrEmpty())
            {
                resp = await insertPRItem(itemRow);
            }
            else
            {
                resp = await updatePrItem(itemRow);
            }

            var fkPrID = Guid.Parse(itemRow.fkPrId);

            resp.vat = calVatAfterUpdateItem(fkPrID);


            return Ok(resp);
        }

        public async Task<IActionResult> syncVatAllPO()
        {
            var resp = new JsonResponse();
            try
            {

                var getPO = await _eSignPrpoContext.TbPrRequests.Where(x => x.FVatAmount == null).ToListAsync();

                if (getPO.Count > 0)
                {
                    foreach (var itemPo in getPO)
                    {
                        itemPo.FVatAmount = calVatAfterUpdateItem(itemPo.UPoId);
                    }
                }

                var state = await _eSignPrpoContext.SaveChangesAsync() > 0;

                resp = new JsonResponse { status = state, message = $"Vat synchronization for PO {getPO.Count} rows is completed." };
                return Ok(resp);
            }
            catch (Exception ex)
            {
                resp = new JsonResponse { status = false, message = ex.InnerException.Message };
                return BadRequest(resp);
            }
        }

        public double calVatAfterUpdateItem(Guid? fkPrID)
        {
            var ListPRPO = _eSignPrpoContext.TbPrRequestItems.Where(x => x.UFkPrid == fkPrID).ToList();

            var sumEx_Vat = ListPRPO.Where(x => x.SVatType == "E").Sum(x => x.FAmount);
            var sumIn_Vat = ListPRPO.Where(x => x.SVatType == "I").Sum(x => CalculateAmountBeforeVat((double)x.FAmount));

            var sumEx_In_Vat = sumEx_Vat + sumIn_Vat;
            var vat_7 = CalculateVat((double)sumEx_In_Vat);

            return vat_7;
        }
        public async Task<JsonResponse> insertPRItem(listPOItem listPOItem)
        {
            var fkPrGuid = Guid.Parse(listPOItem.fkPrId);
            var res = new JsonResponse();


            var getPrReq = await _eSignPrpoContext.TbPrRequests.Where(x => x.UPoId == fkPrGuid).FirstOrDefaultAsync();


            try
            {
                var qty = Convert.ToDouble(listPOItem.qty);
                var unitPrice = Convert.ToDouble(listPOItem.unitPrice);
                var newItem = new TbPrRequestItem
                {
                    UPrItemId = Guid.NewGuid(),
                    NNo = Convert.ToInt32(listPOItem.no),
                    SPartNo = listPOItem.partNo,
                    SPartName = listPOItem.partName,
                    SProject = listPOItem.project,
                    FUnitPrice = unitPrice,
                    FQty = qty,
                    FAmount = unitPrice * qty,
                    DCreated = DateTime.Now,
                    NStatus = getPrReq != null ? 1 : 0,
                    UFkPrid = fkPrGuid,
                    SVatType = listPOItem.vatType,
                    SPoNo = getPrReq != null ? getPrReq.SPoNo : null,

                };

                _eSignPrpoContext.TbPrRequestItems.Add(newItem);
                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                return res = new JsonResponse { status = response, message = "Add item completed." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                res = new JsonResponse { status = false, message = ex.InnerException.Message };
                return res;
            }


        }

        public async Task<JsonResponse> updatePrItem(listPOItem listPOItem)
        {
            var informationData = _accountService.informationUser();
            var res = new JsonResponse();
            try
            {
                var itemGuid = Guid.Parse(listPOItem.uPoItemId);

                var getPRItem = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.UPrItemId == itemGuid).FirstOrDefaultAsync();

                if (getPRItem != null)
                {
                    var qty = Convert.ToDouble(listPOItem.qty);
                    var unitPrice = Convert.ToDouble(listPOItem.unitPrice);
                    getPRItem.NNo = Convert.ToInt32(listPOItem.no);
                    getPRItem.SPartNo = listPOItem.partNo;
                    getPRItem.SPartName = listPOItem.partName;
                    getPRItem.SProject = listPOItem.project;
                    getPRItem.FUnitPrice = unitPrice;
                    getPRItem.FQty = qty;
                    getPRItem.FAmount = unitPrice * qty;
                    getPRItem.SVatType = listPOItem.vatType;


                }

                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                return res = new JsonResponse { status = true, message = "update item completed." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                res = new JsonResponse { status = false, message = ex.InnerException.Message };
                return res;
            }

        }

        [HttpPost]
        public async Task<JsonResponse> deleteItemById(string prItemId)
        {
            var res = new JsonResponse();
            try
            {
                var prGuid = Guid.Parse(prItemId);
                var getPRItem = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.UPrItemId == prGuid).FirstOrDefaultAsync();
                var getVat = calVatAfterUpdateItem(getPRItem.UFkPrid);
                _eSignPrpoContext.TbPrRequestItems.Remove(getPRItem);
                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;
                res = new JsonResponse { status = response, message = "delete item completed.", vat = getVat };
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                res = new JsonResponse { status = false, message = ex.InnerException.Message };
                return res;
            }
        }



        public async Task<IActionResult> getPrItemById(string prItemId)
        {
            try
            {
                var prGuid = Guid.Parse(prItemId);
                var getPRItem = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.UPrItemId == prGuid).FirstOrDefaultAsync();
                return Ok(getPRItem);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
                response.projectPath = $"{_config.GetValue<string>("pathURL")}";
                return View(response);
            }

            var getSubCode1 = await _PRPOService.getSubCode1(getPR?.SMainCode);
            ViewBag.subCode1 = getSubCode1;

            var getSubCode2 = await _PRPOService.getSubCode2(getPR?.SSubCode1);
            ViewBag.subCode2 = getSubCode2;

            var getSubCode3 = await _PRPOService.getSubCode3(getPR?.SSubCode2);
            ViewBag.subCode3 = getSubCode3;

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
                poNo = getPR?.SPoNo,
                vendorName = $"{getPR?.SVendorCode}",
                refQuatation = getPR?.SRefQuotation,
                department = getPR?.SDepartment,
                email = getEmailVC,
                shippingDate = getPR?.DShippingDate,
                poDate = getPR?.DPoDate,
                dueDate = getPR?.DDueDate,
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
                projectPath = $"{_config.GetValue<string>("pathURL")}",
                vatAmount = getPR?.FVatAmount?.ToString("#,##0.00"),
                discountAmount = getPR?.FDiscount,
                listPRPOItems = getPRItem.Select(x => new listPRPOItem
                {
                    no = x?.NNo.ToString(),
                    // partNo = x?.SPartNo.Replace("\n", "\\n").Replace("\r", "\\r"),
                    //partName = x?.SPartName.Replace("\n","\\n").Replace("\r","\\r"),
                    partNo = x?.SPartNo,
                    partName = x?.SPartName,
                    project = x?.SProject,
                    vatType = x.SVatType,
                    unitPrice = x.FUnitPrice?.ToString("#,##0.00"),
                    qty = x.FQty?.ToString("0.00"),
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
        public async Task<IActionResult> CreateOrEdit(PRPOViewModel prpoRequest, string gID, string isEdit, string isReSubmit)
        {
            Guid guid = Guid.Parse(gID);

            var requestPR = new Tuple<bool, string>(false, string.Empty);

            if (int.Parse(isEdit) != 1)
            {
                requestPR = await _PRPOService.InsertPR(prpoRequest, guid);
            }
            else
            {
                requestPR = await _PRPOService.UpdatePR(prpoRequest, guid, isReSubmit);
            }
            if (!requestPR.Item1)
            {
                return NotFound(new { status = requestPR.Item1, msg = requestPR.Item2 });
            }

            return Ok(new { status = requestPR.Item1, msg = requestPR.Item2 });

        }

        public async Task<IActionResult> saveDraftPO(PRPOViewModel prpoRequest, string gID)
        {
            Guid guid = Guid.Parse(gID);

            var requestPO = new JsonResponse();

            requestPO = await _PRPOService.InsertUpdatePR_DRAFT(prpoRequest, guid);

            if (!requestPO.status)
            {
                return NotFound(requestPO);
            }

            return Ok(requestPO);

        }



        [HttpPost]
        public async Task<IActionResult> saveReport(PRPOViewModel prpoRequest, string gID, string isEdit)
        {
            Guid guid = Guid.Parse(gID);

            var requestPR = new Tuple<bool, string>(false, string.Empty);

            if (isEdit == "1")
            {
                requestPR = await _PRPOService.UpdatePrByAppr2(prpoRequest, guid);
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

        public async Task<IActionResult> SC1Data(string searchTerm)
        {

            var sC1Data = await _eSignPrpoContext.TbAccountCodes.Select(x => x.SubCode1).Distinct().ToListAsync();

            var filteredOptions = sC1Data;

            if (searchTerm != null)
            {
                filteredOptions = sC1Data.Where(x => x.ToLower().Contains(searchTerm.ToLower()) || x.ToLower().Contains(searchTerm.ToLower())).ToList();
            }

            return Json(filteredOptions.Select(x => new { id = x, text = x }));
        }

        public async Task<IActionResult> SC2Data(string searchTerm)
        {

            var sC2Data = await _eSignPrpoContext.TbAccountCodes.Select(x => x.SubCode2).Distinct().ToListAsync();

            var filteredOptions = sC2Data;

            if (searchTerm != null)
            {
                filteredOptions = sC2Data.Where(x => x.ToLower().Contains(searchTerm.ToLower()) || x.ToLower().Contains(searchTerm.ToLower())).ToList();
            }

            return Json(filteredOptions.Select(x => new { id = x, text = x }));
        }

        public async Task<IActionResult> SC3Data(string searchTerm)
        {

            var sC3Data = await _eSignPrpoContext.TbAccountCodes.Select(x => x.SubCode3).Distinct().ToListAsync();

            var filteredOptions = sC3Data;

            if (searchTerm != null)
            {
                filteredOptions = sC3Data.Where(x => x.ToLower().Contains(searchTerm.ToLower()) || x.ToLower().Contains(searchTerm.ToLower())).ToList();
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

        public async Task<IActionResult> subCode3Data(string searchTerm, string subCode2)
        {

            var getSubCode3Data = await _PRPOService.getSubCode3(subCode2);

            var filteredOptions = getSubCode3Data;

            if (searchTerm != null)
            {
                filteredOptions = getSubCode3Data.Where(x => x.ToLower().Contains(searchTerm.ToLower()) || x.ToLower().Contains(searchTerm.ToLower())).ToList();
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

        [HttpPost]
        public async Task<IActionResult> getBudgetBalance2(string mainCode, string subCode1, string subCode2, string subCode3)
        {
            var getBB = await _PRPOService.getBudgetBalance2(mainCode, subCode1, subCode2, subCode3);

            if (getBB == null)
            {
                return Json(new { budget = 0, balance = 0 });
            }

            return Json(new { budget = getBB.Budget, balance = getBB.Balance });
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
                return BadRequest(ex?.InnerException.Message);
            }
        }

        public async Task<IActionResult> DeleteFile(string fileName, string queryString)
        {
            if (!Guid.TryParse(queryString, out Guid guid))
            {
                return BadRequest("Invalid query string.");
            }

            // ป้องกัน path traversal
            fileName = Path.GetFileName(fileName);

            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploadfile");
            string filePath = Path.Combine(uploadDir, $"{guid}_{fileName}");


            try
            {
                // ลบไฟล์ถ้ามี
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                var delFile = await _PRPOService.DeleteFile(fileName, guid);
                if (!delFile)
                {
                    return NotFound("File deleted from disk, but database update failed.");
                }

                var attList = await _PRPOService.getAttachmentsData(guid);
                return Ok(new { msg = "File deleted successfully (file may or may not have existed).", attList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
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
            //   string decodedFileName = WebUtility.UrlDecode(fileName);
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

            var getPO = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == PRNo).FirstOrDefaultAsync();

            if (getPO != null && getPO.NStatus == 4)
            {
                var checkInvoiceAcceptDate = await _eSignPrpoContext.TbAcceptInvoices.Where(x => x.SPoNo == PRNo).FirstOrDefaultAsync();

                if (checkInvoiceAcceptDate == null)
                {
                    return NotFound(new { msg = $"{PRNo} - Invoice data not found in database !." });
                }

            }

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


        public async Task<IActionResult> History()
        {
            var getVendor = await _PRPOService.getVendorData();
            ViewBag.Vendor = getVendor;

            var getDeparment = await _PRPOService.getDepData();
            ViewBag.departments = getDeparment;


            return View();
        }

        public async Task<IActionResult> getHistory(string dateStart, string dateEnd, string flowStatus, string vendorName, string department, string project, string mc, string sc1, string sc2, string sc3, string reqName)
        {

            var getPoHistory = await _PRPOService.getPOHistory(dateStart, dateEnd, flowStatus, vendorName, department, project, mc, sc1, sc2, sc3, reqName);

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

        public async Task<IActionResult> ExportAllPR(string datestart, string dateend, string flowStatus)
        {
            try
            {
                XLWorkbook wbook2 = new XLWorkbook();

                var wb = wbook2.Worksheets.Add("Sheet 1");

                wb.PageSetup.PaperSize = XLPaperSize.A4Paper;
                wb.Range("A1:O1").Columns().Style.Fill.BackgroundColor = XLColor.BabyBlueEyes;

                wb.Cell("A1").Value = "PO No.";
                wb.Cell("B1").Value = "User Create PO";
                wb.Cell("C1").Value = "User Department";
                wb.Cell("D1").Value = "Vendor Name";
                wb.Cell("E1").Value = "Currency";
                wb.Cell("F1").Value = "Rate";
                wb.Cell("G1").Value = "PO Status";
                wb.Cell("H1").Value = "Total Amount Currency";
                wb.Cell("I1").Value = "Total Amount THB";
                wb.Cell("J1").Value = "PO Date";
                wb.Cell("K1").Value = "Date of invoice";
                wb.Cell("L1").Value = "Main Code";
                wb.Cell("M1").Value = "Sub Code 1";
                wb.Cell("N1").Value = "Sub Code 2";
                wb.Cell("O1").Value = "Budget";


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


                #endregion

                wb.RangeUsed().SetAutoFilter();
                wb.Columns().AdjustToContents();

                DateTime dateStart = DateTime.ParseExact(datestart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime dateEnd = DateTime.ParseExact(dateend, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                var getAllPR = await _PRPOService.getAllPrModel(dateStart, dateEnd, flowStatus);

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
                        wb.Cell("J" + (2 + i)).Value = $"'{getAllPR[i].poDate}";
                        wb.Cell("K" + (2 + i)).Value = $"'{getAllPR[i].dateOfInvoice}";
                        wb.Cell("L" + (2 + i)).Value = getAllPR[i].mainCode;
                        wb.Cell("M" + (2 + i)).Value = getAllPR[i].subCode1;
                        wb.Cell("N" + (2 + i)).Value = getAllPR[i].subCode2;
                        wb.Cell("O" + (2 + i)).Value = getAllPR[i].budget;


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
                return NotFound("ERROR :" + ex.InnerException.Message);
            }

        }

        //[HttpGet("rdlc-report-preview")]
        public IActionResult GetRdlcReportPreview(PRPOViewModel prpoRequest, string gID)
        {
            Guid guid = Guid.Parse(gID);

            var ListPRPO = _eSignPrpoContext.TbPrRequestItems.Where(x => x.UFkPrid == guid).ToList();

            string reportPath = $"{this._webHostEnvironment.WebRootPath}\\Reports\\PO_Report.rdlc";
            //_logger.LogInformation("Check path : " + reportPath);

            var vendorName = _PRPOService.getVendorName(prpoRequest?.vendorName);

            List<GroupedPO> listGroupBy_PO;

            List<string> subCode1Con = new List<string> { "5713 - Research Expenses",
            "6677 - Inspection Fee",
            "5903 - Travelling Expenses For Overseas",
            "5833 - Transportation Taxable" };

            if (subCode1Con.Contains(prpoRequest.subCode1))
            {
                listGroupBy_PO = ListPRPO.OrderBy(x => x.NNo).GroupBy(x => x.SPartNo)
                .Select(g => new GroupedPO
                {
                    partNo = g.Key,
                    partName = g.First().SPartName,
                    vatType = g.First().SVatType,
                    totalQty = g.Sum(x => Convert.ToDouble(x.FQty)),
                    unitPrice = g.First().FUnitPrice?.ToString("#,##0.00"),
                    amount = g.Sum(x => Convert.ToDouble(x.FAmount))
                }).ToList();
            }
            else
            {
                listGroupBy_PO = ListPRPO.OrderBy(x => x.NNo)
                .Select(x => new GroupedPO
                {
                    partNo = x.SPartNo,
                    partName = x.SPartName,
                    vatType = x.SVatType,
                    totalQty = Convert.ToDouble(x.FQty),
                    unitPrice = x.FUnitPrice?.ToString(),
                    amount = Convert.ToDouble(x.FAmount)
                }).ToList();
            }




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
            dt1.Columns.Add("unitPrice_Header");
            dt1.Columns.Add("amount_Header");
            dt1.Columns.Add("project_SubCode");
            dt1.Columns.Add("discount");
            dt1.Columns.Add("subTotal");

           var subTotal = listGroupBy_PO.Sum(x => x.amount);
            var sumNon_Vat = listGroupBy_PO.Where(x => x.vatType == "N").Sum(x => x.amount);
            var sumEx_Vat = listGroupBy_PO.Where(x => x.vatType == "E").Sum(x => x.amount);
            var sumIn_Vat = listGroupBy_PO.Where(x => x.vatType == "I").Sum(x => CalculateAmountBeforeVat(x.amount));

            var sumEx_In_Vat = sumEx_Vat + sumIn_Vat - (prpoRequest.discountAmount ?? 0);
            //var vat_7 = CalculateVat(sumEx_In_Vat);
            var vat_7 = prpoRequest.vatAmount != null ? double.Parse(prpoRequest.vatAmount.Replace(",", "")) : 0;

            var TotalSum_VAT = sumNon_Vat + sumEx_In_Vat + vat_7;

            var checkProjectInList = ListPRPO.Where(x => !String.IsNullOrEmpty(x.SProject)).GroupBy(x => x.SProject).Select(x => x.Key).ToList();

            var etcPrj = checkProjectInList.Count() > 1 ? "***" : "";
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
               , $"Unit Price\n({prpoRequest.currency})"
               , $"Amount\n({prpoRequest.currency})"
               , $"Project : {(checkProjectInList.Count > 0 ? $"{checkProjectInList?.First()}{etcPrj}" : "")}\n" +
                $"Main Code : {prpoRequest.mainCode}\n" +
                $"Sub Code 1: {prpoRequest.subCode1}\n" +
                $"Sub Code 2: {prpoRequest.subCode2}\n" +
                $"{(prpoRequest.mainCode != "INVESTMENT" ? "" : $"Sub Code 3 :{prpoRequest.subCode3}")}",
               $"{prpoRequest?.discountAmount?.ToString("#,##0.00")}",
               $"{subTotal.ToString("#,##0.00")}"
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
            foreach (var itemPo in listGroupBy_PO)
            {

                var doubleParse_unitPrice = double.Parse(itemPo?.unitPrice);
                var doubleParse_amount = itemPo?.amount;
                dt2.Rows.Add(
                    $"{i}",
                    itemPo?.partNo,
                    itemPo?.partName,
                    doubleParse_unitPrice.ToString("#,##0.00"),
                    itemPo?.totalQty,
                     doubleParse_amount?.ToString("#,##0.00")
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


        //public async Task<IActionResult> acceptInvoiceOfficeTools(string PRNo, string Remark, int approveStatus)
        //{

        //    var informationUser = _accountService.informationUser();

        //    var response = await _workflowService.approveRejectFlow(informationUser, Remark, PRNo, approveStatus);

        //    if (response)
        //    {
        //        // var getPRRequest = await _eSignPrpoContext.TbPrRequests.Where(x => x.sPoNo == PRNo).FirstOrDefaultAsync();
        //        // if (getPRRequest.NStatus == 6)
        //        // {
        //        //     RunExecute();
        //        // }
        //        return Ok(new { msg = PRNo });
        //    }

        //    return NotFound(new { msg = PRNo });
        //}
        [HttpPost]
        public async Task<IActionResult> acceptManuInvoice([FromBody] ManufactureData data)
        {
            CultureInfo culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            try
            {
                var chkRemain = await checkRemainBeforeAcceptInvoice(data);

                if (chkRemain)
                {
                    return BadRequest(new { status = false, msg = "Unable to accept invoice because Remain value is less than 0." });
                }

                var insertAcceptInvoice = new TbAcceptInvoice
                {
                    UGuid = Guid.NewGuid(),
                    SPoNo = data.PoNo,
                    DAcceptDate = DateTime.Parse(data.AcceptDate),
                    SInvoiceNo = data.InvoiceNo,
                    FPrice = (double?)data.Price,
                    SType = "Manufacture",
                    FSteel = (double?)data.Steel,
                    FAluminum = (double?)data.Aluminum,
                    FBrass = (double?)data.Brass,
                    FCopper = (double?)data.Copper,
                    FOther = (double?)data.Other,
                    SRemark = data.Remark,
                    DCreatedDate = DateTime.Now,
                };

                _eSignPrpoContext.TbAcceptInvoices.Add(insertAcceptInvoice);

                var getPR = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == data.PoNo).FirstOrDefaultAsync();
                var getBalance = await _PRPOService.getBudgetBalance(getPR.SMainCode, getPR.SSubCode1, getPR.SSubCode2);

                if (getBalance != null)
                {
                    getBalance.Balance = getBalance.Balance - insertAcceptInvoice.FPrice;
                }

                var resp = await _eSignPrpoContext.SaveChangesAsync() > 0;

                return Ok(new { status = resp, msg = data.PoNo });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, msg = ex.InnerException.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> acceptOffToolInvoice([FromBody] ManufactureData data)
        {
            CultureInfo culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            try
            {
                var chkRemain = await checkRemainBeforeAcceptInvoice(data);

                if (chkRemain)
                {
                    return BadRequest(new { status = false, msg = "Unable to accept invoice because Remain value is less than 0." });
                }
                var insertAcceptInvoice = new TbAcceptInvoice
                {
                    UGuid = Guid.NewGuid(),
                    SPoNo = data.PoNo,
                    DAcceptDate = DateTime.Parse(data.AcceptDate),
                    SInvoiceNo = data.InvoiceNo,
                    FPrice = (double?)data.Price,
                    SType = "Office & Tool",
                    SRemark = data.Remark,
                    DCreatedDate = DateTime.Now,
                };

                _eSignPrpoContext.TbAcceptInvoices.Add(insertAcceptInvoice);


                var getPR = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == data.PoNo).FirstOrDefaultAsync();
                var getBalance = await _PRPOService.getBudgetBalance(getPR.SMainCode, getPR.SSubCode1, getPR.SSubCode2);

                if (getBalance != null)
                {
                    getBalance.Balance = getBalance.Balance - insertAcceptInvoice.FPrice;
                }

                var resp = await _eSignPrpoContext.SaveChangesAsync() > 0;

                return Ok(new { status = resp, msg = data.PoNo });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, msg = ex.InnerException.Message });
            }
        }

        public async Task<bool> checkRemainBeforeAcceptInvoice(ManufactureData data)
        {

            var getPRByNo = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == data.PoNo).FirstOrDefaultAsync();
            var getSumAcceptInvoice = await _eSignPrpoContext.TbAcceptInvoices
        .Where(x => x.SPoNo == data.PoNo)
        .SumAsync(x => (double?)x.FPrice) ?? 0;

            double currentInvoicePrice = (double?)data.Price ?? 0;
            double remaining = getPRByNo.FSumAmtThb.Value - (getSumAcceptInvoice + currentInvoicePrice);


            return remaining < 0;
        }

        [HttpPost]
        public async Task<IActionResult> duplicateDraftPO([FromBody] DuplicateRequestModel model)
        {
            try
            {
                var guId = Guid.Parse(model.Guid);
                var originalPo = _eSignPrpoContext.TbPrRequests.FirstOrDefault(x => x.UPoId == guId && x.NStatus == 5);
                if (originalPo == null)
                    return NotFound(new JsonResponse { status = false, message = "PO not found" });

                var newPo = new TbPrRequest
                {
                    UPoId = Guid.NewGuid(),
                    SVendorCode = originalPo.SVendorCode,
                    SVendorName = originalPo.SVendorName,
                    SDepartment = originalPo.SDepartment,
                    SRefQuotation = "",
                    SCurrency = originalPo.SCurrency,
                    FRate = originalPo.FRate,
                    DShippingDate = originalPo.DShippingDate,
                    SMainCode = originalPo.SMainCode,
                    SSubCode1 = originalPo.SSubCode1,
                    SSubCode2 = originalPo.SSubCode2,
                    SSubCode3 = originalPo.SSubCode3,
                    FSumAmtCurrency = originalPo.FSumAmtCurrency,
                    FSumAmtThb = originalPo.FSumAmtThb,
                    SReason = originalPo.SReason,
                    NStatus = -1,
                    SVatType = originalPo.SVatType,
                    DDeliveryDate = originalPo.DDeliveryDate,
                    DPoDate = DateTime.Now,
                    SCreatedBy = originalPo.SCreatedBy,
                    SCreatedName = originalPo.SCreatedName,
                    DCreated = DateTime.Now,
                    DDueDate = originalPo.DDueDate

                };

                _eSignPrpoContext.TbPrRequests.Add(newPo);

                var originalPoItem = _eSignPrpoContext.TbPrRequestItems.Where(x => x.UFkPrid == guId);


                if (originalPoItem.Count() > 0)
                {
                    foreach (var item in originalPoItem)
                    {
                        var newPOItem = new TbPrRequestItem
                        {
                            UPrItemId = Guid.NewGuid(),
                            NNo = item.NNo,
                            SPartNo = item.SPartNo,
                            SPartName = item.SPartName,
                            SProject = item.SProject,
                            SVatType = item.SVatType,
                            FUnitPrice = item.FUnitPrice,
                            FQty = item.FQty,
                            FAmount = item.FAmount,
                            NStatus = 0,
                            DCreated = DateTime.Now,
                            UFkPrid = newPo.UPoId
                        };

                        _eSignPrpoContext.TbPrRequestItems.Add(newPOItem);
                    }
                }

                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                return Ok(new JsonResponse { status = true, message = $"{newPo.UPoId}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new JsonResponse { status = false, message = ex.InnerException.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> deleteDraftPO([FromBody] DuplicateRequestModel model)
        {
            try
            {
                var guId = Guid.Parse(model.Guid);
                var originalPo = _eSignPrpoContext.TbPrRequests.FirstOrDefault(x => x.UPoId == guId && x.NStatus == -1);
                if (originalPo == null)
                    return NotFound(new JsonResponse { status = false, message = "PO not found" });

                _eSignPrpoContext.TbPrRequests.Remove(originalPo);

                var originalPoItem = _eSignPrpoContext.TbPrRequestItems.Where(x => x.UFkPrid == guId && x.NStatus == 0);

                if (originalPoItem.Count() > 0)
                {
                    _eSignPrpoContext.TbPrRequestItems.RemoveRange(originalPoItem);
                }

                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                return Ok(new JsonResponse { status = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new JsonResponse { status = false, message = ex.InnerException.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> deletePO([FromBody] DuplicateRequestModel model)
        {
            try
            {
                var guId = Guid.Parse(model.Guid);
                var originalPo = _eSignPrpoContext.TbPrRequests.FirstOrDefault(x => x.UPoId == guId);
                if (originalPo == null)
                    return NotFound(new JsonResponse { status = false, message = "PO not found" });

                _eSignPrpoContext.TbPrRequests.Remove(originalPo);

                var originalPoItem = _eSignPrpoContext.TbPrRequestItems.Where(x => x.UFkPrid == guId);

                if (originalPoItem.Count() > 0)
                {
                    _eSignPrpoContext.TbPrRequestItems.RemoveRange(originalPoItem);
                }

                var poReviewer = _eSignPrpoContext.TbPrReviewers.Where(x => x.SPoNo == originalPo.SPoNo);
                if (poReviewer.Count() > 0)
                {
                    _eSignPrpoContext.TbPrReviewers.RemoveRange(poReviewer);
                }

                var getAcceptInvoice = await _eSignPrpoContext.TbAcceptInvoices.Where(x => x.SPoNo == originalPo.SPoNo).ToListAsync();

                if (getAcceptInvoice.Count > 0)
                {
                    double? sumPrice = 0.0;
                    foreach (var item in getAcceptInvoice)
                    {
                        sumPrice += item.FPrice;
                    }


                    var refundBalance = await _PRPOService.getBudgetBalance(originalPo.SMainCode, originalPo.SSubCode1, originalPo.SSubCode2);
                    refundBalance.Balance = refundBalance.Balance + sumPrice;

                    _eSignPrpoContext.TbAcceptInvoices.RemoveRange(getAcceptInvoice);
                }


                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                return Ok(new JsonResponse { status = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new JsonResponse { status = false, message = ex.InnerException.Message });
            }
        }

        public class DuplicateRequestModel
        {
            public string Guid { get; set; }
        }

        public async Task<IActionResult> compareForm(string poGuid)
        {
            var response = new comparePOModel();

            response.poGuid = poGuid;

            var guid = Guid.Parse(poGuid);
            var getCompareInfo = await _eSignPrpoContext.TbCompares.Where(x => x.UGuid == guid).FirstOrDefaultAsync();

            if (getCompareInfo != null)
            {
                response.fBudget = getCompareInfo.FBudget;
                response.fBalance = getCompareInfo.FBalance;
                response.sVendor1 = getCompareInfo.SVendor1;
                response.sVendor2 = getCompareInfo.SVendor2;
                response.dVdr1_Quotation_Date = getCompareInfo.DVdr1QuatationDate;
                response.dVdr2_Quotation_Date = getCompareInfo.DVdr2QuatationDate;
                response.sVendorResult = getCompareInfo.SVendorResult;
                response.sRemarkResult = getCompareInfo.SRemarkResult;
                response.fTotalPriceResult = getCompareInfo.FTotalPriceResult;
            }
            

            return PartialView("_comparePartial", response);
        }

        [HttpGet]
        public async Task<IActionResult> GetCompareItemById(string guid)
        {
            var poGuid = Guid.Parse(guid);

            var item = await _eSignPrpoContext.TbCompareLists.Where(x => x.UFkPrid == poGuid).OrderBy(x => x.DCreateDate).ToListAsync();

            return Json(item);
        }

        public async Task<IActionResult> AddCompareItem(CompareItemModel model)
        {
            try
            {
                var poGuid = Guid.Parse(model.poGuid);
                var compareItem = new TbCompareList
                {
                    UGuid = Guid.NewGuid(),
                    DCreateDate = DateTime.Now,
                    FVendorAmount1 = model.Vendor1Amount,
                    FVendorAmount2 = model.Vendor2Amount,
                    SBrandModel = model.BrandModel,
                    SCpItem = model.Item,
                    FAmount = model.Amount,
                    UFkPrid = poGuid

                };
                _eSignPrpoContext.TbCompareLists.Add(compareItem);
                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;
                return Json(new { success = true, message = "add compare item completed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException.Message });
            }


        }

        [HttpPost]
        public async Task<IActionResult> UpdateCompareItem(CompareItemModel model)
        {
            try
            {
                var itemGuid = Guid.Parse(model.Guid); // รหัสของ CompareItem ที่ต้องการแก้ไข
                var compareItem = await _eSignPrpoContext.TbCompareLists.FindAsync(itemGuid);

                if (compareItem == null)
                {
                    return Json(new { success = false, message = "Compare item not found." });
                }

                // อัปเดตข้อมูล
                compareItem.SCpItem = model.Item;
                compareItem.SBrandModel = model.BrandModel;
                compareItem.FAmount = model.Amount;
                compareItem.FVendorAmount1 = model.Vendor1Amount;
                compareItem.FVendorAmount2 = model.Vendor2Amount;
                //compareItem.DUpdateDate = DateTime.Now; // ถ้ามี field สำหรับบันทึกวันแก้ไข

                var result = await _eSignPrpoContext.SaveChangesAsync() > 0;
                return Json(new { success = result, message = result ? "Update successful." : "No changes saved." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCompareItem(string guid)
        {
            try
            {
                var itemGuid = Guid.Parse(guid);
                var item = await _eSignPrpoContext.TbCompareLists.FirstOrDefaultAsync(x => x.UGuid == itemGuid);
                if (item == null) return Json(new { success = false, message = "Not found this item." });

                _eSignPrpoContext.TbCompareLists.Remove(item);
                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                return Json(new { success = true, message = "delete compare item completed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException.Message });
            }
        }

        [HttpPost]
        public async  Task<IActionResult> SaveCompareHeader([FromBody] comparePOModel model)
        {
            try
            {
                var poGuid = Guid.Parse(model.poGuid);
                if (model == null || poGuid == Guid.Empty)
                {
                    return Json(new { success = false, message = "Invalid input." });
                }

                var entity = _eSignPrpoContext.TbCompares.FirstOrDefault(x => x.UGuid == poGuid);

                if (entity != null)
                {
                    entity.FBudget = model.fBudget;
                    entity.FBalance = model.fBalance;
                    entity.SVendor1 = model.sVendor1;
                    entity.SVendor2 = model.sVendor2;
                    entity.SVendorResult = model.sVendorResult;
                    entity.SRemarkResult = model.sRemarkResult;
                    entity.FTotalPriceResult = model.fTotalPriceResult;
                    entity.DVdr1QuatationDate = model.dVdr1_Quotation_Date;
                    entity.DVdr2QuatationDate = model.dVdr2_Quotation_Date;

                    var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                    return Json(new { success = true, message = "Update compare PO completed." });
                }
                else
                {
                    var newEntity = new TbCompare
                    {
                        UGuid = poGuid, // ใช้ Guid เดิม
                        FBudget = model.fBudget,
                        FBalance = model.fBalance,
                        SVendor1 = model.sVendor1,
                        SVendor2 = model.sVendor2,
                        SVendorResult = model.sVendorResult,
                        SRemarkResult = model.sRemarkResult,
                        FTotalPriceResult = model.fTotalPriceResult,
                        DVdr1QuatationDate = model.dVdr1_Quotation_Date,
                        DVdr2QuatationDate = model.dVdr2_Quotation_Date,
                        DCreateDate = DateTime.Now
                    };

                    _eSignPrpoContext.TbCompares.Add(newEntity);
                    var response = await _eSignPrpoContext.SaveChangesAsync() > 0;
                    return Json(new { success = true, message = "Insert compare PO completed." });
                }
              
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException.Message });
            }
        }

        public async Task<IActionResult> ComparePreview(string poNo)
        {
            var model = new compareViewModel();
           
            var getPOReq = await _eSignPrpoContext.TbPrRequests.FirstOrDefaultAsync(x => x.SPoNo == poNo);
            var infoUser = await _eSignPrpoContext.TbEmployees.FirstOrDefaultAsync(x => x.SEmpUsername == getPOReq.SCreatedBy);

            var getCompareInfo = await _eSignPrpoContext.TbCompares.FirstOrDefaultAsync(x => x.UGuid == getPOReq.UPoId);

            var getFiles = await _eSignPrpoContext.TbAttachments.Where(x => x.UPrId == getPOReq.UPoId).ToListAsync();

            if (getCompareInfo != null) {

                model.fBudget = getCompareInfo.FBudget;
                model.fBalance = getCompareInfo.FBalance;
                model.sVendor1 = getCompareInfo.SVendor1;
                model.sVendor2 = getCompareInfo.SVendor2;
                model.dVdr1_Quotation_Date = getCompareInfo.DVdr1QuatationDate;
                model.dVdr2_Quotation_Date = getCompareInfo.DVdr2QuatationDate;
                model.fTotalPriceResult = getCompareInfo.FTotalPriceResult;
                model.sRemarkResult = getCompareInfo.SRemarkResult;
                model.sVendorResult = getCompareInfo.SVendorResult;
                model.dCreateDate = getCompareInfo.DCreateDate;
                model.division = infoUser.SDepartment;
                model.requestor = infoUser.SEmpName;
                model.reqId = infoUser.SEmpUsername;

                var getCompareItem = await _eSignPrpoContext.TbCompareLists.Where(x => x.UFkPrid == getPOReq.UPoId).ToListAsync();
               

                if (getCompareItem.Count > 0)
                {
                    model.compareItem = getCompareItem.OrderBy(x=>x.DCreateDate).Select(x => new CompareItemModel
                    {
                        Item = x.SCpItem,
                        BrandModel = x.SBrandModel,
                        Amount = x.FAmount.Value,
                        Vendor1Amount = x.FVendorAmount1.Value,
                        Vendor2Amount = x.FVendorAmount2.Value,
                        
                       
                    }).ToList();

                    var sumVendor1 = getCompareItem.Sum(x => x.FVendorAmount1);
                    var sumVendor2 = getCompareItem.Sum(x => x.FVendorAmount2);

                    model.fSubPriceAmount = sumVendor2 - sumVendor1;
                }

                if (getFiles.Count >0)
                {
                    model.FileUploads = getFiles.Select(x => new fileUpload
                    {
                        uPrId = x.UPrId,
                        isSendToSupplier = x?.BIsSendSupplier,
                        sAttach_Name = x.SAttachName,
                        sAttach_File_Size = x?.FAttachFileSize?.ToString("0.00"),
                        sAttach_File_Type = x.SAttachFileType,
                        sAttach_Id = x?.UAttachId

                    }).ToList();
                }

             }


           
            return PartialView("_comparePreview", model);
        }

        public async Task<IActionResult> GetCompareItemListById(string guid)
        {
            var citemGuid = Guid.Parse(guid);

            var getCompareItem = await _eSignPrpoContext.TbCompareLists.Where(x => x.UGuid == citemGuid).FirstOrDefaultAsync();

            return Json(getCompareItem);

        }

        public async Task<IActionResult> ReCalculateVat(double discountAmount , string queryString)
        {
            if (!Guid.TryParse(queryString, out Guid guid))
            {
                return BadRequest("Invalid query string.");
            }

            try
            {
                var ListPRPO = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.UFkPrid == guid).ToListAsync();

                var sumEx_Vat = ListPRPO.Where(x => x.SVatType == "E").Sum(x => x.FAmount);
                var sumIn_Vat = ListPRPO.Where(x => x.SVatType == "I").Sum(x => CalculateAmountBeforeVat((double)x.FAmount));

                var sumEx_In_Vat = (sumEx_Vat + sumIn_Vat) - discountAmount;
                var vat_7 = CalculateVat((double)sumEx_In_Vat);

                return Ok(new { data = vat_7 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }


        }

    }
}
