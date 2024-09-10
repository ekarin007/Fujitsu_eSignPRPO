using Org.BouncyCastle.Bcpg.Sig;
using System.ComponentModel.DataAnnotations;

namespace Fujitsu_eSignPO.Models.PRPO
{
    public class ApproverPRDetailResponse
    {
        public bool checkPermission { get; set; }
        public string poNo { get; set; }
        public string vendorName { get; set; }
        public string department { get; set; }
        public string refQuotation { get; set; }
        public string vatType { get; set; }
        public string currency { get; set; }
        public string rate { get; set; }
        public string shippingDate { get; set; }
        public string poDate { get; set; }
        public string mainCode { get; set; }
        public string subCode1 { get; set; }
        public string subCode2 { get; set; }
        public string budget { get; set; }
        public string balance { get; set; }
        public string totalAmount { get; set; }
        public string totalAmountTHB { get; set; }
        public string reason { get; set; }
        public string deliveryDate { get; set; }
        public int? status { get; set; }              
        public DateTime? createdDate { get; set; }     
        public string vatTotal { get; set; }
        public string totalAmountVatTHB { get; set; }
        public string createdBy { get; set; }
        public List<listPRPOItem> listPRPOItems { get; set; }
        public List<fileUpload> fileUploads { get; set; }

        public List<flowPR> flowReject { get; set; }
        public List<flowPR> flowPRs { get; set; }      
        public bool? isVat { get; set; }
    }

    public class fileUpload
    {
        public Guid? uPrId { get; set; }
        public bool? isSendToSupplier { get; set; }
        public string sAttach_Name { get; set; }
        public string sAttach_File_Type { get; set; }
        public string sAttach_File_Size { get; set; }
        public Guid? sAttach_Id { get; set; }
    }

    public class flowPR
    {
        public string sRw_Approve_ID { get; set; }
        public string sRw_Approve_Name { get; set; }
        public string sRW_Approve_Title { get; set; }
        public DateTime? dRW_Approve_Date { get; set; }
        public int? nRW_Steps { get; set; }
        public string sRW_Status { get; set; }
        public string sRw_Remark { get; set; }
    }

    
}

