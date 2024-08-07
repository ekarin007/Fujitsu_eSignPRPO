using System.ComponentModel.DataAnnotations;

namespace eSignPRPO.Models.PRPO
{
    public class PRPOViewModel
    {
        [Required(ErrorMessage = "Supplier Name is required.")]
        public string supplierName { get; set; }

        [Required(ErrorMessage = "Categorys is required.")]
        public string categoryOption { get; set; }

        public string capexNo { get; set; }
        public string assetOption { get; set; }
        public string assetName { get; set; }
        public string refAssetNo { get; set; }

        [Required(ErrorMessage = "Products is required.")]
        public string productsOption { get; set; }
        public string wh { get; set; }

        [Required(ErrorMessage = "Total Amount is required.")]
        public string totalAmount { get; set; }

        [Required(ErrorMessage = "Total Amount in THB is required.")]
        public string totalAmountTHB { get; set; }

        public double? rate { get; set; }

        public string reason { get; set; }

        public int? nStatus { get; set; }

        public List<listPRPOItem> listPRPOItems { get; set; }
        public List<fileUpload> fileUploads { get; set; }

        public bool? bIsVat { get; set; }
    }

    public class listPRPOItem
    {
        public Guid? uPrItemId { get; set; }
        public string no { get; set; }
        public string item { get; set; }
        public string itemDesc { get; set; }
        public string glCode { get; set; }
        public string costCenter { get; set; }
        public string requestDate { get; set; }
        public string qty { get; set; }
        public string Uom { get; set; }
        public string unitCost { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }

        public double? amountNumber { get; set; }
    }


}
