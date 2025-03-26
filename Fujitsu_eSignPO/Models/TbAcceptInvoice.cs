using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class TbAcceptInvoice
{
    public Guid UGuid { get; set; }

    public string SInvoiceNo { get; set; }

    public string SPoNo { get; set; }

    public string SType { get; set; }

    public DateTime? DAcceptDate { get; set; }

    public double? FPrice { get; set; }

    public double? FSteel { get; set; }

    public double? FCopper { get; set; }

    public double? FBrass { get; set; }

    public double? FAluminum { get; set; }

    public double? FOther { get; set; }

    public string SRemark { get; set; }

    public DateTime? DCreatedDate { get; set; }
}
