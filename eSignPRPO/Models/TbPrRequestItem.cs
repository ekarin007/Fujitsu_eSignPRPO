using System;
using System.Collections.Generic;

namespace eSignPRPO.Models;

public partial class TbPrRequestItem
{
    public Guid UPrItemId { get; set; }

    public int? NNo { get; set; }

    public string SItem { get; set; }

    public string SItemDesc { get; set; }

    public string SGlCode { get; set; }

    public string SCostCenter { get; set; }

    public DateTime? DRequestDate { get; set; }

    public int? NQty { get; set; }

    public string SUom { get; set; }

    public double? FUnitCost { get; set; }

    public string SCurrency { get; set; }

    public double? FAmount { get; set; }

    public int? NStatus { get; set; }

    public string SPrNo { get; set; }

    public DateTime? DCreated { get; set; }
}
