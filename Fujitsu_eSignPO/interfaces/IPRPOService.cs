using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.PRPO;

namespace Fujitsu_eSignPO.interfaces
{
    public interface IPRPOService
    {
        Task<TbPrRequest> getPrRequestByNo(Guid prGuid);
        Task<List<TbPrRequestItem>> getPrRequestItemByNo(string prNo);
        Task<List<TbVendor>> getVendorData();
        Task<List<TbDepartment>> getDepData();
        Task<List<TbCurrency>> getCurrData();
        //Task<List<TbItem>> getItemCode(string typeCategoty);
        Task<List<string>> getMainCode();
        Task<List<string>> getSubCode1(string mainCode);
        Task<List<string>> getSubCode2(string subCode1);
        Task<List<string>> getSubCode3(string subCode2);
        Task<TbAccountCode> getBudgetBalance(string mainCode, string subCode1, string subCode2);
        Task<TbAccountCode> getBudgetBalance2(string mainCode, string subCode1, string subCode2, string subCode3);

        //Task<double?> getRateByCurrency(string curr);
        Task<List<TbAttachment>> getAttachmentsData(Guid guid);
        Task<bool> InsertAttachment(List<IFormFile> files, Guid guid);
        Task<bool> DeleteFile(string fileName, Guid guid);

        Task<Tuple<bool, string>> InsertPR(PRPOViewModel prRequest, Guid guid);
        Task<Tuple<bool, string>> UpdatePR(PRPOViewModel prRequest, Guid guid, string isReSubmit);
        Task<Tuple<bool, string>> UpdatePrByAppr2(PRPOViewModel prRequest, Guid guid);
        Task<List<PrRecordsResponse>> getPrRecords();
        Task<List<PrRecordsResponse>> getPoRecords();
        Task<ApproverPRDetailResponse> getPRAllDetail(string prNo);
        Task<bool> updatePRItem(Guid prItemId, string itemDesc, string qty, double? amount);
        Task<List<PrRecordsResponse>> getPOHistory(string dateStart, string dateEnd , string flowStatus, string vendorName, string department , string project , string mc, string sc1, string sc2, string sc3, string reqName);
        Task<TbWhLocation> getWH(string category, string product);

        Task<List<ExportAllPRModel>> getAllPrModel(DateTime dateStart, DateTime dateEnd,string flowStatus );

        public string getVendorName(string vc);

        Task<string> getVendorEmail(string vc);

        Task<JsonResponse> InsertUpdatePR_DRAFT(PRPOViewModel prRequest, Guid guid);

    }
}
