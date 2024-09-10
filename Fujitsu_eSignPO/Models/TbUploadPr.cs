using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class TbUploadPr
{
    public decimal Fid { get; set; }

    public string Filename { get; set; }

    public string Prnumber { get; set; }

    public DateTime? Createdate { get; set; }
}
