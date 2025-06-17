using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class VwPoCompare
{
    public Guid UPoId { get; set; }

    public string SPoNo { get; set; }

    public string SDepartment { get; set; }

    public string SVendor1 { get; set; }

    public string SVendor2 { get; set; }

    public string SVendorResult { get; set; }

    public string SCpItem { get; set; }

    public double? FVendorAmount1 { get; set; }

    public double? FVendorAmount2 { get; set; }

    public DateTime? DCreateDate { get; set; }
}
