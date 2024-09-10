using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class TbFlow
{
    public string NFlowId { get; set; }

    public string SFlowName { get; set; }

    public string SNextStep { get; set; }

    public string NPostionLevel { get; set; }
}
