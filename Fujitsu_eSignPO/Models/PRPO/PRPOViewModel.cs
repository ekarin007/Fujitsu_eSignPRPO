using System.ComponentModel.DataAnnotations;

namespace Fujitsu_eSignPO.Models.PRPO
{
    public class PRPOViewModel
    {
        [Required(ErrorMessage = "Vendor is required.")]
        public string vendorName { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public string department { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        public string currency { get; set; }
     
        public DateTime? shippingDate { get; set; }

        [Required(ErrorMessage = "PO Date is required.")]
        public DateTime? poDate { get; set; }

        [Required(ErrorMessage = "Main Code is required.")]
        public string mainCode { get; set; }

        [Required(ErrorMessage = "subCode1 is required.")]
        public string subCode1 { get; set; }

        [Required(ErrorMessage = "subCode2 is required.")]
        public string subCode2 { get; set; }

        public double? budget { get; set; }
        public double? balance { get; set; }


        [Required(ErrorMessage = "VAT is required.")]
        public string vatOption { get; set; }
     

        [Required(ErrorMessage = "Total Amount is required.")]
        public string totalAmount { get; set; }

        [Required(ErrorMessage = "Total Amount in THB is required.")]
        public string totalAmountTHB { get; set; }

        public double? rate { get; set; }

        public string reason { get; set; }

        public int? nStatus { get; set; }

        public string refQuatation { get; set; }

        public List<listPRPOItem> listPRPOItems { get; set; }
        public List<fileUpload> fileUploads { get; set; }

    }

    public class listPRPOItem
    {
        public Guid? uPoItemId { get; set; }
        public string no { get; set; }
        public string partNo { get; set; }
        public string partName { get; set; }
        public string vatType { get; set; }
        public string unitPrice { get; set; }
       
        public string qty { get; set; }      
        public string amount { get; set; }

        public double? amountNumber { get; set; }
    }


}
