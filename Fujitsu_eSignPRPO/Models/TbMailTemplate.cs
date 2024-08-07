using System;
using System.Collections.Generic;

namespace eSignPRPO.Models;

public partial class TbMailTemplate
{
    public Guid UMId { get; set; }

    public string SSubject { get; set; }

    public string SBody { get; set; }

    public int? NType { get; set; }

    public DateTime? DCreated { get; set; }
}
