using System;
using System.Collections.Generic;

namespace eSignPRPO.Models;

public partial class TbCompany
{
    public string CompanyCode { get; set; }

    public string CompanyCategory { get; set; }

    public string Name { get; set; }

    public string Address { get; set; }

    public string AddressShipTo { get; set; }
}
