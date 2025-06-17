using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class TbCompare
{
    public Guid UGuid { get; set; }

    public double? FBudget { get; set; }

    public double? FBalance { get; set; }

    public string SVendor1 { get; set; }

    public string SVendor2 { get; set; }

    public string SVendorResult { get; set; }

    public string SRemarkResult { get; set; }

    public double? FTotalPriceResult { get; set; }

    public DateTime? DCreateDate { get; set; }

    public DateTime? DVdr1QuatationDate { get; set; }

    public DateTime? DVdr2QuatationDate { get; set; }
}
