using System;
using System.Collections.Generic;

namespace eSignPRPO.Models;

public partial class TbCustomer
{
    public Guid UCusId { get; set; }

    public string SCusUsername { get; set; }

    public string SCusName { get; set; }

    public string SCusPassword { get; set; }

    public string SCusEmail { get; set; }

    public bool? BActive { get; set; }

    public DateTime? DCreated { get; set; }

    public string SCreatedBy { get; set; }

    public DateTime? DUpdated { get; set; }

    public string SUpdatedBy { get; set; }
}
