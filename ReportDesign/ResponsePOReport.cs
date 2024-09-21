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
        public string reference { get; set; }
        public string department { get; set; }
        public string vendorName { get; set; }

        public string shippingDate { get; set; }
        public string non_Vat { get; set; }
        public string total_Exclude_Vat { get; set; }
        public string vat_7 { get; set; }
        public string totalSum_Vat { get; set; }
        public string prepareBy { get; set; }
        public string prepareBy_FullName { get; set; }

    }

    public class POItem
    {
        public string no { get; set; }
        public string partNo { get; set; }
        public string partName { get; set; }
        public string unitPrice { get; set; }
        public string qty { get; set; }
        public string amount { get; set; }
    }
}
