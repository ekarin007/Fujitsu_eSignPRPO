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
        Task<TbAccountCode> getBudgetBalance(string mainCode, string subCode1, string subCode2);
      
        //Task<double?> getRateByCurrency(string curr);
        Task<List<TbAttachment>> getAttachmentsData(Guid guid);
        Task<bool> InsertAttachment(List<IFormFile> files, Guid guid);
        Task<bool> DeleteFile(string fileName, Guid guid);

        Task<Tuple<bool, string>> InsertPR(PRPOViewModel prRequest, List<listPRPOItem> listPRPOItem, Guid guid);
        Task<Tuple<bool, string>> UpdatePR(PRPOViewModel prRequest, List<listPRPOItem> listPRPOItem, Guid guid,string isReSubmit);
        Task<List<PrRecordsResponse>> getPrRecords();
        Task<List<PrRecordsResponse>> getPoRecords();
        Task<ApproverPRDetailResponse> getPRAllDetail(string prNo);
        Task<bool> updatePRItem(Guid prItemId, string itemDesc, string qty, double? amount);
        Task<List<PrRecordsResponse>> getPOHistory(string dateStart, string dateEnd);
        Task<TbWhLocation> getWH(string category, string product);

        Task<List<ExportAllPRModel>> getAllPrModel(DateTime dateStart, DateTime dateEnd);

        public string getVendorName(string vc);


    }
}
