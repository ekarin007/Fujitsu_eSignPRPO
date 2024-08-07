using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesign
{
    public class ResponsePOReport
    {
        public string poNo { get; set; }
        public string datePo { get; set; }
        public string capex { get; set; }
        public string supplierCode { get; set; }

        public string currency { get; set; }
        public string shipVia { get; set; }
        public string termCondition { get; set; }
        public string paymentCondition { get; set; }
        public string subAmount { get; set; }
        public string vatAmount { get; set; }
        public string totalAmount { get; set; }
        public string remarks { get; set; }
        public string supplierName { get; set; }
        public string supplieAddress { get; set; }
        public string billToName { get; set; }
        public string billToAddress { get; set; }

    }

    public class POItem
    {
        public string no { get; set; }
        public string itemCode { get; set; }
        public string description { get; set; }
        public string quantity { get; set; }
        public string uom { get; set; }
        public string unitPrice { get; set; }
        public string amount { get; set; }
        public string deliveryDate { get; set; }
    }
}
