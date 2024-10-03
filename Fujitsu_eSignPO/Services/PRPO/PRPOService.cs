using DocumentFormat.OpenXml.Drawing;
using Fujitsu_eSignPO.Data;
using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.Account;
using Fujitsu_eSignPO.Models.PRPO;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Data;
using System.Globalization;

namespace Fujitsu_eSignPO.Services.PRPO
{
    public class PRPOService : IPRPOService
    {
        private static FgdtESignPoContext _eSignPrpoContext;
        private readonly IAccountService _accountService;
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<PRPOService> _logger;
        private readonly IMailService _mailService;

        public PRPOService(FgdtESignPoContext eSignPrpoContext, IAccountService accountService, IWorkflowService workflowService, ILogger<PRPOService> logger, IMailService mailService)
        {
            _eSignPrpoContext = eSignPrpoContext;
            _accountService = accountService;
            _workflowService = workflowService;
            _logger = logger;
            _mailService = mailService;

        }

        #region About getData on PR Requests

        public async Task<TbPrRequest> getPrRequestByNo(Guid poGuid) => await _eSignPrpoContext.TbPrRequests.Where(x => x.UPoId == poGuid).FirstOrDefaultAsync();
        public async Task<List<TbPrRequestItem>> getPrRequestItemByNo(string poNo) => await _eSignPrpoContext.TbPrRequestItems.Where(x => x.SPoNo == poNo).ToListAsync();
        public async Task<List<TbVendor>> getVendorData() => await _eSignPrpoContext.TbVendors.ToListAsync();
        public async Task<List<TbDepartment>> getDepData() => await _eSignPrpoContext.TbDepartments.Where(x=>x.PreCode != "").ToListAsync();

        public async Task<List<TbCurrency>> getCurrData() => await _eSignPrpoContext.TbCurrencies.ToListAsync();

        public async Task<List<string>> getMainCode() => await _eSignPrpoContext.TbAccountCodes.Select(x => x.MainCode).Distinct().ToListAsync();
        public async Task<List<string>> getSubCode1(string mainCode) => await _eSignPrpoContext.TbAccountCodes.Where(x => x.MainCode == mainCode).Select(x => x.SubCode1).Distinct().ToListAsync();
        public async Task<List<string>> getSubCode2(string subCode1) => await _eSignPrpoContext.TbAccountCodes.Where(x => x.SubCode1 == subCode1).Select(x => x.SubCode2).Distinct().ToListAsync();

        public async Task<TbAccountCode> getBudgetBalance(string mainCode, string subCode1, string subCode2) => await _eSignPrpoContext.TbAccountCodes.Where(x => x.MainCode == mainCode && x.SubCode1 == subCode1 && x.SubCode2 == subCode2).FirstOrDefaultAsync();
        //public async Task<double?> getRateByCurrency(string curr) => await _eSignPrpoContext.TbCurrencies.OrderByDescending(x => x.CurrencyName).Select(x => x.CurrencyName).FirstOrDefaultAsync();


        public async Task<List<TbAttachment>> getAttachmentsData(Guid guid) => await _eSignPrpoContext.TbAttachments.Where(x => x.UPrId == guid).ToListAsync();

        public async Task<List<PrRecordsResponse>> getPrRecords()
        {
            var response = new List<PrRecordsResponse>();
            var informationData = _accountService.informationUser();



            if (informationData.positionLevel == "0")
            {
                var getPrRequests = await _eSignPrpoContext.TbPrRequests.Where(x => x.SCreatedBy == informationData.sID).ToListAsync();

                response = getPrRequests.Select(x => new PrRecordsResponse()
                {
                    SPoNo = x.SPoNo,
                    SSupplierName = $"{getVendorName(x.SVendorCode)}",
                    FSumAmtCurrency = x.FSumAmtCurrency,
                    FSumAmtThb = x.FSumAmtThb,
                    SStatus = getFlowName(x.NStatus),
                    NStatus = x?.NStatus,
                    DCreated = x.DCreated,
                    SCreatedBy = x.SCreatedBy,
                    UPoID = x.UPoId
                }).ToList();

                return response;
            }

            var checkPostionLevel_1 = informationData.positionLevel == "1";
            var checkPostionLevel_2 = informationData.positionLevel == "2";

            if (checkPostionLevel_1)
            {
                var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => x.SRwApproveId == informationData.sID && x.NRwStatus == 0 && x.NRwSteps == 1).ToListAsync();

                response = getPrRequests.Select(x => new PrRecordsResponse()
                {
                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.VendorName}",
                    FSumAmtCurrency = x.FSumAmtCurrency,
                    FSumAmtThb = x.FSumAmtThb,
                    SStatus = getFlowName(x.NStatus),
                    DCreated = x.DCreated,
                    NStatus = x?.NStatus
                }).ToList();

                return response;
            }


            if (checkPostionLevel_2)
            {
                int step = (int.Parse(informationData.positionLevel));
                var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => x.SRwApproveTitle == informationData.title && x.NRwStatus == 0 && x.NRwSteps == step).ToListAsync();

                response = getPrRequests.Select(x => new PrRecordsResponse()
                {
                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.VendorName}",
                    FSumAmtCurrency = x.FSumAmtCurrency,
                    FSumAmtThb = x.FSumAmtThb,
                    SStatus = getFlowName(x.NStatus),
                    DCreated = x.DCreated,
                    NStatus = x?.NStatus
                }).ToList();

                return response;
            }

            //if (informationData.title == "Purchasing Officer")
            //{
            //    var getPrRequests = await _eSignPrpoContext.TbPrRequests.Where(x => x.SCreatedBy == informationData.sID).ToListAsync();


            //    response = getPrRequests.Select(x => new PrRecordsResponse()
            //    {

            //        SPoNo = x.SPoNo,
            //        SSupplierName = $"{x.SVendorCode} | {x.SVendorName}",
            //        FSumAmtCurrency = x.FSumAmtCurrency,
            //        FSumAmtThb = x.FSumAmtThb,
            //        SStatus = getFlowName(x.NStatus),
            //        DCreated = x.DCreated,
            //        SCreatedBy = x.SCreatedBy,
            //        NStatus = x?.NStatus,
            //        UPoID = x.UPoId
            //    }).ToList();

            //    return response;

            //}

            ////Step5:
            //if (informationData.title == "Purchasing Manager")
            //{
            //    var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => (x.SRwApproveTitle == informationData.title && x.NRwStatus == 0 && x.NRwSteps == 5) || (x.SRwApproveId == informationData.sID && x.NRwStatus == 0 && x.NRwSteps == 1)).ToListAsync();

            //    response = getPrRequests.Select(x => new PrRecordsResponse()
            //    {

            //        SPoNo = x.SPoNo,
            //        SSupplierName = $"{x.SSupplierCode} | {x.SSupplierName}",
            //        FSumAmtCurrency = x.FSumAmtCurrency,
            //        FSumAmtThb = x.FSumAmtThb,
            //        SStatus = getFlowName(x.NStatus),
            //        DCreated = x.DCreated,
            //        NStatus = x?.NStatus
            //    }).ToList();

            //    return response;
            //}



            return response;
        }

        public string getVendorName(string vendorCode)
        {
            return _eSignPrpoContext.TbVendors.Where(x => x.VendorCode == vendorCode).Select(x => x.VendorName).FirstOrDefault();
        }

        public async Task<List<PrRecordsResponse>> getPoRecords()
        {
            var response = new List<PrRecordsResponse>();
            var informationData = _accountService.informationUser();

            if (informationData.positionLevel == "99")
            {
                var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => (x.SRwApproveId == informationData.sID && x.NRwStatus == 0 && x.NRwSteps == 3)).ToListAsync();

                response = getPrRequests.Select(x => new PrRecordsResponse()
                {

                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.VendorName}",
                    FSumAmtCurrency = x.FSumAmtCurrency,
                    FSumAmtThb = x.FSumAmtThb,
                    SStatus = getFlowName(x.NStatus),
                    DCreated = x.DCreated,
                    NStatus = x?.NStatus
                }).ToList();

                return response;
            }



            return response;
        }

        public async Task<List<PrRecordsResponse>> getPOHistory(string dateStart, string dateEnd)
        {

            List<DateTime> ListDate = new List<DateTime>();



            var response = new List<PrRecordsResponse>();
            var informationData = _accountService.informationUser();



            if (dateStart != null && dateEnd != null)
            {
                DateTime dateSt = DateTime.ParseExact(dateStart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime dateN = DateTime.ParseExact(dateEnd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                TimeSpan tsStart = new TimeSpan(0, 0, 0);
                TimeSpan tsEnd = new TimeSpan(23, 59, 0);

                var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => (x.SRwApproveId == informationData.sID && x.NRwStatus == 1) &&
                (x.DCreated >= dateSt + tsStart && x.DCreated <= dateN + tsEnd))
                    .Select(x => new { x.SPoNo, x.VendorName, x.FSumAmtCurrency, x.FSumAmtThb, x.NStatus, x.DCreated })
                    .Distinct()
                    .ToListAsync();

                response = getPrRequests.Select(x => new PrRecordsResponse
                {

                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.VendorName}",
                    FSumAmtCurrency = x.FSumAmtCurrency,
                    FSumAmtThb = x.FSumAmtThb,
                    SStatus = getFlowName(x.NStatus),
                    DCreated = x.DCreated
                }).ToList();
            }
            else
            {
                var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => (x.SRwApproveId == informationData.sID && x.NRwStatus == 1)).Select(x => new { x.SPoNo, x.VendorName, x.FSumAmtCurrency, x.FSumAmtThb, x.NStatus, x.DCreated }).Distinct().ToListAsync();

                response = getPrRequests.Select(x => new PrRecordsResponse
                {

                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.VendorName}",
                    FSumAmtCurrency = x.FSumAmtCurrency,
                    FSumAmtThb = x.FSumAmtThb,
                    SStatus = getFlowName(x.NStatus),
                    DCreated = x.DCreated
                }).ToList();
            }



            return response;


        }

        public string getFlowName(int? nStatus)
        {
            var workflowStatus = _workflowService.getStatusFlow();
            var response = workflowStatus.Where(a => a.NFlowId == nStatus.ToString()).FirstOrDefault().SFlowName;
            return response;
        }

        #endregion

        public async Task<bool> InsertAttachment(List<IFormFile> files, Guid guid)
        {
            var attacments = new List<TbAttachment>();

            foreach (var itemFile in files)
            {
                var fileSize = fileSizeMB(itemFile.Length);

                var attID = Guid.NewGuid();
                var attacment = new TbAttachment
                {
                    UAttachId = attID,
                    SAttachName = $"{itemFile.FileName}",
                    SAttachFileType = itemFile.ContentType,
                    FAttachFileSize = fileSize,
                    DCreated = DateTime.Now,
                    UPrId = guid
                };

                attacments.Add(attacment);
            }
            _eSignPrpoContext.TbAttachments.AddRange(attacments);
            var response = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return response;
        }

        public double fileSizeMB(long fileLength)
        {
            long fileSizeInBytes = fileLength;
            double fileSizeInKB = fileSizeInBytes / 1024.0;
            double fileSizeInMB = fileSizeInKB / 1024.0;

            return fileSizeInMB;
        }

        public async Task<bool> DeleteFile(string fileName, Guid guid)
        {
            var deleteFile = await _eSignPrpoContext.TbAttachments.Where(x => x.SAttachName == fileName && x.UPrId == guid).FirstOrDefaultAsync();

            _eSignPrpoContext.TbAttachments.Remove(deleteFile);
            var response = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return response;

        }

        public async Task<Tuple<bool, string>> InsertPR(PRPOViewModel prRequest, List<listPRPOItem> listPRPOItem, Guid guid)
        {
            try
            {
                var informationData = _accountService.informationUser();
                var getVendorName = await _eSignPrpoContext.TbVendors.Where(x => x.VendorCode == prRequest.vendorName).Select(x=>x.VendorName).FirstOrDefaultAsync();

                var addPR = new TbPrRequest
                {
                    UPoId = guid,
                    SPoNo = await generatePONo(prRequest?.department),
                    SVendorCode = prRequest?.vendorName,
                    SVendorName = getVendorName,
                    SDepartment = prRequest?.department,
                    SRefQuotation = prRequest?.refQuatation,
                    SVatType = prRequest?.vatOption,
                    SCurrency = prRequest?.currency,
                    FRate = prRequest?.rate,
                    DShippingDate = prRequest?.shippingDate == DateTime.MinValue ? null : prRequest?.shippingDate,
                    DPoDate = prRequest?.poDate,
                    SMainCode = prRequest?.mainCode,
                    SSubCode1 = prRequest?.subCode1,
                    SSubCode2 = prRequest?.subCode2,
                    //  FBudget = prRequest?.budget,
                    // FBalance = prRequest?.balance,
                    //SSupplierCode = prRequest?.supplierName.Split("|")[0],
                    //SSupplierName = prRequest?.supplierName.Split("|")[1],
                    NStatus = 1,
                    FSumAmtCurrency = double.Parse(prRequest?.totalAmount.Replace(",", "")),
                    FSumAmtThb = double.Parse(prRequest?.totalAmountTHB.Replace(",", "")),
                    SReason = prRequest?.reason,
                    SCreatedBy = informationData?.sID,
                    SCreatedName = informationData?.name,
                    DCreated = DateTime.Now,

                };

                _eSignPrpoContext.TbPrRequests.Add(addPR);

                var addPRListItem = listPRPOItem.Select(x => new TbPrRequestItem
                {
                    UPrItemId = Guid.NewGuid(),
                    NNo = int.Parse(x?.no),
                    SPartNo = x?.partNo,
                    SPartName = x?.partName,
                    SVatType = x?.vatType,
                    FUnitPrice = double.Parse(x?.unitPrice.Replace(",", "")),
                    NQty = int.Parse(x?.qty),
                    FAmount = double.Parse(x?.amount.Replace(",", "")),
                    NStatus = 1,
                    DCreated = DateTime.Now,
                    SPoNo = addPR.SPoNo


                }).ToList();

                _eSignPrpoContext.TbPrRequestItems.AddRange(addPRListItem);

                var getBalance = await getBudgetBalance(prRequest.mainCode, prRequest.subCode1, prRequest.subCode2);

                if (getBalance != null)
                {
                    getBalance.Balance = getBalance.Balance - double.Parse(prRequest?.totalAmountTHB.Replace(",", ""));
                }


                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                if (response)
                {
                    _logger.LogInformation($"Create PR : {addPR.SPoNo} is success , by user : [{addPR.SCreatedBy}] {addPR.SCreatedName} ");

                    await _workflowService.generateWorkflow(addPR?.SDepartment, addPR?.SPoNo);

                    //var nextPosition = informationData.positionLevel == "4" ? 5 : 1;

                    //await _mailService.sendEmail(addPR?.sPoNo, 1, 1, null);
                }

                return Tuple.Create(response, $"Create PO No. : {addPR?.SPoNo} is success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Tuple.Create(false, ex.Message); ;
            }
        }

        public async Task<Tuple<bool, string>> UpdatePR(PRPOViewModel prRequest, List<listPRPOItem> listPRPOItem, Guid guid, string isReSubmit)
        {
            try
            {
                var informationData = _accountService.informationUser();

                var responsePR = await getPrRequestByNo(guid);


                var refundBalance = await getBudgetBalance(responsePR.SMainCode, responsePR.SSubCode1, responsePR.SSubCode2);

                refundBalance.Balance = refundBalance.Balance + responsePR.FSumAmtThb;

                await _eSignPrpoContext.SaveChangesAsync();

                var getVendorName = await _eSignPrpoContext.TbVendors.Where(x => x.VendorCode == prRequest.vendorName).Select(x => x.VendorName).FirstOrDefaultAsync();


                responsePR.SVendorCode = prRequest?.vendorName;
                responsePR.SVendorName = getVendorName;
                responsePR.SDepartment = prRequest?.department;
                responsePR.SRefQuotation = prRequest?.refQuatation;
                responsePR.SVatType = prRequest?.vatOption;
                responsePR.SCurrency = prRequest?.currency;
                responsePR.FRate = prRequest?.rate;
                responsePR.DShippingDate = prRequest?.shippingDate == DateTime.MinValue ? null : prRequest?.shippingDate;
                responsePR.DPoDate = prRequest?.poDate;
                responsePR.SMainCode = prRequest?.mainCode;
                responsePR.SSubCode1 = prRequest?.subCode1;
                responsePR.SSubCode2 = prRequest?.subCode2;
                // responsePR.FBudget = prRequest?.budget;
                // responsePR.FBalance = prRequest?.balance;
                responsePR.FSumAmtCurrency = double.Parse(prRequest?.totalAmount.Replace(",", ""));
                responsePR.FSumAmtThb = double.Parse(prRequest?.totalAmountTHB.Replace(",", ""));
                responsePR.SReason = prRequest?.reason;
                responsePR.DUpdated = DateTime.Now;


                if (isReSubmit == "1")
                {
                    responsePR.NStatus = 1;
                }


                var reponseListPR = await getPrRequestItemByNo(responsePR.SPoNo);

                _eSignPrpoContext.TbPrRequestItems.RemoveRange(reponseListPR);

                var addPRListItem = listPRPOItem.Select(x => new TbPrRequestItem
                {
                    UPrItemId = Guid.NewGuid(),
                    NNo = int.Parse(x?.no),
                    SPartNo = x?.partNo,
                    SPartName = x?.partName,
                    SVatType = x?.vatType,
                    FUnitPrice = double.Parse(x?.unitPrice.Replace(",", "")),
                    NQty = int.Parse(x?.qty),
                    FAmount = double.Parse(x?.amount.Replace(",", "")),
                    NStatus = 1,
                    DCreated = DateTime.Now,
                    SPoNo = responsePR.SPoNo

                }).ToList();



                _eSignPrpoContext.TbPrRequestItems.AddRange(addPRListItem);

                var getBalance = await getBudgetBalance(prRequest.mainCode, prRequest.subCode1, prRequest.subCode2);

                if (getBalance != null)
                {
                    getBalance.Balance = getBalance.Balance - double.Parse(prRequest?.totalAmountTHB.Replace(",", ""));
                }

                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                if (response)
                {
                    if (isReSubmit == "1")
                    {
                        await _workflowService.generateWorkflow(responsePR?.SDepartment, responsePR?.SPoNo);

                        await _mailService.sendEmail(responsePR?.SPoNo, 1, 1, null);

                        _logger.LogInformation($"Re-Submit PO : {responsePR.SPoNo} is success , by user : [{responsePR.SCreatedBy}] {responsePR.SCreatedName} ");
                    }
                    else
                    {
                        _logger.LogInformation($"Update PO : {responsePR.SPoNo} is success , by user : [{responsePR.SCreatedBy}] {responsePR.SCreatedName} ");
                    }

                }

                return Tuple.Create(response, $"{(isReSubmit == "1" ? "Re-Submit" : "Update")} PR No. : {responsePR.SPoNo} is success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Tuple.Create(false, ex.Message); ;
            }
        }

        public async Task<string> generatePONo(string dep)
        {

            var now = DateTime.Now;
            var aprilFirst = new DateTime(now.Year, 4, 1);
            var now_year = DateTime.Now.ToString("yy");

            var poNo = $"FGDT-{dep}-{now_year}0001";
            var getPoDesc = new TbPrRequest();

            if (now < aprilFirst)
            {
                getPoDesc = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo.Contains($"FGDT-{dep}")).OrderByDescending(x => x.DCreated).FirstOrDefaultAsync();
            }
            else
            {
                getPoDesc = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo.Contains($"FGDT-{dep}-{now_year}")).OrderByDescending(x => x.DCreated).FirstOrDefaultAsync();
            }

            if (getPoDesc != null)
            {
                int poYear = now.Year;
                if (now < aprilFirst)
                {
                    poYear = now.Year - 1;
                    int _numgetPoDesc = Convert.ToInt16(getPoDesc.SPoNo.Substring(10, 4)) + 1;
                    return $"FGDT-{dep}-{poYear % 100:D2}{_numgetPoDesc:D4}";
                }

                if (now >= aprilFirst)
                {
                    int _numgetPoDesc = Convert.ToInt16(getPoDesc.SPoNo.Substring(10, 4)) + 1;
                    return $"FGDT-{dep}-{DateTime.Now.ToString("yy")}{_numgetPoDesc:D4}";
                }

            }

            return poNo;
        }


        public async Task<ApproverPRDetailResponse> getPRAllDetail(string prNo)

        {
            var response = new ApproverPRDetailResponse();

            var getPRByNo = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == prNo).FirstOrDefaultAsync();

            var getPRItemByNo = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.SPoNo == prNo).ToListAsync();

            var getFiles = await _eSignPrpoContext.TbAttachments.Where(x => x.UPrId == getPRByNo.UPoId).ToListAsync();

            var getFlowPR = await _eSignPrpoContext.TbPrReviewers.OrderBy(x => x.NRwStatus).Where(x => x.SPoNo == prNo && x.NRwStatus != 9 && x.NRwSteps != 99).ToListAsync(); ;

            var getRejectFlow = await _eSignPrpoContext.TbPrReviewers.OrderByDescending(x => x.DCreated).Where(x => x.SPoNo == prNo && x.NRwStatus == 9 && x.BIsReject == true).ToListAsync();


            var informationData = _accountService.informationUser();

            var checkPermision = getFlowPR.Where(x => (x.SRwApproveId == informationData.sID || x.SRwApproveTitle == informationData.title) && x.NRwStatus == 0).Count();
            var getVendorName = await _eSignPrpoContext.TbVendors.Where(x => x.VendorCode == getPRByNo.SVendorCode).FirstOrDefaultAsync();

            response = new ApproverPRDetailResponse()
            {
                checkPermission = checkPermision > 0,
                poNo = getPRByNo?.SPoNo,
                vendorName = getVendorName?.VendorName,
                department = getPRByNo?.SDepartment,
                refQuotation = getPRByNo?.SRefQuotation,
                vatType = vatType(getPRByNo?.SVatType),
                currency = getPRByNo?.SCurrency,
                rate = getPRByNo?.FRate.ToString(),
                shippingDate = getPRByNo?.DShippingDate == null ? "" : getPRByNo?.DShippingDate?.ToString("dd/MM/yyyy"),
                poDate = getPRByNo?.DPoDate?.ToString("dd/MM/yyyy"),
                mainCode = getPRByNo?.SMainCode,
                subCode1 = getPRByNo?.SSubCode1,
                subCode2 = getPRByNo?.SSubCode2,
                budget = getPRByNo?.FBudget?.ToString("N"),
                balance = getPRByNo?.FBalance?.ToString("N"),
                totalAmount = getPRByNo?.FSumAmtCurrency?.ToString("N"),
                totalAmountTHB = getPRByNo?.FSumAmtThb?.ToString("N"),

                reason = getPRByNo?.SReason,
                status = getPRByNo.NStatus,
                deliveryDate = getPRByNo?.DDeliveryDate?.ToString("dd/MM/yyyy"),
                listPRPOItems = getPRItemByNo.Select(x => new listPRPOItem
                {
                    no = x?.NNo?.ToString(),
                    uPoItemId = x?.UPrItemId,
                    partNo = x?.SPartNo,
                    partName = x?.SPartName,
                    vatType = vatType(x?.SVatType),
                    unitPrice = x?.FUnitPrice?.ToString("N"),
                    qty = x?.NQty.ToString(),
                    amount = x?.FAmount?.ToString("N"),
                    //amountNumber = x?.FAmount,

                }).ToList(),
                fileUploads = getFiles.Select(x => new fileUpload
                {
                    uPrId = x.UPrId,
                    isSendToSupplier = x?.BIsSendSupplier,
                    sAttach_Name = x.SAttachName,
                    sAttach_File_Size = x?.FAttachFileSize?.ToString("0.00"),
                    sAttach_File_Type = x.SAttachFileType,
                    sAttach_Id = x?.UAttachId

                }).ToList(),
                flowPRs = getFlowPR.Select(x => new flowPR
                {
                    sRw_Approve_ID = x.SRwApproveId,
                    nRW_Steps = x.NRwSteps,
                    sRw_Approve_Name = x.SRwApproveName,
                    sRW_Approve_Title = x.SRwApproveTitle,
                    dRW_Approve_Date = x.DRwApproveDate,
                    sRw_Remark = x.SRwRemark,
                    sRW_Status = x.NRwStatus.ToString()
                }).OrderBy(x => x.nRW_Steps).ToList(),
                flowReject = getRejectFlow.Select(x => new flowPR
                {
                    sRw_Approve_ID = x.SRwApproveId,
                    nRW_Steps = x.NRwSteps,
                    sRw_Approve_Name = x.SRwApproveName,
                    sRW_Approve_Title = x.SRwApproveTitle,
                    dRW_Approve_Date = x.DRwApproveDate,
                    sRw_Remark = x.SRwRemark,
                    sRW_Status = x.NRwStatus.ToString()
                }).OrderBy(x => x.nRW_Steps).ToList()
            };

            return response;
        }

        public string vatType(string vt)
        {
            var res = "";
            if (vt == "I")
            {
                res = "Include VAT";
            }
            else if (vt == "E")
            {
                res = "Exclude VAT";
            }
            else if (vt == "N")
            {
                res = "No VAT";
            }

            return res;
        }

        public async Task<bool> updatePRItem(Guid prItemId, string itemDesc, string qty, double? amount)
        {
            try
            {
                var getPrItem = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.UPrItemId == prItemId).FirstOrDefaultAsync();

                // getPrItem.SItemDesc = itemDesc;
                getPrItem.NQty = int.Parse(qty);
                // getPrItem.FUnitCost = amount;
                getPrItem.FAmount = int.Parse(qty) * amount;
                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                var listPritem = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.SPoNo == getPrItem.SPoNo).ToListAsync();

                var getCurr = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == getPrItem.SPoNo).FirstOrDefaultAsync();

                var sumAllAmount = listPritem.Sum(x => x.FAmount);

                getCurr.FSumAmtCurrency = sumAllAmount;
                getCurr.FSumAmtThb = getCurr?.FRate != null ? sumAllAmount * getCurr?.FRate : sumAllAmount;

                response = await _eSignPrpoContext.SaveChangesAsync() > 0;


                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }

        }

        public async Task<TbWhLocation> getWH(string category, string product) => await _eSignPrpoContext.TbWhLocations.Where(x => x.Location == product && x.Category == category).FirstOrDefaultAsync();


        public async Task<List<ExportAllPRModel>> getAllPrModel(DateTime dateStart, DateTime dateEnd)
        {
            var response = new List<ExportAllPRModel>();

            TimeSpan tsStart = new TimeSpan(0, 0, 0);
            TimeSpan tsEnd = new TimeSpan(23, 59, 0);
            var allPrRequest = await _eSignPrpoContext.TbPrRequests.Where(x => x.DCreated >= dateStart + tsStart && x.DCreated <= dateEnd + tsEnd).ToListAsync();

            if (allPrRequest.Count > 0)
            {
                foreach (var itemPR in allPrRequest)
                {
                    var res = new ExportAllPRModel();
                    //res.prNo = itemPR?.sPoNo;
                    //res.userCreatePR = itemPR?.SCreatedBy;
                    //res.prIssuedDate = itemPR?.DCreated?.ToString("dd/MM/yyyy");


                    //res.supplierCode = itemPR?.SSupplierCode;
                    //res.supplierName = itemPR?.SSupplierName;
                    //res.ReferenceA = itemPR?.SReason;
                    //res.capexNo = itemPR?.SCapexNo;
                    //res.assetName = itemPR?.SAssentName;
                    //res.refAsset = itemPR?.STypeAsset;
                    //res.location = itemPR?.SProduct;

                    string requisitionType = "";

                    string[] issuedStatus = new string[] { "0", "1", "2", "3" };
                    if (issuedStatus.Contains(itemPR?.NStatus?.ToString()))
                    {
                        requisitionType = "PR Issued";
                    }
                    else if (itemPR?.NStatus <= 4)
                    {
                        requisitionType = "Approved";
                    }
                    else if (itemPR?.NStatus >= 5)
                    {
                        requisitionType = "Converted";
                    }

                    res.requisitionType = requisitionType;

                    //if (requisitionType != "PR Issued")
                    //{
                    //    res.poNo = itemPR?.SPoNo;
                    //    var getPoCreated = await _eSignPrpoContext.TbPrReviewers.Where(x => x.sPoNo == itemPR.sPoNo && (x.NRwSteps == 4 && x.NRwStatus == 3)).FirstOrDefaultAsync();
                    //    res.userCreatePO = getPoCreated?.SRwApproveName;
                    //    res.poIssuedDate = itemPR?.DConvertToPo?.ToString("dd/MM/yyyy");
                    //}

                    string poStatus = "Inprocess";
                    if (itemPR.NStatus == 7)
                    {
                        poStatus = "Closed";
                    }

                    res.poStatus = poStatus;


                    res.expectDeliveryDate = itemPR?.DDeliveryDate?.ToString("dd/MM/yyyy");
                    //res.warehouse = itemPR?.SWh;

                    response.Add(res);
                }
            }

            return response;
        }
    }
}
