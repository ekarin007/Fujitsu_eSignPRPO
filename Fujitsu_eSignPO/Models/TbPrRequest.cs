using System;
using System.Collections.Generic;

namespace eSignPRPO.Models;

public partial class TbPrRequest
{
    public Guid UPrId { get; set; }

    public string SPrNo { get; set; }

    public string SPoNo { get; set; }

    public string SDepartment { get; set; }

    public string SPostion { get; set; }

    public string SSupplierCode { get; set; }

    public string SSupplierName { get; set; }

    public string SProduct { get; set; }

    public string SWh { get; set; }

    public string SCategory { get; set; }

    public string SCapexNo { get; set; }

    public string STypeAsset { get; set; }

    public string SAssentName { get; set; }

    public string SAssetNo { get; set; }

    public int? NStatus { get; set; }

    public string SCurrency { get; set; }

    public double? FRate { get; set; }

    public double? FSumAmtCurrency { get; set; }

    public double? FSumAmtThb { get; set; }

    public string SReason { get; set; }

    public string SCreatedBy { get; set; }

    public string SCreatedName { get; set; }

    public DateTime? DCreated { get; set; }

    public DateTime? DUpdated { get; set; }

    public DateTime? DDeliveryDate { get; set; }

    public DateTime? DConvertToPo { get; set; }

    public bool? BIsVat { get; set; }
}
