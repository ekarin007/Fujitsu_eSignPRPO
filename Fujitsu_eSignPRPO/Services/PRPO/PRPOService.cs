using eSignPRPO.Data;
using eSignPRPO.interfaces;
using eSignPRPO.Models;
using eSignPRPO.Models.Account;
using eSignPRPO.Models.PRPO;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace eSignPRPO.Services.PRPO
{
    public class PRPOService : IPRPOService
    {
        private static ESignPrpoContext _eSignPrpoContext;
        private readonly IAccountService _accountService;
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<PRPOService> _logger;
        private readonly IMailService _mailService;

        public PRPOService(ESignPrpoContext eSignPrpoContext, IAccountService accountService, IWorkflowService workflowService, ILogger<PRPOService> logger, IMailService mailService)
        {
            _eSignPrpoContext = eSignPrpoContext;
            _accountService = accountService;
            _workflowService = workflowService;
            _logger = logger;
            _mailService = mailService;

        }

        #region About getData on PR Requests

        public async Task<TbPrRequest> getPrRequestByNo(Guid prGuid) => await _eSignPrpoContext.TbPrRequests.Where(x => x.UPrId == prGuid).FirstOrDefaultAsync();
        public async Task<List<TbPrRequestItem>> getPrRequestItemByNo(string prNo) => await _eSignPrpoContext.TbPrRequestItems.Where(x => x.SPrNo == prNo).ToListAsync();
        public async Task<List<TbCusSup>> getSupplierData() => await _eSignPrpoContext.TbCusSups.ToListAsync();

        public async Task<CusSupResponse> getSupplierByID(string supID)
        {

            var getTbCusSups = await _eSignPrpoContext.TbCusSups.Where(x => x.TBpid == supID).FirstOrDefaultAsync();

            var response = new CusSupResponse
            {
                TCcur = getTbCusSups.TCcur,
                TBpid = getTbCusSups.TBpid,
                TNama = getTbCusSups.TNama,
                TSeak = getTbCusSups.TSeak,
                TRate = _eSignPrpoContext.TbCurrencies.Where(x => x.TCcur == getTbCusSups.TCcur).OrderByDescending(x => x.TStdt).Select(x => x.TRate).First()
            };

            return response;
        }
        public async Task<List<TbItem>> getItemCode(string typeCategoty)
        {
            if (typeCategoty == "Capex")
            {
                return await _eSignPrpoContext.TbItems.Where(x => x.TCitg == "A01").ToListAsync();
            }

            if (typeCategoty == "Non Inventory (Ex. RM / Consumable / Other)")
            {
                var listType = new List<string> { "A02", "A03" };
                return await _eSignPrpoContext.TbItems.Where(x => listType.Contains(x.TCitg)).ToListAsync();
            }

            List<string> listInventory = new List<string> { "Inventory Spare part", "Inventory RM (Domestic : DDP / DAP)", "Inventory RM (Oversea : EXW / CIF)", "Inventory RM (P&A)" };
            if (listInventory.Contains(typeCategoty))
            {
                var listType = new List<string> { "A01", "A02", "A03" };
                return await _eSignPrpoContext.TbItems.Where(x => !listType.Contains(x.TCitg)).ToListAsync();
            }

            return new List<TbItem>();

        }

        public async Task<double?> getRateByCurrency(string curr) => await _eSignPrpoContext.TbCurrencies.Where(x => x.TCcur == curr).OrderByDescending(x => x.TStdt).Select(x => x.TRate).FirstOrDefaultAsync();

        public async Task<List<TbGlnumber>> getGlCode() => await _eSignPrpoContext.TbGlnumbers.ToListAsync();
        public async Task<List<TbCostCenter>> getCostCenter() => await _eSignPrpoContext.TbCostCenters.Where(x => !string.IsNullOrEmpty(x.TDimx)).ToListAsync();
        public async Task<List<string>> getDistinctCurrency() => await _eSignPrpoContext.TbItemPrices.Select(x => x.TCcur).Distinct().ToListAsync();
        public async Task<TbItem> getItemDataByCode(string itemCode) => await _eSignPrpoContext.TbItems.Where(x => x.TItem == itemCode).FirstOrDefaultAsync();
        public async Task<TbItemPrice> getItemPrice(string itemCode) => await _eSignPrpoContext.TbItemPrices.Where(x => x.TItem == itemCode).FirstOrDefaultAsync();

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
                    SPrNo = x.SPrNo,
                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.SSupplierCode} | {x.SSupplierName}",
                    FSumAmtCurrency = x.FSumAmtCurrency,
                    FSumAmtThb = x.FSumAmtThb,
                    SStatus = getFlowName(x.NStatus),
                    NStatus = x?.NStatus,
                    DCreated = x.DCreated,
                    SCreatedBy = x.SCreatedBy,
                    UPrID = x.UPrId
                }).ToList();

                return response;
            }

            if (informationData.positionLevel == "1")
            {
                var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => x.SRwApproveId == informationData.sID && x.NRwStatus == 0 && x.NRwSteps == 1).ToListAsync();

                //if (informationData.positionLevel == "5" && getPrRequests.Count != 0)
                //{
                //    goto Step5;
                //}

                response = getPrRequests.Select(x => new PrRecordsResponse()
                {
                    SPrNo = x.SPrNo,
                    SSupplierName = $"{x.SSupplierCode} | {x.SSupplierName}",
                    FSumAmtCurrency = x.FSumAmtCurrency,
                    FSumAmtThb = x.FSumAmtThb,
                    SStatus = getFlowName(x.NStatus),
                    DCreated = x.DCreated,
                    NStatus = x?.NStatus
                }).ToList();

                return response;
            }

            var listTitle = new List<string> { "Finance and Accounting Manager", "General Manager" };
            if (listTitle.Contains(informationData.title))
            {
                int step = (int.Parse(informationData.positionLevel));
                var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => x.SRwApproveTitle == informationData.title && x.NRwStatus == 0 && x.NRwSteps == step).ToListAsync();

                response = getPrRequests.Select(x => new PrRecordsResponse()
                {
                    SPrNo = x.SPrNo,
                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.SSupplierCode} | {x.SSupplierName}",
                    FSumAmtCurrency = x.FSumAmtCurrency,
                    FSumAmtThb = x.FSumAmtThb,
                    SStatus = getFlowName(x.NStatus),
                    DCreated = x.DCreated,
                    NStatus = x?.NStatus
                }).ToList();

                return response;
            }

            if (informationData.title == "Purchasing Officer")
            {
                var getPrRequests = await _eSignPrpoContext.TbPrRequests.Where(x => x.SCreatedBy == informationData.sID).ToListAsync();


                response = getPrRequests.Select(x => new PrRecordsResponse()
                {
                    SPrNo = x.SPrNo,
                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.SSupplierCode} | {x.SSupplierName}",
                    FSumAmtCurrency = x.FSumAmtCurrency,
                    FSumAmtThb = x.FSumAmtThb,
                    SStatus = getFlowName(x.NStatus),
                    DCreated = x.DCreated,
                    SCreatedBy = x.SCreatedBy,
                    NStatus = x?.NStatus,
                    UPrID = x.UPrId
                }).ToList();

                return response;

            }

            //Step5:
            if (informationData.title == "Purchasing Manager")
            {
                var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => (x.SRwApproveTitle == informationData.title && x.NRwStatus == 0 && x.NRwSteps == 5) || (x.SRwApproveId == informationData.sID && x.NRwStatus == 0 && x.NRwSteps == 1)).ToListAsync();

                response = getPrRequests.Select(x => new PrRecordsResponse()
                {
                    SPrNo = x.SPrNo,
                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.SSupplierCode} | {x.SSupplierName}",
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

        public async Task<List<PrRecordsResponse>> getPoRecords()
        {
            var response = new List<PrRecordsResponse>();
            var informationData = _accountService.informationUser();
            if (informationData.positionLevel == "4")
            {

                var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => (x.SRwApproveTitle == informationData.title && x.NRwStatus == 0 && x.NRwSteps == 4)).ToListAsync();

                response = getPrRequests.Select(x => new PrRecordsResponse
                {
                    SPrNo = x.SPrNo,
                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.SSupplierCode} | {x.SSupplierName}",
                    FSumAmtCurrency = x.FSumAmtCurrency,
                    FSumAmtThb = x.FSumAmtThb,
                    SStatus = getFlowName(x.NStatus),
                    DCreated = x.DCreated
                }).ToList();
                return response;
            }

            if (informationData.positionLevel == "99")
            {
                var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => (x.SRwApproveId == informationData.sID && x.NRwStatus == 0 && x.NRwSteps == 6) || (x.SRwApproveId == informationData.sID && x.NRwStatus == 0 && x.NRwSteps == 7)).ToListAsync();

                response = getPrRequests.Select(x => new PrRecordsResponse()
                {
                    SPrNo = x.SPrNo,
                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.SSupplierCode} | {x.SSupplierName}",
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

            if (informationData.positionLevel == "4")
            {

                if (dateStart != null && dateEnd != null)
                {
                    DateTime dateSt = DateTime.ParseExact(dateStart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime dateN = DateTime.ParseExact(dateEnd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    TimeSpan tsStart = new TimeSpan(0, 0, 0);
                    TimeSpan tsEnd = new TimeSpan(23, 59, 0);
                    var getPrRes = await _eSignPrpoContext.VwPrReviewers.Where(x => (x.SRwApproveId == informationData.sID && x.NRwStatus == 3)
                    && (x.DCreated >= dateSt + tsStart && x.DCreated <= dateN + tsEnd))
                        .Select(x => new { x.SPrNo, x.SPoNo, x.SSupplierCode, x.SSupplierName, x.FSumAmtCurrency, x.FSumAmtThb, x.NStatus, x.DCreated })
                        .Distinct()
                        .ToListAsync();
                    response = getPrRes.Select(x => new PrRecordsResponse
                    {
                        SPrNo = x.SPrNo,
                        SPoNo = x.SPoNo,
                        SSupplierName = $"{x.SSupplierCode} | {x.SSupplierName}",
                        FSumAmtCurrency = x.FSumAmtCurrency,
                        FSumAmtThb = x.FSumAmtThb,
                        SStatus = getFlowName(x.NStatus),
                        DCreated = x.DCreated
                    }).ToList();
                }
                else
                {
                    var getPrRes = await _eSignPrpoContext.VwPrReviewers.Where(x => (x.SRwApproveId == informationData.sID && x.NRwStatus == 3)).Select(x => new { x.SPrNo, x.SPoNo, x.SSupplierCode, x.SSupplierName, x.FSumAmtCurrency, x.FSumAmtThb, x.NStatus, x.DCreated }).Distinct().ToListAsync();
                    response = getPrRes.Select(x => new PrRecordsResponse
                    {
                        SPrNo = x.SPrNo,
                        SPoNo = x.SPoNo,
                        SSupplierName = $"{x.SSupplierCode} | {x.SSupplierName}",
                        FSumAmtCurrency = x.FSumAmtCurrency,
                        FSumAmtThb = x.FSumAmtThb,
                        SStatus = getFlowName(x.NStatus),
                        DCreated = x.DCreated
                    }).ToList();
                }



                return response;
            }

            if (dateStart != null && dateEnd != null)
            {
                DateTime dateSt = DateTime.ParseExact(dateStart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime dateN = DateTime.ParseExact(dateEnd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                TimeSpan tsStart = new TimeSpan(0, 0, 0);
                TimeSpan tsEnd = new TimeSpan(23, 59, 0);

                var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => (x.SRwApproveId == informationData.sID && x.NRwStatus == 1) &&
                (x.DCreated >= dateSt + tsStart && x.DCreated <= dateN + tsEnd))
                    .Select(x => new { x.SPrNo, x.SPoNo, x.SSupplierCode, x.SSupplierName, x.FSumAmtCurrency, x.FSumAmtThb, x.NStatus, x.DCreated })
                    .Distinct()
                    .ToListAsync();

                response = getPrRequests.Select(x => new PrRecordsResponse
                {
                    SPrNo = x.SPrNo,
                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.SSupplierCode} | {x.SSupplierName}",
                    FSumAmtCurrency = x.FSumAmtCurrency,
                    FSumAmtThb = x.FSumAmtThb,
                    SStatus = getFlowName(x.NStatus),
                    DCreated = x.DCreated
                }).ToList();
            }
            else
            {
                var getPrRequests = await _eSignPrpoContext.VwPrReviewers.Where(x => (x.SRwApproveId == informationData.sID && x.NRwStatus == 1)).Select(x => new { x.SPrNo, x.SPoNo, x.SSupplierCode, x.SSupplierName, x.FSumAmtCurrency, x.FSumAmtThb, x.NStatus, x.DCreated }).Distinct().ToListAsync();

                response = getPrRequests.Select(x => new PrRecordsResponse
                {
                    SPrNo = x.SPrNo,
                    SPoNo = x.SPoNo,
                    SSupplierName = $"{x.SSupplierCode} | {x.SSupplierName}",
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


                var addPR = new TbPrRequest
                {
                    UPrId = guid,
                    SPrNo = await generatePRNo(),
                    SDepartment = informationData?.department,
                    SPostion = informationData?.position,
                    SSupplierCode = prRequest?.supplierName.Split("|")[0],
                    SSupplierName = prRequest?.supplierName.Split("|")[1],
                    SProduct = prRequest?.productsOption,
                    SWh = prRequest?.wh,
                    SCategory = prRequest?.categoryOption,
                    SCapexNo = prRequest?.capexNo,
                    STypeAsset = prRequest?.assetOption,
                    SAssentName = prRequest?.assetName,
                    SAssetNo = prRequest?.refAssetNo,
                    NStatus = 1,
                    FSumAmtCurrency = double.Parse(prRequest?.totalAmount.Replace(",", "")),
                    FSumAmtThb = double.Parse(prRequest?.totalAmountTHB.Replace(",", "")),
                    SReason = prRequest?.reason,
                    SCreatedBy = informationData?.sID,
                    SCreatedName = informationData?.name,
                    DCreated = DateTime.Now,
                    SCurrency = listPRPOItem.Select(x => x.currency).FirstOrDefault(),
                    FRate = prRequest?.rate,
                    BIsVat = true

                };

                _eSignPrpoContext.TbPrRequests.Add(addPR);

                var addPRListItem = listPRPOItem.Select(x => new TbPrRequestItem
                {
                    UPrItemId = Guid.NewGuid(),
                    NNo = int.Parse(x?.no),
                    SItem = x?.item,
                    SItemDesc = x?.itemDesc,
                    SGlCode = x?.glCode,
                    SCostCenter = x?.costCenter,
                    DRequestDate = DateTime.Parse(x?.requestDate),
                    NQty = int.Parse(x?.qty),
                    SUom = x?.Uom,
                    FUnitCost = double.Parse(x?.unitCost.Replace(",", "")),
                    SCurrency = x?.currency,
                    FAmount = double.Parse(x?.amount.Replace(",", "")),
                    NStatus = 1,
                    DCreated = DateTime.Now,
                    SPrNo = addPR.SPrNo


                }).ToList();

                _eSignPrpoContext.TbPrRequestItems.AddRange(addPRListItem);

                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                if (response)
                {
                    _logger.LogInformation($"Create PR : {addPR.SPrNo} is success , by user : [{addPR.SCreatedBy}] {addPR.SCreatedName} ");

                    await _workflowService.generateWorkflow(addPR?.SDepartment, addPR?.SPrNo);

                    //var nextPosition = informationData.positionLevel == "4" ? 5 : 1;

                    await _mailService.sendEmail(addPR?.SPrNo, 1, 1, null);


                }

                return Tuple.Create(response, $"Create PR No. : {addPR?.SPrNo} is success.");
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

                responsePR.SDepartment = informationData?.department;
                responsePR.SPostion = informationData?.position;
                responsePR.SSupplierCode = prRequest?.supplierName.Split("|")[0];
                responsePR.SSupplierName = prRequest?.supplierName.Split("|")[1];
                responsePR.SProduct = prRequest?.productsOption;
                responsePR.SWh = prRequest?.wh;
                responsePR.SCategory = prRequest?.categoryOption;
                responsePR.SCapexNo = prRequest?.capexNo;
                responsePR.STypeAsset = prRequest?.assetOption;
                responsePR.SAssentName = prRequest?.assetName;
                responsePR.SAssetNo = prRequest?.refAssetNo;
                responsePR.FSumAmtCurrency = double.Parse(prRequest?.totalAmount.Replace(",", ""));
                responsePR.FSumAmtThb = double.Parse(prRequest?.totalAmountTHB.Replace(",", ""));
                responsePR.SReason = prRequest?.reason;
                responsePR.DUpdated = DateTime.Now;
                responsePR.SCurrency = listPRPOItem.Select(x => x.currency).FirstOrDefault();
                responsePR.FRate = prRequest?.rate;

                if (isReSubmit == "1")
                {
                    responsePR.NStatus = 1;
                }


                var reponseListPR = await getPrRequestItemByNo(responsePR.SPrNo);

                _eSignPrpoContext.TbPrRequestItems.RemoveRange(reponseListPR);

                var addPRListItem = listPRPOItem.Select(x => new TbPrRequestItem
                {
                    UPrItemId = Guid.NewGuid(),
                    NNo = int.Parse(x?.no),
                    SItem = x?.item,
                    SItemDesc = x?.itemDesc,
                    SGlCode = x?.glCode,
                    SCostCenter = x?.costCenter,
                    DRequestDate = DateTime.Parse(x?.requestDate),
                    NQty = int.Parse(x?.qty),
                    SUom = x?.Uom,
                    FUnitCost = double.Parse(x?.unitCost.Replace(",", "")),
                    SCurrency = x?.currency,
                    FAmount = double.Parse(x?.amount.Replace(",", "")),
                    NStatus = 1,
                    DCreated = DateTime.Now,
                    SPrNo = responsePR.SPrNo

                }).ToList();

                _eSignPrpoContext.TbPrRequestItems.AddRange(addPRListItem);

                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                if (response)
                {
                    if (isReSubmit == "1")
                    {
                        await _workflowService.generateWorkflow(responsePR?.SDepartment, responsePR?.SPrNo);

                        await _mailService.sendEmail(responsePR?.SPrNo, 1, 1, null);

                        _logger.LogInformation($"Re-Submit PR : {responsePR.SPrNo} is success , by user : [{responsePR.SCreatedBy}] {responsePR.SCreatedName} ");
                    }
                    else
                    {
                        _logger.LogInformation($"Update PR : {responsePR.SPrNo} is success , by user : [{responsePR.SCreatedBy}] {responsePR.SCreatedName} ");
                    }

                }

                return Tuple.Create(response, $"{(isReSubmit == "1" ? "Re-Submit" : "Update")} PR No. : {responsePR.SPrNo} is success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Tuple.Create(false, ex.Message); ;
            }
        }

        public async Task<string> generatePRNo()
        {
            var prNo = $"PR{DateTime.Now.ToString("yyMM")}00001";
            var getPrDesc = await _eSignPrpoContext.TbPrRequests.OrderByDescending(x => x.SPrNo).FirstOrDefaultAsync();

            if (getPrDesc != null)
            {
                if (getPrDesc.SPrNo.Substring(4, 2) == DateTime.Now.ToString("MM"))
                {
                    int _numgetPrDesc = Convert.ToInt16(getPrDesc.SPrNo.Substring(6, 5)) + 1;
                    prNo = $"PR{DateTime.Now.ToString("yyMM")}{_numgetPrDesc.ToString("00000")}";
                    return prNo;
                }

                if (getPrDesc.SPrNo.Substring(2, 2) == DateTime.Now.ToString("yy"))
                {
                    int _numgetPrDesc = Convert.ToInt16(getPrDesc.SPrNo.Substring(6, 5)) + 1;
                    prNo = $"PR{DateTime.Now.ToString("yyMM")}{_numgetPrDesc.ToString("00000")}";
                    return prNo;
                }
            }

            return prNo;
        }


        public async Task<ApproverPRDetailResponse> getPRAllDetail(string prNo)
        
        {
            var response = new ApproverPRDetailResponse();

            var getPRByNo = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPrNo == prNo).FirstOrDefaultAsync();

            var getPRItemByNo = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.SPrNo == prNo).ToListAsync();

            var getFiles = await _eSignPrpoContext.TbAttachments.Where(x => x.UPrId == getPRByNo.UPrId).ToListAsync();

            var getFlowPR = await _eSignPrpoContext.TbPrReviewers.OrderBy(x => x.NRwStatus).Where(x => x.SPrNo == prNo && x.NRwStatus != 9 && x.NRwSteps != 99).ToListAsync(); ;

            var getRejectFlow = await _eSignPrpoContext.TbPrReviewers.OrderByDescending(x => x.DCreated).Where(x => x.SPrNo == prNo && x.NRwStatus == 9 && x.BIsReject == true).ToListAsync();

            var getShipVia = await _eSignPrpoContext.TbShipVia.Where(x => x.TSfbp == getPRByNo.SSupplierCode).FirstOrDefaultAsync();
            var informationData = _accountService.informationUser();

            var checkPermision = getFlowPR.Where(x => (x.SRwApproveId == informationData.sID || x.SRwApproveTitle == informationData.title) && x.NRwStatus == 0).Count();

            response = new ApproverPRDetailResponse()
            {
                checkPermission = checkPermision > 0,
                prNo = getPRByNo?.SPrNo,
                poNo = getPRByNo?.SPoNo,
                createdDate = getPRByNo?.DCreated,
                capexNo = getPRByNo?.SCapexNo,
                categoryType = getPRByNo?.SCategory,
                totalAmount = getPRByNo?.FSumAmtCurrency?.ToString("N"),
                totalAmountTHB = getPRByNo?.FSumAmtThb?.ToString("N"),
                productsType = getPRByNo?.SProduct,
                wh = getPRByNo?.SWh,
                supplierCode = getPRByNo?.SSupplierCode,
                supplierName = $"{getPRByNo?.SSupplierCode} | {getPRByNo?.SSupplierName}",
                refAssetNo = getPRByNo?.SAssetNo,
                assetName = getPRByNo?.SAssentName,
                assetType = getPRByNo?.STypeAsset,
                reason = getPRByNo?.SReason,
                status = getPRByNo.NStatus,
                shipVia = getShipVia?.TDsca,
                isVat = getPRByNo?.BIsVat,
                deliveryDate = getPRByNo?.DDeliveryDate?.ToString("dd/MM/yyyy"),
                listPRPOItems = getPRItemByNo.Select(x => new listPRPOItem
                {
                    uPrItemId = x?.UPrItemId,
                    no = x?.NNo?.ToString(),
                    item = x?.SItem,
                    itemDesc = x?.SItemDesc,
                    Uom = x?.SUom,
                    qty = x?.NQty.ToString(),
                    amount = x?.FAmount?.ToString("N"),
                    amountNumber = x?.FAmount,
                    costCenter = x?.SCostCenter,
                    currency = x?.SCurrency,
                    glCode = x?.SGlCode,
                    unitCost = x?.FUnitCost?.ToString("N"),
                    requestDate = x?.DRequestDate?.ToString("dd-MM-yyyy")

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


        public async Task<bool> updatePRItem(Guid prItemId, string itemDesc, string qty, double? amount)
        {
            try
            {
                var getPrItem = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.UPrItemId == prItemId).FirstOrDefaultAsync();

                getPrItem.SItemDesc = itemDesc;
                getPrItem.NQty = int.Parse(qty);
                getPrItem.FUnitCost = amount;
                getPrItem.FAmount = int.Parse(qty) * amount;
                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

                var listPritem = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.SPrNo == getPrItem.SPrNo).ToListAsync();

                var getCurr = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPrNo == getPrItem.SPrNo).FirstOrDefaultAsync();

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
                    res.prNo = itemPR?.SPrNo;
                    res.userCreatePR = itemPR?.SCreatedBy;
                    res.prIssuedDate = itemPR?.DCreated?.ToString("dd/MM/yyyy");

                   
                    res.supplierCode = itemPR?.SSupplierCode;
                    res.supplierName = itemPR?.SSupplierName;
                    res.ReferenceA = itemPR?.SReason;
                    res.capexNo = itemPR?.SCapexNo;
                    res.assetName = itemPR?.SAssentName;
                    res.refAsset = itemPR?.STypeAsset;
                    res.location = itemPR?.SProduct;

                    string requisitionType = "";

                    string[] issuedStatus = new string[] { "0", "1","2","3" };
                    if (issuedStatus.Contains(itemPR?.NStatus?.ToString()))
                    {
                        requisitionType = "PR Issued";
                    }else if (itemPR?.NStatus <= 4)
                    {
                        requisitionType = "Approved";
                    }
                    else if (itemPR?.NStatus >= 5)
                    {
                        requisitionType = "Converted";
                    }

                    res.requisitionType = requisitionType;

                    if (requisitionType != "PR Issued")
                    {
                        res.poNo = itemPR?.SPoNo;
                        var getPoCreated = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPrNo == itemPR.SPrNo && (x.NRwSteps == 4 && x.NRwStatus == 3)).FirstOrDefaultAsync();
                        res.userCreatePO = getPoCreated?.SRwApproveName;
                        res.poIssuedDate = itemPR?.DConvertToPo?.ToString("dd/MM/yyyy");
                    }

                    string poStatus = "Inprocess";
                    if (itemPR.NStatus == 7)
                    {
                        poStatus = "Closed";
                    }

                    res.poStatus = poStatus;


                    res.expectDeliveryDate = itemPR?.DDeliveryDate?.ToString("dd/MM/yyyy");
                    res.warehouse = itemPR?.SWh;

                    response.Add(res);
                }
            }

            return response;
        }
    }
}
