using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class TbCompareList
{
    public Guid UGuid { get; set; }

    public string SCpItem { get; set; }

    public string SBrandModel { get; set; }

    public double? FVendorAmount1 { get; set; }

    public double? FVendorAmount2 { get; set; }

    public DateTime? DCreateDate { get; set; }

    public Guid? UFkPrid { get; set; }

    public double? FAmount { get; set; }
}
