namespace Fujitsu_eSignPO.Models.PRPO
{
    public class PrRecordsResponse
    {
        public Guid UPoID { get; set; }
        public string SPoNo { get; set; }
        public string SSupplierName { get; set; }
        public double? FSumAmtCurrency { get; set; }
        public double? FSumAmtThb { get; set; }
        public int? NStatus { get; set; }
        public string SStatus { get; set; }
        public DateTime? DCreated { get; set; }
        public string SCreatedBy { get; set; }
        public string SCreatedByName { get; set; }
        public DateTime? DDueDate { get; set; }
        public string SMainCode { get; set; }
        public string SSubCode1 { get; set; }
        public string SSubCode2 { get; set; }
    }
}
