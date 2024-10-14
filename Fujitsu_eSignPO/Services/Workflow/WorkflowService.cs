using AspNetCore.Reporting;
using Fujitsu_eSignPO.Data;
using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.Account;
using Fujitsu_eSignPO.Services.PRPO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;
using Fujitsu_eSignPO.Models.PRPO;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Fujitsu_eSignPO.Services.Workflow
{
    public class WorkflowService : IWorkflowService
    {
        private static FgdtESignPoContext _eSignPrpoContext;
        private readonly ILogger<WorkflowService> _logger;
        private readonly IMailService _mailService;
        private readonly IAccountService _accountService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public WorkflowService(FgdtESignPoContext eSignPrpoContext, ILogger<WorkflowService> logger, IMailService mailService, IAccountService accountService, IWebHostEnvironment webHostEnvironment)
        {
            _eSignPrpoContext = eSignPrpoContext;
            _logger = logger;
            _mailService = mailService;
            _accountService = accountService;
            _webHostEnvironment = webHostEnvironment;
        }

        public List<TbFlow> getStatusFlow() => _eSignPrpoContext.TbFlows.ToList();

        public async Task<bool> generateWorkflow(string department, string prNo)
        {
            try
            {
                var getinfo = _accountService.informationUser();
                var getManager = new TbEmployee();

                if (getinfo.positionLevel == "4")
                {
                    getManager = await _eSignPrpoContext.TbEmployees.Where(x => x.SDepartment == department && x.NPositionLevel == 5).FirstOrDefaultAsync();
                }
                else
                {
                    getManager = await _eSignPrpoContext.TbEmployees.Where(x => x.SDepartment == department && x.NPositionLevel == 1).FirstOrDefaultAsync();
                }

                if (getManager == null)
                {
                    _logger.LogError("someting wrong when getManager !");
                    return false;
                }

                var prReviewerList = new List<TbPrReviewer>();


                var prReviewer = new TbPrReviewer();

                prReviewer = new TbPrReviewer
                {
                    URwId = Guid.NewGuid(),
                    SRwApproveId = getManager?.SEmpUsername,
                    SRwApproveName = getManager?.SEmpName,
                    SRwApproveDepartment = getManager?.SDepartment,
                    SRwApproveTitle = getManager?.SEmpTitle,
                    NRwSteps = 1,
                    NRwStatus = 0,
                    SPoNo = prNo,
                    DCreated = DateTime.Now,

                };

                prReviewerList.Add(prReviewer);


                _eSignPrpoContext.TbPrReviewers.AddRange(prReviewerList);

                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                if (response)
                {
                    var calTotalVAT = await calculateTotalVATAmount(prNo);
                   await _mailService.sendEmail(prNo, 1, 1, null, calTotalVAT);
                }
                _logger.LogInformation($"generate workflow PO : {prNo} is created.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"generate workflow PO Error : {ex.Message}");
                return false;
            }
        }

        public async Task<bool> approveRejectFlow(informationData informationData, string remark, string prNo, int approveStatus)
        {
            var response = false;
            try
            {


                var getPRRequest = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == prNo).FirstOrDefaultAsync();

                if (getPRRequest == null)
                {
                    return false;
                }

                if (getPRRequest.NStatus == 1)
                {
                    var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPoNo == getPRRequest.SPoNo && x.NRwSteps == 1 && x.NRwStatus == 0).FirstOrDefaultAsync();

                    if (getPrReviewer == null)
                    {
                        return false;
                    }

                    getPRRequest.NStatus = 2;
                    getPRRequest.DUpdated = DateTime.Now;

                    if (approveStatus == 9)
                    {
                        RejectPR(getPRRequest.SPoNo);
                        getPRRequest.NStatus = 0;
                        getPrReviewer.BIsReject = true;
                    }

                    getPrReviewer.NRwStatus = approveStatus;
                    getPrReviewer.DRwApproveDate = DateTime.Now;
                    getPrReviewer.SRwRemark = remark;

                    response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                    if (response)
                    {
                        if (approveStatus != 9)
                        {
                            await NextStepToAccountant(getPrReviewer);
                            var calTotalVAT = await calculateTotalVATAmount(prNo);
                            await _mailService.sendEmail(prNo, 2, 1, null , calTotalVAT);
                        }
                        else
                        {
                            var calTotalVAT = await calculateTotalVATAmount(prNo);
                            await _mailService.sendRejectEmail(prNo , calTotalVAT);
                        }
                    }


                    _logger.LogInformation($"PO : {prNo} Status Item {informationData.title} = {getPrReviewer.NRwStatus}{Environment.NewLine}");
                    return response;
                }

                #region Not avaliable
                //if (getPRRequest.NStatus == 2)
                //{
                //    var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPoNo == getPRRequest.SPoNo && x.NRwSteps == 2 && x.NRwStatus == 0).FirstOrDefaultAsync();

                //    if (getPrReviewer == null)
                //    {
                //        return false;
                //    }


                //    getPRRequest.NStatus = 4;
                //    int numMail = 4;
                //    if (getPRRequest.FSumAmtThb > 20000)
                //    {
                //        getPRRequest.NStatus = 3;
                //        numMail = 3;
                //    }


                //    if (approveStatus == 9)
                //    {
                //        RejectPR(getPRRequest.SPoNo);
                //        getPRRequest.NStatus = 0;
                //        getPrReviewer.BIsReject = true;
                //    }
                //    getPrReviewer.SRwApproveId = informationData?.sID;
                //    getPrReviewer.SRwApproveName = informationData?.name;
                //    getPrReviewer.NRwStatus = approveStatus;
                //    getPrReviewer.DRwApproveDate = DateTime.Now;
                //    getPrReviewer.SRwRemark = remark;


                //    response = await _eSignPrpoContext.SaveChangesAsync() > 0;
                //    if (response)
                //    {

                //        if (approveStatus != 9)
                //        {
                //            if (getPRRequest.FSumAmtThb > 20000)
                //            {
                //                await NextStepToGM(getPrReviewer);
                //            }
                //            else
                //            {
                //                await NextStepToPurchOff(getPrReviewer);
                //            }

                //            await _mailService.sendEmail(prNo, numMail, 1, null);
                //        }
                //        else
                //        {
                //            await _mailService.sendRejectEmail(prNo);
                //        }

                //    }


                //    _logger.LogInformation($"PR : {prNo} Status Item {informationData.title} = {getPrReviewer.NRwStatus}{Environment.NewLine}");
                //    return response;

                //}

                //if (getPRRequest.NStatus == 3)
                //{
                //    var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPoNo == getPRRequest.SPoNo && x.NRwSteps == 3 && x.NRwStatus == 0).FirstOrDefaultAsync();

                //    if (getPrReviewer == null)
                //    {
                //        return false;
                //    }

                //    getPRRequest.NStatus = 4;

                //    if (approveStatus == 9)
                //    {
                //        RejectPR(getPRRequest.SPoNo);
                //        getPRRequest.NStatus = 0;
                //        getPrReviewer.BIsReject = true;
                //    }
                //    getPrReviewer.SRwApproveId = informationData?.sID;
                //    getPrReviewer.SRwApproveName = informationData?.name;
                //    getPrReviewer.NRwStatus = approveStatus;
                //    getPrReviewer.DRwApproveDate = DateTime.Now;
                //    getPrReviewer.SRwRemark = remark;

                //    response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                //    if (response)
                //    {
                //        if (approveStatus != 9)
                //        {
                //            await NextStepToPurchOff(getPrReviewer);
                //            await _mailService.sendEmail(prNo, 4, 1, null);
                //        }
                //        else
                //        {
                //            await _mailService.sendRejectEmail(prNo);
                //        }
                //    }
                //    _logger.LogInformation($"PR : {prNo} Status Item {informationData.title} = {getPrReviewer.NRwStatus}{Environment.NewLine}");
                //    return response;

                //}

                #endregion

                if (getPRRequest.NStatus == 2)
                {
                    var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPoNo == getPRRequest.SPoNo && x.NRwSteps == 2 && x.NRwStatus == 0).FirstOrDefaultAsync();

                    if (getPrReviewer == null)
                    {
                        return false;
                    }

                    getPRRequest.NStatus = 4;

                    if (approveStatus == 9)
                    {
                        RejectPR(getPRRequest.SPoNo);
                        getPRRequest.NStatus = 0;
                        getPrReviewer.BIsReject = true;
                    }
                    getPrReviewer.SRwApproveId = informationData?.sID;
                    getPrReviewer.SRwApproveName = informationData?.name;
                    getPrReviewer.NRwStatus = approveStatus;
                    getPrReviewer.DRwApproveDate = DateTime.Now;
                    getPrReviewer.SRwRemark = remark;

                    response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                    if (response)
                    {
                        if (approveStatus != 9)
                        {
                            await NextStepToRegisterDate(getPRRequest);
                            await NextStepToWaitInvoice(getPRRequest);
                            var genFile = await generateFile(getPRRequest?.SPoNo);
                            var calTotalVAT = await calculateTotalVATAmount(prNo);
                            await _mailService.sendEmail(prNo, 3, 2, genFile,calTotalVAT);
                        }
                        else
                        {
                            var calTotalVAT = await calculateTotalVATAmount(prNo);
                            await _mailService.sendRejectEmail(prNo,calTotalVAT);
                        }
                    }

                    _logger.LogInformation($"PO : {prNo} Status Item {informationData.title} = {getPrReviewer.NRwStatus}{Environment.NewLine}");
                    return response;

                }


                //if (getPRRequest.NStatus == 3)
                //{
                //    var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPoNo == getPRRequest.SPoNo && x.NRwSteps == 3 && x.NRwStatus == 0).FirstOrDefaultAsync();

                //    if (getPrReviewer == null)
                //    {
                //        return false;
                //    }

                //    getPRRequest.NStatus = 4;
                //    getPRRequest.DDeliveryDate = DateTime.Parse(remark);
                //    getPrReviewer.SRwApproveId = informationData?.sID;
                //    getPrReviewer.SRwApproveName = informationData?.name;
                //    getPrReviewer.NRwStatus = approveStatus;
                //    getPrReviewer.DRwApproveDate = DateTime.Now;
                //    getPrReviewer.SRwRemark = "Supplier confirmed delivery date.";

                //    response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                //    if (response)
                //    {
                //        await NextStepToWaitInvoice(getPRRequest);
                //        var calTotalVAT = await calculateTotalVATAmount(prNo);
                //        await _mailService.sendEmail(prNo, 4, 3, null,calTotalVAT);
                //    }


                //    _logger.LogInformation($"PO : {prNo} Status Item = {getPrReviewer.NRwStatus}{Environment.NewLine}");
                //    return response;

                //}

                if (getPRRequest.NStatus == 4)
                {
                    var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPoNo == getPRRequest.SPoNo && x.NRwSteps == 4 && x.NRwStatus == 0).FirstOrDefaultAsync();

                    if (getPrReviewer == null)
                    {
                        return false;
                    }

                    getPRRequest.NStatus = 5;
                    getPRRequest.DAcceptIvoiceDate = DateTime.Parse(remark);
                    getPrReviewer.SRwApproveId = informationData?.sID;
                    getPrReviewer.SRwApproveName = informationData?.name;
                    getPrReviewer.NRwStatus = approveStatus;
                    getPrReviewer.DRwApproveDate = DateTime.Now;
                    getPrReviewer.SRwRemark = "Requestor confirmed to accept invoice.";

                    response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                    if (response)
                    {

                        //await _mailService.sendEmail(prNo, 7, 4, null);
                    }


                    _logger.LogInformation($"PO : {prNo} Status Item = {getPrReviewer.NRwStatus}{Environment.NewLine}");
                    return response;

                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return response;
            }
        }

        public async Task<bool> approveReprocessFlow(informationData informationData, string remark, string prNo, int approveStatus)
        {
            var response = false;

            var getPRRequest = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == prNo).FirstOrDefaultAsync();

            if (getPRRequest != null)
            {
                //Update Status at PR Request is 4
                getPRRequest.NStatus = 4;


                var updatePRReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.NRwSteps == 4 && x.SPoNo == prNo && x.NRwStatus != 9).FirstOrDefaultAsync();

                if (updatePRReviewer != null)
                {
                    updatePRReviewer.DRwApproveDate = null;
                    updatePRReviewer.NRwStatus = 0;
                    updatePRReviewer.SRwRemark = null;
                }


                var DeletePrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.NRwSteps > 4 && x.SPoNo == prNo && x.NRwStatus != 9).ToListAsync();

                if (DeletePrReviewer.Count > 0)
                {
                    _eSignPrpoContext.TbPrReviewers.RemoveRange(DeletePrReviewer);
                }


                var insertReprocess = new TbPrReviewer
                {
                    URwId = Guid.NewGuid(),
                    SRwApproveId = informationData?.sID,
                    SRwApproveName = informationData?.name,
                    SRwApproveTitle = informationData?.title,
                    SRwApproveDepartment = informationData?.department,
                    DRwApproveDate = DateTime.Now,
                    NRwSteps = 99,
                    NRwStatus = 1,
                    DCreated = DateTime.Now,
                    SPoNo = prNo,
                    SRwRemark = remark
                };

                _eSignPrpoContext.TbPrReviewers.Add(insertReprocess);

                response = _eSignPrpoContext.SaveChanges() > 0;

                _logger.LogInformation($"PO : {prNo} is Reprocess by  [{informationData?.sID}] {informationData?.name} {Environment.NewLine}");
            }

            return response;

        }


        public void RejectPR(string prNo)
        {
            var listReviewer = _eSignPrpoContext.TbPrReviewers.Where(x => x.SPoNo == prNo).ToList();
            foreach (var itemReviewer in listReviewer)
            {
                itemReviewer.NRwStatus = 9;
            }


        }
        public async Task<bool> NextStepToAccountant(TbPrReviewer _reviewer)
        {

            var resp = false;
            var getAccountant = await _eSignPrpoContext.TbEmployees.Where(x => x.NPositionLevel == 2).FirstOrDefaultAsync();

            if (getAccountant == null)
            {
                _logger.LogError("someting wrong when getAccountant !");
                return resp;
            }

            var reviewer = new TbPrReviewer
            {
                URwId = Guid.NewGuid(),
                SRwApproveDepartment = getAccountant?.SDepartment,
                SRwApproveTitle = getAccountant?.SEmpTitle,
                NRwSteps = 2,
                NRwStatus = 0,
                SPoNo = _reviewer.SPoNo,
                DCreated = DateTime.Now,

            };

            _eSignPrpoContext.TbPrReviewers.Add(reviewer);

            resp = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return resp;
        }


        public async Task<bool> NextStepToRegisterDate(TbPrRequest _request)
        {
            var resp = false;

            var getVendorName = _eSignPrpoContext.TbVendors.Where(x => x.VendorCode == _request.SVendorCode).FirstOrDefault();
            var reviewer = new TbPrReviewer
            {
                URwId = Guid.NewGuid(),
                SRwApproveId = _request.SVendorCode,
                SRwApproveName = getVendorName.VendorName,
                SRwApproveDepartment = string.Empty,
                SRwApproveTitle = "Supplier Register Delivery Date",
                NRwSteps = 3,
                NRwStatus = 1,
                SPoNo = _request.SPoNo,
                DCreated = DateTime.Now,
                DRwApproveDate = DateTime.Now,
                SRwRemark = "System Bypass"
            };

            _eSignPrpoContext.TbPrReviewers.Add(reviewer);

            resp = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return resp;
        }

        public async Task<bool> NextStepToWaitInvoice(TbPrRequest _request)
        {
            var resp = false;
            var getRequestor = await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpUsername == _request.SCreatedBy).FirstOrDefaultAsync();

            if (getRequestor == null)
            {
                _logger.LogError("someting wrong when get Requestor !");
                return resp;
            }

            var reviewer = new TbPrReviewer
            {
                URwId = Guid.NewGuid(),
                SRwApproveId = getRequestor?.SEmpUsername,
                SRwApproveName = getRequestor?.SEmpName,
                SRwApproveDepartment = getRequestor?.SDepartment,
                SRwApproveTitle = getRequestor?.SEmpTitle,
                NRwSteps = 4,
                NRwStatus = 0,
                SPoNo = _request?.SPoNo,
                DCreated = DateTime.Now,

            };

            _eSignPrpoContext.TbPrReviewers.Add(reviewer);

            resp = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return resp;
        }


        public async Task<double> calculateTotalVATAmount(string prNo)
        {
            var res = await getPRAllDetail(prNo);
            var sumNon_Vat = res.listPRPOItems.Where(x => x.vatType == "N").Sum(x => double.Parse(x.amount.Replace(",", "")));
            var sumEx_Vat = res.listPRPOItems.Where(x => x.vatType == "E").Sum(x => double.Parse(x.amount.Replace(",", "")));
            var sumIn_Vat = res.listPRPOItems.Where(x => x.vatType == "I").Sum(x => CalculateAmountBeforeVat(double.Parse(x.amount.Replace(",", ""))));

            var sumEx_In_Vat = sumEx_Vat + sumIn_Vat;
            var vat_7 = CalculateVat(sumEx_In_Vat);

            var TotalSum_VAT = sumNon_Vat + sumEx_In_Vat + vat_7;

            return TotalSum_VAT;
        }
        public async Task<byte[]> generateFile(string prNo)
        {
            var path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\PO_Report.rdlc";

            var res = await getPRAllDetail(prNo);

            LocalReport localReport = new LocalReport(path);

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

            var sumNon_Vat = res.listPRPOItems.Where(x => x.vatType == "N").Sum(x => double.Parse(x.amount.Replace(",", "")));
            var sumEx_Vat = res.listPRPOItems.Where(x => x.vatType == "E").Sum(x => double.Parse(x.amount.Replace(",", "")));
            var sumIn_Vat = res.listPRPOItems.Where(x => x.vatType == "I").Sum(x => CalculateAmountBeforeVat(double.Parse(x.amount.Replace(",", ""))));

            var sumEx_In_Vat = sumEx_Vat + sumIn_Vat;
            var vat_7 = CalculateVat(sumEx_In_Vat);

            var TotalSum_VAT = sumNon_Vat + sumEx_In_Vat + vat_7;

            dt1.Rows.Add(
                res?.poNo,
                res?.poDate,
                res?.refQuotation,
                res?.department,
                res?.vendorName,
                res?.shippingDate,
              $"{(sumNon_Vat == 0 ? "-" : sumNon_Vat.ToString("N"))}",
                $"{sumEx_In_Vat.ToString("N")}",
               $"{vat_7.ToString("N")}",
               $"{TotalSum_VAT.ToString("N")}",
               res?.createdBy,
               res?.createdBy,
               res?.reason


                );

            DataTable dt2 = new DataTable("POItem");
            dt2.Columns.Add("no");
            dt2.Columns.Add("partNo");
            dt2.Columns.Add("partName");
            dt2.Columns.Add("unitPrice");
            dt2.Columns.Add("qty");
            dt2.Columns.Add("amount");

            var i = 1;
            foreach (var itemPo in res.listPRPOItems)
            {
                dt2.Rows.Add(
                    $"{i}",
                    itemPo?.partNo,
                    itemPo?.partName,
                    itemPo?.unitPrice,
                    itemPo?.qty,
                     itemPo?.amount
                    );

                i++;
            }

            for (int j = 15; j >= i; j--)
            {
                dt2.Rows.Add(
                    "",
                    "",
                    "",
                    "",
                   "",
                    "-"
                    );

            }

            localReport.AddDataSource("DataSet1", dt1);
            localReport.AddDataSource("DataSet2", dt2);


            var getPrReview = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPoNo == prNo).ToListAsync();




            var signPM = "";
            var checkManager = getPrReview.Where(x => x.NRwSteps == 1 && x.NRwStatus == 1).FirstOrDefault();
            if (checkManager != null)
            {
                signPM = convertToBase(checkManager.SRwApproveId);
            }



            var signAcc = "";
            var checkAccountant = getPrReview.Where(x => x.NRwSteps == 2 && x.NRwStatus == 1).FirstOrDefault();
            if (checkAccountant != null)
            {
                signAcc = convertToBase(checkAccountant.SRwApproveId);
            }

            var param = new Dictionary<string, string>
            {
                { "MgrImage" , signPM },
                {
                    "AccImage" , signAcc
                }
            };

            var result = localReport.Execute(RenderType.Pdf, extension, param, mimTypes);

            return result.MainStream;
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

        public string convertToBase(string fileName)
        {
            string sign = "";
            try
            {
                var imagePath = $"{this._webHostEnvironment.WebRootPath}\\signature\\{fileName}.png";

                if (!File.Exists(imagePath))
                {
                    return sign;
                }

                using (var b = new Bitmap(imagePath))
                {
                    using (var ms = new MemoryStream())
                    {
                        b.Save(ms, ImageFormat.Bmp);
                        sign = Convert.ToBase64String(ms.ToArray());
                    }
                }

                return sign;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return sign;
            }
        }

        public async Task<ApproverPRDetailResponse> getPRAllDetail(string prNo)
        {

            var response = new ApproverPRDetailResponse();
            try
            {
                var getPRByNo = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == prNo).FirstOrDefaultAsync();

                var getPRItemByNo = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.SPoNo == prNo).ToListAsync();

                var getFiles = await _eSignPrpoContext.TbAttachments.Where(x => x.UPrId == getPRByNo.UPoId).ToListAsync();

                var getFlowPR = await _eSignPrpoContext.TbPrReviewers.OrderBy(x => x.NRwStatus).Where(x => x.SPoNo == prNo && x.NRwStatus != 9).ToListAsync(); ;

                var getRejectFlow = await _eSignPrpoContext.TbPrReviewers.OrderByDescending(x => x.DCreated).Where(x => x.SPoNo == prNo && x.NRwStatus == 9 && x.BIsReject == true).ToListAsync();


                var informationData = _accountService.informationUser();

                var checkPermision = getFlowPR.Where(x => (x.SRwApproveId == informationData.sID || x.SRwApproveTitle == informationData.title) && x.NRwStatus == 0).Count();
                var getVendorName = await _eSignPrpoContext.TbVendors.Where(x => x.VendorCode == getPRByNo.SVendorCode).FirstOrDefaultAsync();

                response = new ApproverPRDetailResponse();

                response.checkPermission = checkPermision > 0;

                response.poNo = getPRByNo?.SPoNo;
                response.createdDate = getPRByNo?.DCreated;

                response.totalAmount = getPRByNo?.FSumAmtCurrency?.ToString("N");
                response.totalAmountTHB = getPRByNo?.FSumAmtThb?.ToString("N");
                //response.vatTotal = getVat?.ToString("N");
                //response.totalAmountVatTHB = (getPRByNo?.FSumAmtThb + getVat)?.ToString("N");

                response.vendorName = $"{getVendorName.VendorName}";

                response.reason = getPRByNo?.SReason;
                response.status = getPRByNo.NStatus;
                response.department = getPRByNo?.SDepartment;
                response.createdBy = getPRByNo?.SCreatedName;
                response.deliveryDate = getPRByNo?.DDeliveryDate?.ToString("dd/MM/yyyy");
                response.currency = getPRByNo?.SCurrency;
                response.poDate = getPRByNo?.DPoDate?.ToString("dd-MM-yyyy");
                response.shippingDate = getPRByNo?.DShippingDate?.ToString("dd-MM-yyyy");
                response.refQuotation = getPRByNo?.SRefQuotation;
                response.listPRPOItems = getPRItemByNo.OrderBy(x => x.NNo).Select(x => new listPRPOItem
                {
                    uPoItemId = x?.UPrItemId,
                    no = x?.NNo?.ToString(),
                    partNo = x?.SPartNo,
                    partName = x?.SPartName,
                    unitPrice = x?.FUnitPrice?.ToString("N"),
                    qty = x?.NQty.ToString(),
                    amount = x?.FAmount?.ToString("N"),
                    vatType = x?.SVatType


                }).ToList();
                response.fileUploads = getFiles.Select(x => new fileUpload
                {
                    uPrId = x.UPrId,
                    sAttach_Name = x.SAttachName,
                    sAttach_File_Size = x?.FAttachFileSize?.ToString("0.00"),
                    sAttach_File_Type = x.SAttachFileType

                }).ToList();
                response.flowPRs = getFlowPR.Select(x => new flowPR
                {
                    sRw_Approve_ID = x.SRwApproveId,
                    nRW_Steps = x.NRwSteps,
                    sRw_Approve_Name = x.SRwApproveName,
                    sRW_Approve_Title = x.SRwApproveTitle,
                    dRW_Approve_Date = x.DRwApproveDate,
                    sRw_Remark = x.SRwRemark,
                    sRW_Status = x.NRwStatus.ToString()
                }).OrderBy(x => x.nRW_Steps).ToList();
                response.flowReject = getRejectFlow.Select(x => new flowPR
                {
                    sRw_Approve_ID = x.SRwApproveId,
                    nRW_Steps = x.NRwSteps,
                    sRw_Approve_Name = x.SRwApproveName,
                    sRW_Approve_Title = x.SRwApproveTitle,
                    dRW_Approve_Date = x.DRwApproveDate,
                    sRw_Remark = x.SRwRemark,
                    sRW_Status = x.NRwStatus.ToString()
                }).OrderBy(x => x.nRW_Steps).ToList();


                return response;
            }
            catch (Exception ex)
            {
                return response;
            }

        }

    }
}
