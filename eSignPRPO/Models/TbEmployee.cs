using System;
using System.Collections.Generic;

namespace eSignPRPO.Models;

public partial class TbEmployee
{
    public string NEmpId { get; set; }

    public string SEmpName { get; set; }

    public string SEmpEmail { get; set; }

    public string SEmpUsername { get; set; }

    public string SEmpPassword { get; set; }

    public string SEmpTitle { get; set; }

    public string SDepartment { get; set; }

    public string SPosition { get; set; }

    public int? NPositionLevel { get; set; }

    public int? NPo { get; set; }

    public bool? BActive { get; set; }

    public DateTime? DCreated { get; set; }

    public string SCreatedBy { get; set; }

    public string SSignature { get; set; }

    public string SUpdatedBy { get; set; }

    public DateTime? DUpdated { get; set; }

    public string Telephone { get; set; }

    public string Mobile { get; set; }
}
