using System;
using System.Collections.Generic;

namespace Fujitsu_eSignPO.Models;

public partial class TbAttachment
{
    public Guid UAttachId { get; set; }

    public string SAttachName { get; set; }

    public string SAttachFileType { get; set; }

    public double? FAttachFileSize { get; set; }

    public DateTime? DCreated { get; set; }

    public Guid? UPrId { get; set; }

    public bool? BIsSendSupplier { get; set; }
}
