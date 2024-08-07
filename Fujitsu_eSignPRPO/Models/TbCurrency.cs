using System;
using System.Collections.Generic;

namespace eSignPRPO.Models;

public partial class TbCurrency
{
    public string TBcur { get; set; }

    public string TCcur { get; set; }

    public string TRtyp { get; set; }

    public DateTime TStdt { get; set; }

    public double? TRate { get; set; }
}
