using System;
using System.Collections.Generic;
using Fujitsu_eSignPO.Models;
using Microsoft.EntityFrameworkCore;

namespace Fujitsu_eSignPO.Data;

public partial class FgdtESignPoContext : DbContext
{
    public FgdtESignPoContext()
    {
    }

    public FgdtESignPoContext(DbContextOptions<FgdtESignPoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TbAccountCode> TbAccountCodes { get; set; }

    public virtual DbSet<TbAttachment> TbAttachments { get; set; }

    public virtual DbSet<TbCompany> TbCompanies { get; set; }

    public virtual DbSet<TbCurrency> TbCurrencies { get; set; }

    public virtual DbSet<TbCustomer> TbCustomers { get; set; }

    public virtual DbSet<TbDepartment> TbDepartments { get; set; }

    public virtual DbSet<TbEmployee> TbEmployees { get; set; }

    public virtual DbSet<TbFlow> TbFlows { get; set; }

    public virtual DbSet<TbMailTemplate> TbMailTemplates { get; set; }

    public virtual DbSet<TbNormalCode> TbNormalCodes { get; set; }

    public virtual DbSet<TbPaymentCondition> TbPaymentConditions { get; set; }

    public virtual DbSet<TbPrRequest> TbPrRequests { get; set; }

    public virtual DbSet<TbPrRequestItem> TbPrRequestItems { get; set; }

    public virtual DbSet<TbPrReviewer> TbPrReviewers { get; set; }

    public virtual DbSet<TbTermAndCondition> TbTermAndConditions { get; set; }

    public virtual DbSet<TbVendor> TbVendors { get; set; }

    public virtual DbSet<TbWhLocation> TbWhLocations { get; set; }

    public virtual DbSet<VwInsertErp> VwInsertErps { get; set; }

    public virtual DbSet<VwPrReviewer> VwPrReviewers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbAccountCode>(entity =>
        {
            entity.HasKey(e => e.UAcGuid);

            entity.ToTable("tbAccountCode");

            entity.Property(e => e.UAcGuid)
                .ValueGeneratedNever()
                .HasColumnName("uAcGuid");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.DCreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("dCreatedDate");
            entity.Property(e => e.DUpdatedBy)
                .HasColumnType("datetime")
                .HasColumnName("dUpdatedBy");
            entity.Property(e => e.MainCode)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.SCreatedBy)
                .HasMaxLength(50)
                .HasColumnName("sCreatedBy");
            entity.Property(e => e.SUpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("sUpdatedBy");
            entity.Property(e => e.SubCode1)
                .IsRequired()
                .HasMaxLength(250);
            entity.Property(e => e.SubCode2)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<TbAttachment>(entity =>
        {
            entity.HasKey(e => e.UAttachId);

            entity.ToTable("TB_Attachment");

            entity.Property(e => e.UAttachId)
                .ValueGeneratedNever()
                .HasColumnName("uAttach_ID");
            entity.Property(e => e.BIsSendSupplier).HasColumnName("bIsSendSupplier");
            entity.Property(e => e.DCreated)
                .HasColumnType("datetime")
                .HasColumnName("dCreated");
            entity.Property(e => e.FAttachFileSize).HasColumnName("fAttach_File_Size");
            entity.Property(e => e.SAttachFileType)
                .HasMaxLength(100)
                .HasColumnName("sAttach_File_Type");
            entity.Property(e => e.SAttachName).HasColumnName("sAttach_Name");
            entity.Property(e => e.UPrId).HasColumnName("uPR_ID");
        });

        modelBuilder.Entity<TbCompany>(entity =>
        {
            entity.HasKey(e => e.CompanyCode);

            entity.ToTable("tbCompany");

            entity.Property(e => e.CompanyCode).HasMaxLength(10);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.AddressShipTo).HasMaxLength(500);
            entity.Property(e => e.CompanyCategory).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(500);
        });

        modelBuilder.Entity<TbCurrency>(entity =>
        {
            entity.HasKey(e => e.CurrencyCode);

            entity.ToTable("tbCurrency");

            entity.Property(e => e.CurrencyCode).HasMaxLength(50);
            entity.Property(e => e.CurrencyName).HasMaxLength(50);
        });

        modelBuilder.Entity<TbCustomer>(entity =>
        {
            entity.HasKey(e => e.UCusId);

            entity.ToTable("TB_Customers");

            entity.Property(e => e.UCusId)
                .ValueGeneratedNever()
                .HasColumnName("uCusID");
            entity.Property(e => e.BActive).HasColumnName("bActive");
            entity.Property(e => e.DCreated)
                .HasColumnType("datetime")
                .HasColumnName("dCreated");
            entity.Property(e => e.DUpdated)
                .HasColumnType("datetime")
                .HasColumnName("dUpdated");
            entity.Property(e => e.SCreatedBy)
                .HasMaxLength(50)
                .HasColumnName("sCreatedBy");
            entity.Property(e => e.SCusEmail).HasColumnName("sCusEmail");
            entity.Property(e => e.SCusName).HasColumnName("sCusName");
            entity.Property(e => e.SCusPassword)
                .HasMaxLength(50)
                .HasColumnName("sCusPassword");
            entity.Property(e => e.SCusUsername)
                .HasMaxLength(50)
                .HasColumnName("sCusUsername");
            entity.Property(e => e.SUpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("sUpdatedBy");
        });

        modelBuilder.Entity<TbDepartment>(entity =>
        {
            entity.HasKey(e => e.DepartmentCode);

            entity.ToTable("tbDepartment");

            entity.Property(e => e.DepartmentCode).HasMaxLength(50);
            entity.Property(e => e.DepartmentName).HasMaxLength(250);
        });

        modelBuilder.Entity<TbEmployee>(entity =>
        {
            entity.HasKey(e => e.NEmpId).HasName("PK_tb_emp");

            entity.ToTable("TB_Employees");

            entity.Property(e => e.NEmpId)
                .HasMaxLength(10)
                .HasColumnName("nEmpID");
            entity.Property(e => e.BActive).HasColumnName("bActive");
            entity.Property(e => e.DCreated)
                .HasColumnType("datetime")
                .HasColumnName("dCreated");
            entity.Property(e => e.DUpdated)
                .HasColumnType("datetime")
                .HasColumnName("dUpdated");
            entity.Property(e => e.Mobile).HasMaxLength(100);
            entity.Property(e => e.NPo).HasColumnName("nPO");
            entity.Property(e => e.NPositionLevel).HasColumnName("nPositionLevel");
            entity.Property(e => e.SCreatedBy)
                .HasMaxLength(50)
                .HasColumnName("sCreatedBy");
            entity.Property(e => e.SDepartment)
                .HasMaxLength(150)
                .HasColumnName("sDepartment");
            entity.Property(e => e.SEmpEmail)
                .HasMaxLength(250)
                .HasColumnName("sEmpEmail");
            entity.Property(e => e.SEmpName)
                .HasMaxLength(250)
                .HasColumnName("sEmpName");
            entity.Property(e => e.SEmpPassword)
                .HasMaxLength(100)
                .HasColumnName("sEmpPassword");
            entity.Property(e => e.SEmpTitle)
                .HasMaxLength(100)
                .HasColumnName("sEmpTitle");
            entity.Property(e => e.SEmpUsername)
                .HasMaxLength(100)
                .HasColumnName("sEmpUsername");
            entity.Property(e => e.SPosition)
                .HasMaxLength(150)
                .HasColumnName("sPosition");
            entity.Property(e => e.SSignature).HasColumnName("sSignature");
            entity.Property(e => e.SUpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("sUpdatedBy");
            entity.Property(e => e.Telephone).HasMaxLength(100);
        });

        modelBuilder.Entity<TbFlow>(entity =>
        {
            entity.HasKey(e => e.NFlowId);

            entity.ToTable("TB_Flow");

            entity.Property(e => e.NFlowId)
                .HasMaxLength(1)
                .HasColumnName("nFlowID");
            entity.Property(e => e.NPostionLevel)
                .HasMaxLength(1)
                .HasColumnName("nPostionLevel");
            entity.Property(e => e.SFlowName)
                .HasMaxLength(50)
                .HasColumnName("sFlowName");
            entity.Property(e => e.SNextStep)
                .HasMaxLength(50)
                .HasColumnName("sNextStep");
        });

        modelBuilder.Entity<TbMailTemplate>(entity =>
        {
            entity.HasKey(e => e.UMId);

            entity.ToTable("TB_Mail_Template");

            entity.Property(e => e.UMId)
                .ValueGeneratedNever()
                .HasColumnName("uM_ID");
            entity.Property(e => e.DCreated)
                .HasColumnType("datetime")
                .HasColumnName("dCreated");
            entity.Property(e => e.NType).HasColumnName("nType");
            entity.Property(e => e.SBody).HasColumnName("sBody");
            entity.Property(e => e.SSubject).HasColumnName("sSubject");
        });

        modelBuilder.Entity<TbNormalCode>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tbNormalCode");

            entity.Property(e => e.AccountName).HasMaxLength(50);
            entity.Property(e => e.MainCode).HasMaxLength(50);
            entity.Property(e => e.Section).HasMaxLength(50);
        });

        modelBuilder.Entity<TbPaymentCondition>(entity =>
        {
            entity.HasKey(e => e.TIfbp);

            entity.ToTable("tbPaymentCondition");

            entity.Property(e => e.TIfbp)
                .HasMaxLength(50)
                .HasColumnName("t_ifbp");
            entity.Property(e => e.TCpay)
                .HasMaxLength(50)
                .HasColumnName("t_cpay");
            entity.Property(e => e.TDsca)
                .HasMaxLength(250)
                .HasColumnName("t_dsca");
        });

        modelBuilder.Entity<TbPrRequest>(entity =>
        {
            entity.HasKey(e => e.UPoId);

            entity.ToTable("TB_PR_Request");

            entity.Property(e => e.UPoId)
                .ValueGeneratedNever()
                .HasColumnName("uPO_ID");
            entity.Property(e => e.DAcceptIvoiceDate)
                .HasColumnType("datetime")
                .HasColumnName("dAcceptIvoiceDate");
            entity.Property(e => e.DCreated)
                .HasColumnType("datetime")
                .HasColumnName("dCreated");
            entity.Property(e => e.DDeliveryDate)
                .HasColumnType("datetime")
                .HasColumnName("dDeliveryDate");
            entity.Property(e => e.DPoDate)
                .HasColumnType("datetime")
                .HasColumnName("dPo_Date");
            entity.Property(e => e.DShippingDate)
                .HasColumnType("datetime")
                .HasColumnName("dShippingDate");
            entity.Property(e => e.DUpdated)
                .HasColumnType("datetime")
                .HasColumnName("dUpdated");
            entity.Property(e => e.FBalance).HasColumnName("fBalance");
            entity.Property(e => e.FBudget).HasColumnName("fBudget");
            entity.Property(e => e.FRate).HasColumnName("fRate");
            entity.Property(e => e.FSumAmtCurrency).HasColumnName("fSum_Amt_Currency");
            entity.Property(e => e.FSumAmtThb).HasColumnName("fSum_Amt_THB");
            entity.Property(e => e.NStatus).HasColumnName("nStatus");
            entity.Property(e => e.SCreatedBy)
                .HasMaxLength(100)
                .HasColumnName("sCreated_By");
            entity.Property(e => e.SCreatedName)
                .HasMaxLength(100)
                .HasColumnName("sCreated_Name");
            entity.Property(e => e.SCurrency)
                .HasMaxLength(50)
                .HasColumnName("sCurrency");
            entity.Property(e => e.SDepartment)
                .HasMaxLength(50)
                .HasColumnName("sDepartment");
            entity.Property(e => e.SMainCode)
                .HasMaxLength(50)
                .HasColumnName("sMainCode");
            entity.Property(e => e.SPoNo)
                .HasMaxLength(20)
                .HasColumnName("sPO_No");
            entity.Property(e => e.SReason)
                .HasColumnType("text")
                .HasColumnName("sReason");
            entity.Property(e => e.SRefQuotation)
                .HasMaxLength(100)
                .HasColumnName("sRefQuotation");
            entity.Property(e => e.SSubCode1)
                .HasMaxLength(50)
                .HasColumnName("sSubCode1");
            entity.Property(e => e.SSubCode2)
                .HasMaxLength(50)
                .HasColumnName("sSubCode2");
            entity.Property(e => e.SVatType)
                .HasMaxLength(50)
                .HasColumnName("sVatType");
            entity.Property(e => e.SVendorCode)
                .HasMaxLength(10)
                .HasColumnName("sVendor_Code");
            entity.Property(e => e.SVendorName)
                .HasMaxLength(250)
                .HasColumnName("sVendor_Name");
        });

        modelBuilder.Entity<TbPrRequestItem>(entity =>
        {
            entity.HasKey(e => e.UPrItemId);

            entity.ToTable("TB_PR_Request_Item");

            entity.Property(e => e.UPrItemId)
                .ValueGeneratedNever()
                .HasColumnName("uPR_Item_ID");
            entity.Property(e => e.DCreated)
                .HasColumnType("datetime")
                .HasColumnName("dCreated");
            entity.Property(e => e.FAmount).HasColumnName("fAmount");
            entity.Property(e => e.FUnitPrice).HasColumnName("fUnitPrice");
            entity.Property(e => e.NNo).HasColumnName("nNo");
            entity.Property(e => e.NQty).HasColumnName("nQty");
            entity.Property(e => e.NStatus).HasColumnName("nStatus");
            entity.Property(e => e.SPartName)
                .HasMaxLength(150)
                .HasColumnName("sPartName");
            entity.Property(e => e.SPartNo)
                .HasMaxLength(100)
                .HasColumnName("sPartNo");
            entity.Property(e => e.SPoNo)
                .HasMaxLength(20)
                .HasColumnName("sPO_No");
            entity.Property(e => e.SVatType)
                .HasMaxLength(50)
                .HasColumnName("sVatType");
        });

        modelBuilder.Entity<TbPrReviewer>(entity =>
        {
            entity.HasKey(e => e.URwId);

            entity.ToTable("TB_PR_Reviewers");

            entity.Property(e => e.URwId)
                .ValueGeneratedNever()
                .HasColumnName("uRw_ID");
            entity.Property(e => e.BIsReject).HasColumnName("bIsReject");
            entity.Property(e => e.DCreated)
                .HasColumnType("datetime")
                .HasColumnName("dCreated");
            entity.Property(e => e.DRwApproveDate)
                .HasColumnType("datetime")
                .HasColumnName("dRW_Approve_Date");
            entity.Property(e => e.NRwStatus).HasColumnName("nRW_Status");
            entity.Property(e => e.NRwSteps).HasColumnName("nRW_Steps");
            entity.Property(e => e.SPoNo)
                .HasMaxLength(20)
                .HasColumnName("sPO_No");
            entity.Property(e => e.SRwApproveDepartment)
                .HasMaxLength(100)
                .HasColumnName("sRw_Approve_Department");
            entity.Property(e => e.SRwApproveId)
                .HasMaxLength(100)
                .HasColumnName("sRw_Approve_ID");
            entity.Property(e => e.SRwApproveName)
                .HasMaxLength(100)
                .HasColumnName("sRw_Approve_Name");
            entity.Property(e => e.SRwApproveTitle)
                .HasMaxLength(100)
                .HasColumnName("sRW_Approve_Title");
            entity.Property(e => e.SRwRemark).HasColumnName("sRw_Remark");
        });

        modelBuilder.Entity<TbTermAndCondition>(entity =>
        {
            entity.HasKey(e => e.TOtbp);

            entity.ToTable("tbTermAndCondition");

            entity.Property(e => e.TOtbp)
                .HasMaxLength(50)
                .HasColumnName("t_otbp");
            entity.Property(e => e.TDsca)
                .HasMaxLength(250)
                .HasColumnName("t_dsca");
        });

        modelBuilder.Entity<TbVendor>(entity =>
        {
            entity.HasKey(e => e.VendorCode);

            entity.ToTable("tbVendor");

            entity.Property(e => e.VendorCode).HasMaxLength(50);
            entity.Property(e => e.VendorName).HasMaxLength(500);
        });

        modelBuilder.Entity<TbWhLocation>(entity =>
        {
            entity.HasKey(e => new { e.Location, e.Category }).HasName("PK_tbWH");

            entity.ToTable("tbWH_Location");

            entity.Property(e => e.Location).HasMaxLength(50);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Wh)
                .HasMaxLength(50)
                .HasColumnName("WH");
        });

        modelBuilder.Entity<VwInsertErp>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_Insert_ERP");

            entity.Property(e => e.DConvertToPo)
                .HasColumnType("datetime")
                .HasColumnName("dConvertToPO");
            entity.Property(e => e.DCreated)
                .HasColumnType("datetime")
                .HasColumnName("dCreated");
            entity.Property(e => e.FSumAmtThb).HasColumnName("fSum_Amt_THB");
            entity.Property(e => e.NStatus).HasColumnName("nStatus");
            entity.Property(e => e.SCapexNo)
                .HasMaxLength(100)
                .HasColumnName("sCapex_No");
            entity.Property(e => e.SCreatedBy)
                .HasMaxLength(100)
                .HasColumnName("sCreated_By");
            entity.Property(e => e.SCurrency)
                .HasMaxLength(50)
                .HasColumnName("sCurrency");
            entity.Property(e => e.SPoNo)
                .HasMaxLength(20)
                .HasColumnName("sPO_No");
            entity.Property(e => e.SProduct)
                .HasMaxLength(20)
                .HasColumnName("sProduct");
            entity.Property(e => e.SReason)
                .HasColumnType("text")
                .HasColumnName("sReason");
            entity.Property(e => e.SSupplierCode)
                .HasMaxLength(10)
                .HasColumnName("sSupplier_Code");
            entity.Property(e => e.SWh)
                .HasMaxLength(50)
                .HasColumnName("sWH");
        });

        modelBuilder.Entity<VwPrReviewer>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_PR_Reviewers");

            entity.Property(e => e.DCreated)
                .HasColumnType("datetime")
                .HasColumnName("dCreated");
            entity.Property(e => e.DRwApproveDate)
                .HasColumnType("datetime")
                .HasColumnName("dRW_Approve_Date");
            entity.Property(e => e.DRwCreated)
                .HasColumnType("datetime")
                .HasColumnName("dRw_Created");
            entity.Property(e => e.FSumAmtCurrency).HasColumnName("fSum_Amt_Currency");
            entity.Property(e => e.FSumAmtThb).HasColumnName("fSum_Amt_THB");
            entity.Property(e => e.NRwStatus).HasColumnName("nRW_Status");
            entity.Property(e => e.NRwSteps).HasColumnName("nRW_Steps");
            entity.Property(e => e.NStatus).HasColumnName("nStatus");
            entity.Property(e => e.SCreatedBy)
                .HasMaxLength(100)
                .HasColumnName("sCreated_By");
            entity.Property(e => e.SCreatedName)
                .HasMaxLength(100)
                .HasColumnName("sCreated_Name");
            entity.Property(e => e.SDepartment)
                .HasMaxLength(50)
                .HasColumnName("sDepartment");
            entity.Property(e => e.SPoNo)
                .HasMaxLength(20)
                .HasColumnName("sPO_No");
            entity.Property(e => e.SRwApproveDepartment)
                .HasMaxLength(100)
                .HasColumnName("sRw_Approve_Department");
            entity.Property(e => e.SRwApproveId)
                .HasMaxLength(100)
                .HasColumnName("sRw_Approve_ID");
            entity.Property(e => e.SRwApproveName)
                .HasMaxLength(100)
                .HasColumnName("sRw_Approve_Name");
            entity.Property(e => e.SRwApproveTitle)
                .HasMaxLength(100)
                .HasColumnName("sRW_Approve_Title");
            entity.Property(e => e.SRwRemark).HasColumnName("sRw_Remark");
            entity.Property(e => e.URwId).HasColumnName("uRw_ID");
            entity.Property(e => e.VendorName).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
