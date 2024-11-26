namespace Fujitsu_eSignPO.Models.PRPO
{
    public class ExportAllPRModel
    {

        public string poNo { get; set; }
        public string createdName { get; set; }
        public string department { get; set; }
        public string vendorName { get; set; }
        public string curr { get; set; }
        public string rate { get; set; }
        public string status { get; set; }
        
        public string sumAmtCurr { get; set; }
        public string sumAmtTHB { get; set; }
       
        public string createDate { get; set; }
        public string poDate { get; set;}
        public string dateOfInvoice { get; set; }
        public string mainCode { get; set; }
        public string subCode1 { get; set; }
        public string subCode2 { get; set; }

        public string budget { get; set; }
    }
}
