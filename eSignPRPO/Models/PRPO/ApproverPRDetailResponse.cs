using Org.BouncyCastle.Bcpg.Sig;
using System.ComponentModel.DataAnnotations;

namespace eSignPRPO.Models.PRPO
{
    public class ApproverPRDetailResponse
    {
        public bool checkPermission { get; set; }
        public string prNo { get; set; }
        public string poNo { get; set; }
        public string supplierCode { get; set; }
        public string supplierName { get; set; }
        public string categoryType { get; set; }
        public string capexNo { get; set; }
        public string assetType { get; set; }
        public string assetName { get; set; }
        public string refAssetNo { get; set; }
        public string productsType { get; set; }
        public string other { get; set; }
        public string totalAmount { get; set; }
        public string totalAmountTHB { get; set; }
        public string reason { get; set; }
        public string deliveryDate { get; set; }
        public int? status { get; set; }
        public string wh { get; set; }
        public string shipVia { get; set; }
        public string currency { get; set; }
        public DateTime? createdDate { get; set; }
        public string paymentCondition { get; set; }
        public string termCondition { get; set; }
        public string vatTotal { get; set; }
        public string totalAmountVatTHB { get; set; }
        public string createdBy { get; set; }
        public List<listPRPOItem> listPRPOItems { get; set; }
        public List<fileUpload> fileUploads { get; set; }

        public List<flowPR> flowReject { get; set; }
        public List<flowPR> flowPRs { get; set; }
        public address supplierInfo { get; set; }
        public address shipToInfo { get; set; }
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

    public class address
    {
        public string name { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address3 { get; set; }
        public string address4 { get; set; }
        public string tel { get; set; }
        public string fax { get; set; }
        public string contact { get; set; }
        public string email { get; set; }
    }
}

