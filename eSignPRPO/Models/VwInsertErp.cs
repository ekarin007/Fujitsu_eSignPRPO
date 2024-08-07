using System;
using System.Collections.Generic;

namespace eSignPRPO.Models;

public partial class VwInsertErp
{
    public string SPoNo { get; set; }

    public string SSupplierCode { get; set; }

    public DateTime? DConvertToPo { get; set; }

    public string SCurrency { get; set; }

    public string SWh { get; set; }

    public string SProduct { get; set; }

    public string SCreatedBy { get; set; }

    public string SReason { get; set; }

    public string SCapexNo { get; set; }

    public double? FSumAmtThb { get; set; }

    public int? NStatus { get; set; }

    public DateTime? DCreated { get; set; }
}
