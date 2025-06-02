using DocumentFormat.OpenXml.Office.CustomUI;
using System.Reflection.Metadata.Ecma335;

namespace Fujitsu_eSignPO.Models.PRPO
{
    public class comparePOModel
    {
        public string poGuid { get; set; }
        public double? fBudget { get; set; }
        public double? fBalance { get; set; }
        public string sVendor1 { get; set; }
        public string sVendor2 { get; set; }
        public string sVendorResult { get; set; }
        public string sRemarkResult { get; set; }
        public double? fTotalPriceResult { get; set; }
        public DateTime? dVdr1_Quotation_Date { get; set; }
        public DateTime? dVdr2_Quotation_Date { get; set; }
       
    }

    public class CompareItemModel
    {
        public string Guid { get; set; }
        public string Item { get; set; }
        public string BrandModel { get; set; }
        public double Amount { get; set; }
        public double Vendor1Amount { get; set; }
        public double Vendor2Amount { get; set; }
        public DateTime CreatedAt { get; set; }

        public string poGuid { get; set; }
    }

    public class compareViewModel
    {
        public string poGuid { get; set; }
        public double? fBudget { get; set; }
        public double? fBalance { get; set; }
        public string sVendor1 { get; set; }
        public string sVendor2 { get; set; }
        public string sVendorResult { get; set; }
        public string sRemarkResult { get; set; }
        public double? fTotalPriceResult { get; set; }
        public DateTime? dVdr1_Quotation_Date { get; set; }
        public DateTime? dVdr2_Quotation_Date { get; set; }
        public DateTime? dCreateDate { get; set; }
        public string division { get; set; }
        public string requestor { get; set; }
        public string reqId { get; set; }
        public List<CompareItemModel> compareItem { get; set; }
        public List<fileUpload> FileUploads { get; set; }
    }
}
