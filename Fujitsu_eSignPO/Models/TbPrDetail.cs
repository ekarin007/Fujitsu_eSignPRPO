using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class TbPrDetail
{
    public int Dtranid { get; set; }

    public string Prnumber { get; set; }

    public decimal Seq { get; set; }

    public string Item { get; set; }

    public string Itemdesc { get; set; }

    public string Glcode { get; set; }

    public string Costcenter { get; set; }

    public DateTime Requestdate { get; set; }

    public decimal Qty { get; set; }

    public string Uom { get; set; }

    public double UnitCost { get; set; }

    public string Curr { get; set; }

    public double Amtcurr { get; set; }

    public double Amtthb { get; set; }

    public double Sumamtcurr { get; set; }

    public double Sumamtthb { get; set; }

    public string Reason { get; set; }

    public string FlagSatus { get; set; }
}
