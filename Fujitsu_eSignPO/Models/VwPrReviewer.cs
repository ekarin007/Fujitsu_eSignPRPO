using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class VwPrReviewer
{
    public string SPoNo { get; set; }

    public string SDepartment { get; set; }

    public string VendorName { get; set; }

    public int? NStatus { get; set; }

    public double? FSumAmtCurrency { get; set; }

    public double? FSumAmtThb { get; set; }

    public string SCreatedBy { get; set; }

    public string SCreatedName { get; set; }

    public DateTime? DCreated { get; set; }

    public string SRwApproveId { get; set; }

    public Guid URwId { get; set; }

    public string SRwApproveName { get; set; }

    public string SRwApproveDepartment { get; set; }

    public string SRwApproveTitle { get; set; }

    public DateTime? DRwApproveDate { get; set; }

    public int? NRwSteps { get; set; }

    public int? NRwStatus { get; set; }

    public DateTime? DRwCreated { get; set; }

    public string SRwRemark { get; set; }

    public string SMainCode { get; set; }

    public string SSubCode1 { get; set; }

    public string SSubCode2 { get; set; }

    public DateTime? DPoDate { get; set; }

    public DateTime? DAcceptIvoiceDate { get; set; }

    public string SVendorName { get; set; }

    public string SCurrency { get; set; }

    public double? FRate { get; set; }

    public Guid? UFkPrid { get; set; }

    public Guid UPoId { get; set; }

    public DateTime? DDueDate { get; set; }

    public string SProject { get; set; }
}
