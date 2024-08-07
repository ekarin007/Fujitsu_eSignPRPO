using AspNetCore.Reporting;
using eSignPRPO.Data;
using eSignPRPO.interfaces;
using eSignPRPO.Models;
using eSignPRPO.Models.Account;
using eSignPRPO.Services.PRPO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;
using eSignPRPO.Models.PRPO;
using System.Linq;

namespace eSignPRPO.Services.Workflow
{
    public class WorkflowService : IWorkflowService
    {
        private static ESignPrpoContext _eSignPrpoContext;
        private readonly ILogger<WorkflowService> _logger;
        private readonly IMailService _mailService;
        private readonly IAccountService _accountService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public WorkflowService(ESignPrpoContext eSignPrpoContext, ILogger<WorkflowService> logger, IMailService mailService, IAccountService accountService, IWebHostEnvironment webHostEnvironment)
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
                    SPrNo = prNo,
                    DCreated = DateTime.Now,

                };

                prReviewerList.Add(prReviewer);


                _eSignPrpoContext.TbPrReviewers.AddRange(prReviewerList);

                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;
                _logger.LogInformation($"Work flow PR : {prNo} is created.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> approveRejectFlow(informationData informationData, string remark, string prNo, int approveStatus)
        {
            var response = false;
            try
            {


                var getPRRequest = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPrNo == prNo).FirstOrDefaultAsync();

                if (getPRRequest == null)
                {
                    return false;
                }

                if (getPRRequest.NStatus == 1)
                {
                    var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPrNo == getPRRequest.SPrNo && x.NRwSteps == 1 && x.NRwStatus == 0).FirstOrDefaultAsync();

                    if (getPrReviewer == null)
                    {
                        return false;
                    }

                    getPRRequest.NStatus = 2;

                    if (approveStatus == 9)
                    {
                        RejectPR(getPRRequest.SPrNo);
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
                            await NextStepToFinance(getPrReviewer);
                            await _mailService.sendEmail(prNo, 2, 1, null);
                        }
                        else
                        {
                            await _mailService.sendRejectEmail(prNo);
                        }
                    }


                    _logger.LogInformation($"PR : {prNo} Status Item {informationData.title} = {getPrReviewer.NRwStatus}{Environment.NewLine}");
                    return response;
                }

                if (getPRRequest.NStatus == 2)
                {
                    var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPrNo == getPRRequest.SPrNo && x.NRwSteps == 2 && x.NRwStatus == 0).FirstOrDefaultAsync();

                    if (getPrReviewer == null)
                    {
                        return false;
                    }


                    getPRRequest.NStatus = 4;
                    int numMail = 4;
                    if (getPRRequest.FSumAmtThb > 20000)
                    {
                        getPRRequest.NStatus = 3;
                        numMail = 3;
                    }


                    if (approveStatus == 9)
                    {
                        RejectPR(getPRRequest.SPrNo);
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
                            if (getPRRequest.FSumAmtThb > 20000)
                            {
                                await NextStepToGM(getPrReviewer);
                            }
                            else
                            {
                                await NextStepToPurchOff(getPrReviewer);
                            }

                            await _mailService.sendEmail(prNo, numMail, 1, null);
                        }
                        else
                        {
                            await _mailService.sendRejectEmail(prNo);
                        }

                    }


                    _logger.LogInformation($"PR : {prNo} Status Item {informationData.title} = {getPrReviewer.NRwStatus}{Environment.NewLine}");
                    return response;

                }

                if (getPRRequest.NStatus == 3)
                {
                    var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPrNo == getPRRequest.SPrNo && x.NRwSteps == 3 && x.NRwStatus == 0).FirstOrDefaultAsync();

                    if (getPrReviewer == null)
                    {
                        return false;
                    }

                    getPRRequest.NStatus = 4;

                    if (approveStatus == 9)
                    {
                        RejectPR(getPRRequest.SPrNo);
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
                            await NextStepToPurchOff(getPrReviewer);
                            await _mailService.sendEmail(prNo, 4, 1, null);
                        }
                        else
                        {
                            await _mailService.sendRejectEmail(prNo);
                        }
                    }
                    _logger.LogInformation($"PR : {prNo} Status Item {informationData.title} = {getPrReviewer.NRwStatus}{Environment.NewLine}");
                    return response;

                }

                if (getPRRequest.NStatus == 5)
                {
                    var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPrNo == getPRRequest.SPrNo && x.NRwSteps == 5 && x.NRwStatus == 0).FirstOrDefaultAsync();

                    if (getPrReviewer == null)
                    {
                        return false;
                    }

                    getPRRequest.NStatus = 6;

                    if (approveStatus == 9)
                    {
                        RejectPR(getPRRequest.SPrNo);
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

                            var genFile = await generateFile(getPRRequest?.SPrNo);

                            await _mailService.sendEmail(prNo, 6, 5, genFile);
                        }
                        else
                        {
                            await _mailService.sendRejectEmail(prNo);
                        }
                    }

                    _logger.LogInformation($"PR : {prNo} Status Item {informationData.title} = {getPrReviewer.NRwStatus}{Environment.NewLine}");
                    return response;

                }

                //if (getPRRequest.NStatus == 6)
                //{
                //    var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPrNo == getPRRequest.SPrNo && x.NRwSteps == 6 && x.NRwStatus == 0).FirstOrDefaultAsync();

                //    if (getPrReviewer == null)
                //    {
                //        return false;
                //    }

                //    getPRRequest.NStatus = 7;

                //    getPrReviewer.SRwApproveId = informationData?.sID;
                //    getPrReviewer.SRwApproveName = informationData?.name;
                //    getPrReviewer.NRwStatus = approveStatus;
                //    getPrReviewer.DRwApproveDate = DateTime.Now;
                //    getPrReviewer.SRwRemark = remark;

                //    response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                //    if (response)
                //    {

                //        await NextStepToRegisterDate(getPRRequest);
                //    }

                //    _logger.LogInformation($"PR : {prNo} Status Item {informationData.title} = {getPrReviewer.NRwStatus}{Environment.NewLine}");
                //    return response;

                //}

                if (getPRRequest.NStatus == 6)
                {
                    var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPrNo == getPRRequest.SPrNo && x.NRwSteps == 6 && x.NRwStatus == 0).FirstOrDefaultAsync();

                    if (getPrReviewer == null)
                    {
                        return false;
                    }

                    getPRRequest.NStatus = 7;
                    getPRRequest.DDeliveryDate = DateTime.Parse(remark);
                    getPrReviewer.SRwApproveId = informationData?.sID;
                    getPrReviewer.SRwApproveName = informationData?.name;
                    getPrReviewer.NRwStatus = approveStatus;
                    getPrReviewer.DRwApproveDate = DateTime.Now;
                    //getPrReviewer.SRwRemark = remark;

                    response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                    if (response)
                    {
                        await _mailService.sendEmail(prNo, 7, 4, null);
                    }


                    _logger.LogInformation($"PR : {prNo} Status Item = {getPrReviewer.NRwStatus}{Environment.NewLine}");
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

            var getPRRequest = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPrNo == prNo).FirstOrDefaultAsync();

            if (getPRRequest != null)
            {
                //Update Status at PR Request is 4
                getPRRequest.NStatus = 4;

                
                var updatePRReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.NRwSteps == 4 && x.SPrNo == prNo && x.NRwStatus != 9).FirstOrDefaultAsync();

                if (updatePRReviewer != null)
                {
                    updatePRReviewer.DRwApproveDate = null;
                    updatePRReviewer.NRwStatus = 0;
                    updatePRReviewer.SRwRemark = null;
                }

                //Delete Status more than 4
                var DeletePrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.NRwSteps > 4 && x.SPrNo == prNo && x.NRwStatus != 9).ToListAsync();

                if (DeletePrReviewer.Count > 0)
                {
                    _eSignPrpoContext.TbPrReviewers.RemoveRange(DeletePrReviewer);
                }
            
                                
                var insertReprocess = new TbPrReviewer
                {
                    URwId = Guid.NewGuid(),
                    SRwApproveId = informationData?.sID,
                    SRwApproveName = informationData?.name,
                    SRwApproveTitle= informationData?.title,
                    SRwApproveDepartment = informationData?.department,
                    DRwApproveDate = DateTime.Now,
                    NRwSteps = 99,
                    NRwStatus = 1,
                    DCreated = DateTime.Now,
                    SPrNo = prNo,
                    SRwRemark = remark
                };

                _eSignPrpoContext.TbPrReviewers.Add(insertReprocess);

                response = _eSignPrpoContext.SaveChanges() > 0;

                _logger.LogInformation($"PR : {prNo} is Reprocess by  [{informationData?.sID}] {informationData?.name} {Environment.NewLine}");
            }

            return response;

        }


        public void RejectPR(string prNo)
        {
            var listReviewer = _eSignPrpoContext.TbPrReviewers.Where(x => x.SPrNo == prNo).ToList();
            foreach (var itemReviewer in listReviewer)
            {
                itemReviewer.NRwStatus = 9;
            }


        }
        public async Task<bool> NextStepToFinance(TbPrReviewer _reviewer)
        {

            var resp = false;
            var getFinance = await _eSignPrpoContext.TbEmployees.Where(x => x.NPositionLevel == 2).FirstOrDefaultAsync();

            if (getFinance == null)
            {
                _logger.LogError("someting wrong when getFinance !");
                return resp;
            }

            var reviewer = new TbPrReviewer
            {
                URwId = Guid.NewGuid(),
                SRwApproveDepartment = getFinance?.SDepartment,
                SRwApproveTitle = getFinance?.SEmpTitle,
                NRwSteps = 2,
                NRwStatus = 0,
                SPrNo = _reviewer.SPrNo,
                DCreated = DateTime.Now,

            };

            _eSignPrpoContext.TbPrReviewers.Add(reviewer);

            resp = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return resp;
        }

        public async Task<bool> NextStepToGM(TbPrReviewer _reviewer)
        {

            var resp = false;
            var getGM = await _eSignPrpoContext.TbEmployees.Where(x => x.NPositionLevel == 3).FirstOrDefaultAsync();

            if (getGM == null)
            {
                _logger.LogError("someting wrong when getGM !");
                return resp;
            }

            var reviewer = new TbPrReviewer
            {
                URwId = Guid.NewGuid(),
                SRwApproveDepartment = getGM?.SDepartment,
                SRwApproveTitle = getGM?.SEmpTitle,
                NRwSteps = 3,
                NRwStatus = 0,
                SPrNo = _reviewer.SPrNo,
                DCreated = DateTime.Now,

            };

            _eSignPrpoContext.TbPrReviewers.Add(reviewer);

            resp = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return resp;
        }

        public async Task<bool> NextStepToPurchOff(TbPrReviewer _reviewer)
        {

            var resp = false;
            var getPurchOff = await _eSignPrpoContext.TbEmployees.Where(x => x.NPositionLevel == 4).FirstOrDefaultAsync();

            if (getPurchOff == null)
            {
                _logger.LogError("someting wrong when GetPurchase Officer !");
                return resp;
            }

            var reviewer = new TbPrReviewer
            {
                URwId = Guid.NewGuid(),
                SRwApproveDepartment = getPurchOff?.SDepartment,
                SRwApproveTitle = getPurchOff?.SEmpTitle,
                NRwSteps = 4,
                NRwStatus = 0,
                SPrNo = _reviewer.SPrNo,
                DCreated = DateTime.Now,

            };

            _eSignPrpoContext.TbPrReviewers.Add(reviewer);

            resp = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return resp;
        }

        public async Task<bool> NextStepToPurchMgr(TbPrReviewer _reviewer)
        {

            var resp = false;
            var getPurchMgr = await _eSignPrpoContext.TbEmployees.Where(x => x.NPositionLevel == 5).FirstOrDefaultAsync();

            if (getPurchMgr == null)
            {
                _logger.LogError("someting wrong when GetPurchase Mgr !");
                return resp;
            }

            var reviewer = new TbPrReviewer
            {
                URwId = Guid.NewGuid(),
                SRwApproveDepartment = getPurchMgr?.SDepartment,
                SRwApproveTitle = getPurchMgr?.SEmpTitle,
                NRwSteps = 5,
                NRwStatus = 0,
                SPrNo = _reviewer.SPrNo,
                DCreated = DateTime.Now,

            };

            _eSignPrpoContext.TbPrReviewers.Add(reviewer);

            resp = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return resp;
        }

        public async Task<bool> NextStepToSupplierAck(TbPrRequest _request)
        {
            var resp = false;
            var reviewer = new TbPrReviewer
            {
                URwId = Guid.NewGuid(),
                SRwApproveId = _request.SSupplierCode,
                SRwApproveName = _request.SSupplierName,
                SRwApproveDepartment = string.Empty,
                SRwApproveTitle = "Supplier Acknowledge",
                NRwSteps = 6,
                NRwStatus = 0,
                SPrNo = _request.SPrNo,
                DCreated = DateTime.Now,

            };

            _eSignPrpoContext.TbPrReviewers.Add(reviewer);

            resp = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return resp;
        }

        public async Task<bool> NextStepToRegisterDate(TbPrRequest _request)
        {
            var resp = false;
            var reviewer = new TbPrReviewer
            {
                URwId = Guid.NewGuid(),
                SRwApproveId = _request.SSupplierCode,
                SRwApproveName = _request.SSupplierName,
                SRwApproveDepartment = string.Empty,
                SRwApproveTitle = "Supplier Register Delivery Date",
                NRwSteps = 6,
                NRwStatus = 0,
                SPrNo = _request.SPrNo,
                DCreated = DateTime.Now,

            };

            _eSignPrpoContext.TbPrReviewers.Add(reviewer);

            resp = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return resp;
        }

        public async Task<Tuple<bool, string>> convertPOFlow(informationData informationData, string remark, string prNo, int approveStatus)
        {
            var response = false;
            var getPRRequest = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPrNo == prNo).FirstOrDefaultAsync();

            if (getPRRequest == null)
            {
                return Tuple.Create(false, string.Empty);
            }
            if (getPRRequest.NStatus == 4)
            {
                var getPrReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPrNo == getPRRequest.SPrNo && x.NRwSteps == 4 && x.NRwStatus == 0).FirstOrDefaultAsync();

                if (getPrReviewer == null)
                {
                    return Tuple.Create(false, string.Empty);
                }

                var poNo = await generatePONo(getPRRequest?.SCategory, prNo);

                getPRRequest.SPoNo = poNo;
                getPRRequest.NStatus = 5;
                getPRRequest.DConvertToPo = DateTime.Now;

                if (approveStatus == 9)
                {
                    RejectPR(getPRRequest.SPrNo);
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
                        await NextStepToPurchMgr(getPrReviewer);
                        //var genFile = await generateFile(getPRRequest?.SPrNo);
                        await _mailService.sendEmail(prNo, 5, 2, null);
                    }
                    else
                    {
                        await _mailService.sendRejectEmail(prNo);
                    }
                }

                _logger.LogInformation($"PR : {prNo} => PO : {poNo} Status Item {informationData.title} = {getPrReviewer.NRwStatus}{Environment.NewLine}");
                return Tuple.Create(true, poNo); ;

            }

            return Tuple.Create(false, string.Empty);
        }


        public async Task<string> generatePONo(string cateType, string prNo)
        {
            //Check PO before because reprocess step
            var checkPoPr = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPrNo == prNo && x.SPoNo != null).FirstOrDefaultAsync();
            if (checkPoPr != null)
            {
                return checkPoPr?.SPoNo;
            }

            string prefix = string.Empty;
            List<string> inventoryList = new List<string> { "Inventory Spare part", "Inventory RM (Domestic : DDP / DAP)", "Inventory RM (Oversea : EXW / CIF)", "Inventory RM (P&A)" };
            if (cateType == "Capex")
            {
                prefix = "CX";
            }
            else if (inventoryList.Contains(cateType))
            {
                prefix = "PO";
            }
            else
            {
                prefix = "PC";
            }

            var poNo = $"{prefix}{DateTime.Now.ToString("yy")}00001";
            var getPrDesc = await _eSignPrpoContext.TbPrRequests.Where(x => x.SCategory == cateType && x.SPoNo != null).OrderByDescending(x => x.DCreated).FirstOrDefaultAsync();

            if (getPrDesc != null)
            {
                //if (getPrDesc?.SPoNo?.Substring(4, 2) == DateTime.Now.ToString("MM"))
                //{
                //    int _numgetPrDesc = Convert.ToInt16(getPrDesc?.SPoNo?.Substring(6, 5)) + 1;
                //    poNo = $"{prefix}{DateTime.Now.ToString("yyMM")}{_numgetPrDesc.ToString("00000")}";
                //    return poNo;
                //}

                if (getPrDesc?.SPoNo?.Substring(2, 2) == DateTime.Now.ToString("yy"))
                {
                    int _numgetPrDesc = Convert.ToInt16(getPrDesc?.SPoNo?.Substring(4, 5)) + 1;
                    poNo = $"{prefix}{DateTime.Now.ToString("yy")}{_numgetPrDesc.ToString("00000")}";
                    return poNo;
                }
            }


            return poNo;
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
            dt1.Columns.Add("capex");
            dt1.Columns.Add("supplierCode");
            dt1.Columns.Add("currency");
            dt1.Columns.Add("shipVia");
            dt1.Columns.Add("termCondition");
            dt1.Columns.Add("paymentCondition");
            dt1.Columns.Add("subAmount");
            dt1.Columns.Add("vatAmount");
            dt1.Columns.Add("totalAmount");
            dt1.Columns.Add("remarks");
            dt1.Columns.Add("supplierName");
            dt1.Columns.Add("supplieAddress");
            dt1.Columns.Add("billToName");
            dt1.Columns.Add("billToAddress");
            dt1.Rows.Add(
                res?.poNo,
                $"Date : {res.createdDate?.ToString("dd.MMM.yyyy")}",
                res?.capexNo,
                res?.supplierCode,
                res?.currency,
                res?.shipVia,
                res?.termCondition,
                res?.paymentCondition,
                res?.totalAmountTHB,
                res?.vatTotal,
                res?.totalAmountVatTHB,
                $"{res?.reason}\nRequest by : {res?.createdBy}\n{res?.flowPRs.FirstOrDefault(x => x.nRW_Steps == 4).sRw_Remark}",
                res?.supplierInfo?.name,
                $"{res?.supplierInfo?.address1}\n" +
                $"{res?.supplierInfo?.address2}\n" +
                $"{res?.supplierInfo?.address3}\n"+
                $"{res?.supplierInfo?.address4}\n\n\n\n"+
                $"Tel.No.:{res?.supplierInfo?.tel}\t\tFax:{res?.supplierInfo?.fax}\n" +
                $"Contact:{res?.supplierInfo?.contact}\n" +
                $"E-Mail:{res?.supplierInfo?.email}",
                res?.shipToInfo?.name,
                res?.shipToInfo?.address1
                );

            DataTable dt2 = new DataTable("POItem");
            dt2.Columns.Add("no");
            dt2.Columns.Add("itemCode");
            dt2.Columns.Add("description");
            dt2.Columns.Add("quantity");
            dt2.Columns.Add("uom");
            dt2.Columns.Add("unitPrice");
            dt2.Columns.Add("amount");
            dt2.Columns.Add("deliveryDate");

            var i = 1;
            foreach (var itemPo in res.listPRPOItems)
            {
                dt2.Rows.Add(
                    $"{i}",
                    itemPo?.item,
                    itemPo?.itemDesc,
                    itemPo?.qty,
                    itemPo?.Uom,
                     itemPo?.unitCost,
                    itemPo?.amount,
                    itemPo?.requestDate
                    );

                i++;
            }


            localReport.AddDataSource("DataSet1", dt1);
            localReport.AddDataSource("DataSet2", dt2);


            var getPrReview = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPrNo == prNo).ToListAsync();

            // var signBuyer = "";
            //var checkOfficer = getPrReview.Where(x => x.NRwSteps == 4 && x.NRwStatus == 3).FirstOrDefault();
            //if (checkOfficer != null)
            //{
            //    signBuyer = convertToBase(checkOfficer.SRwApproveId);
            //}


            var signPM = "";
            var checkManager = getPrReview.Where(x => x.NRwSteps == 5 && x.NRwStatus == 1).FirstOrDefault();
            if (checkManager != null)
            {
                signPM = convertToBase(checkManager.SRwApproveId);
            }


            var param = new Dictionary<string, string>
            {
                { "PMImage" , signPM}
            };

            var result = localReport.Execute(RenderType.Pdf, extension, param, mimTypes);

            return result.MainStream;
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
                var getPRByNo = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPrNo == prNo).FirstOrDefaultAsync();

                var getPRItemByNo = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.SPrNo == prNo).ToListAsync();

                var getFiles = await _eSignPrpoContext.TbAttachments.Where(x => x.UPrId == getPRByNo.UPrId).ToListAsync();

                var getFlowPR = await _eSignPrpoContext.TbPrReviewers.OrderBy(x => x.NRwStatus).Where(x => x.SPrNo == prNo && x.NRwStatus != 9).ToListAsync(); ;

                var getRejectFlow = await _eSignPrpoContext.TbPrReviewers.OrderByDescending(x => x.DCreated).Where(x => x.SPrNo == prNo && x.NRwStatus == 9 && x.BIsReject == true).ToListAsync();

                var getShipVia = await _eSignPrpoContext.TbShipVia.Where(x => x.TSfbp == getPRByNo.SSupplierCode).FirstOrDefaultAsync();
                var informationData = _accountService.informationUser();

                var checkPermision = getFlowPR.Where(x => (x.SRwApproveId == informationData.sID || x.SRwApproveTitle == informationData.title) && x.NRwStatus == 0).Count();

                var getTermCon = await _eSignPrpoContext.TbTermAndConditions.Where(x => x.TOtbp == getPRByNo.SSupplierCode).FirstOrDefaultAsync();

                var getPaymentCon = await _eSignPrpoContext.TbPaymentConditions.Where(x => x.TIfbp == getPRByNo.SSupplierCode).FirstOrDefaultAsync();

                var getVat = getPRByNo?.BIsVat == true ? getPRByNo?.FSumAmtThb * 0.07 : 0;

                var getSupAdd = await _eSignPrpoContext.TbSupplierAddresses.Where(x => x.TBpid == getPRByNo.SSupplierCode).FirstOrDefaultAsync();

                var getShipTo = await _eSignPrpoContext.TbCompanies.Where(x => x.CompanyCode == getPRByNo.SProduct).FirstOrDefaultAsync();


                response = new ApproverPRDetailResponse();

                response.checkPermission = checkPermision > 0;
                response.prNo = getPRByNo?.SPrNo;
                response.poNo = getPRByNo?.SPoNo;
                response.createdDate = getPRByNo?.DCreated;
                response.capexNo = getPRByNo?.SCapexNo;
                response.categoryType = getPRByNo?.SCategory;
                response.totalAmount = getPRByNo?.FSumAmtCurrency?.ToString("N");
                response.totalAmountTHB = getPRByNo?.FSumAmtThb?.ToString("N");
                response.vatTotal = getVat?.ToString("N");
                response.totalAmountVatTHB = (getPRByNo?.FSumAmtThb + getVat)?.ToString("N");
                response.productsType = getPRByNo?.SProduct;
                response.wh = getPRByNo?.SWh;
                response.supplierCode = getPRByNo?.SSupplierCode;
                response.supplierName = $"{getPRByNo?.SSupplierCode} | {getPRByNo?.SSupplierName}";
                response.refAssetNo = getPRByNo?.SAssetNo;
                response.assetName = getPRByNo?.SAssentName;
                response.assetType = getPRByNo?.STypeAsset;
                response.reason = getPRByNo?.SReason;
                response.status = getPRByNo.NStatus;
                response.shipVia = getShipVia?.TDsca;
                response.termCondition = getTermCon?.TDsca;
                response.paymentCondition = getPaymentCon?.TDsca;
                response.createdBy = getPRByNo?.SCreatedName;
                response.deliveryDate = getPRByNo?.DDeliveryDate?.ToString("dd/MM/yyyy");
                response.currency = getPRByNo?.SCurrency;
                response.listPRPOItems = getPRItemByNo.Select(x => new listPRPOItem
                {
                    uPrItemId = x?.UPrItemId,
                    no = x?.NNo?.ToString(),
                    item = x?.SItem,
                    itemDesc = x?.SItemDesc,
                    Uom = x?.SUom,
                    qty = x?.NQty.ToString(),
                    amount = x?.FAmount?.ToString("N"),
                    costCenter = x?.SCostCenter,
                    currency = x?.SCurrency,
                    glCode = x?.SGlCode,
                    unitCost = x?.FUnitCost?.ToString("N"),
                    requestDate = x?.DRequestDate?.ToString("dd.MMM.yyyy")

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
                response.supplierInfo = new address
                {
                    name = getSupAdd?.TNama,
                    address1 = getSupAdd?.TLn03,
                    address2 = getSupAdd?.TLn04,
                    address3 = getSupAdd?.TLn05,
                    address4 = getSupAdd?.TLn06,
                    tel = getSupAdd?.TTelp,
                    fax = getSupAdd?.TTelx,
                    contact = getSupAdd?.TInet,
                    email = getSupAdd?.TInfo
                };
                response.shipToInfo = new address
                {
                    name = getShipTo?.Name,
                    address1 = getShipTo?.Address
                };



                return response;
            }
            catch (Exception ex)
            {
                return response;
            }

        }

        public async Task<bool> isVat(string prNo, string isChecked)
        {
            var getPrRequest = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPrNo == prNo).FirstOrDefaultAsync();

            getPrRequest.BIsVat = isChecked == "1" ? true : false;

            var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

            return response;
        }
    }
}
