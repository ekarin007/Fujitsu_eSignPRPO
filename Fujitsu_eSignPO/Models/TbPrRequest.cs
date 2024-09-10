using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class TbPrRequest
{
    public Guid UPoId { get; set; }

    public string SPoNo { get; set; }

    public string SVendorCode { get; set; }

    public string SVendorName { get; set; }

    public string SDepartment { get; set; }

    public string SRefQuotation { get; set; }

    public string SCurrency { get; set; }

    public double? FRate { get; set; }

    public DateTime? DShippingDate { get; set; }

    public string SMainCode { get; set; }

    public string SSubCode1 { get; set; }

    public string SSubCode2 { get; set; }

    public double? FBudget { get; set; }

    public double? FBalance { get; set; }

    public double? FSumAmtCurrency { get; set; }

    public double? FSumAmtThb { get; set; }

    public string SReason { get; set; }

    public int? NStatus { get; set; }

    public string SVatType { get; set; }

    public DateTime? DDeliveryDate { get; set; }

    public DateTime? DPoDate { get; set; }

    public string SCreatedBy { get; set; }

    public string SCreatedName { get; set; }

    public DateTime? DCreated { get; set; }

    public DateTime? DUpdated { get; set; }

    public DateTime? DAcceptIvoiceDate { get; set; }
}
