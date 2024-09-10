using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class TbAccountCode
{
    public Guid UAcGuid { get; set; }

    public string MainCode { get; set; }

    public string SubCode1 { get; set; }

    public string SubCode2 { get; set; }

    public double? Budget { get; set; }

    public double? Balance { get; set; }

    public bool? Active { get; set; }

    public string SCreatedBy { get; set; }

    public DateTime? DCreatedDate { get; set; }

    public string SUpdatedBy { get; set; }

    public DateTime? DUpdatedBy { get; set; }
}
