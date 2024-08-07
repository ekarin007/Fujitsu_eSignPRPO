using eSignPRPO.Models;
using eSignPRPO.Models.PRPO;

namespace eSignPRPO.interfaces
{
    public interface IPRPOService
    {
        Task<TbPrRequest> getPrRequestByNo(Guid prGuid);
        Task<List<TbPrRequestItem>> getPrRequestItemByNo(string prNo);
        Task<List<TbCusSup>> getSupplierData();
        Task<List<TbItem>> getItemCode(string typeCategoty);
        Task<TbItem> getItemDataByCode(string itemCode);
        Task<List<TbGlnumber>> getGlCode();
        Task<List<TbCostCenter>> getCostCenter();
        Task<List<string>> getDistinctCurrency();
        Task<TbItemPrice> getItemPrice(string itemCode);
        Task<CusSupResponse> getSupplierByID(string supID);
        Task<double?> getRateByCurrency(string curr);
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


    }
}
