using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class TbPrRequestItem
{
    public Guid UPrItemId { get; set; }

    public int? NNo { get; set; }

    public string SPartNo { get; set; }

    public string SPartName { get; set; }

    public string SVatType { get; set; }

    public double? FUnitPrice { get; set; }

    public int? NQty { get; set; }

    public double? FAmount { get; set; }

    public int? NStatus { get; set; }

    public string SPoNo { get; set; }

    public DateTime? DCreated { get; set; }
}
