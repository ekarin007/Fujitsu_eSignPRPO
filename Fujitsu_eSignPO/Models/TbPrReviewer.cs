using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class TbPrReviewer
{
    public Guid URwId { get; set; }

    public string SRwApproveId { get; set; }

    public string SRwApproveName { get; set; }

    public string SRwApproveDepartment { get; set; }

    public string SRwApproveTitle { get; set; }

    public DateTime? DRwApproveDate { get; set; }

    public int? NRwSteps { get; set; }

    public int? NRwStatus { get; set; }

    public bool? BIsReject { get; set; }

    public DateTime? DCreated { get; set; }

    public string SPoNo { get; set; }

    public string SRwRemark { get; set; }
}
