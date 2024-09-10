using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class TbMicRuning
{
    public string OuCode { get; set; }

    public string Year { get; set; }

    public string Month { get; set; }

    public decimal? RuningNo { get; set; }

    public string DocType { get; set; }

    public DateTime? Createdate { get; set; }

    public DateTime? UpdateDate { get; set; }
}
