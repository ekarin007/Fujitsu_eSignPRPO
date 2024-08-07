using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using eSignPRPO.Models;

namespace eSignPRPO.Data;

public partial class ESignPrpoContext : DbContext
{
    public ESignPrpoContext()
    {
    }

    public ESignPrpoContext(DbContextOptions<ESignPrpoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TbAttachment> TbAttachments { get; set; }

    public virtual DbSet<TbCompany> TbCompanies { get; set; }

    public virtual DbSet<TbCostCenter> TbCostCenters { get; set; }

    public virtual DbSet<TbCurrency> TbCurrencies { get; set; }

    public virtual DbSet<TbCusSup> TbCusSups { get; set; }

    public virtual DbSet<TbCustomer> TbCustomers { get; set; }

    public virtual DbSet<TbEmployee> TbEmployees { get; set; }

    public virtual DbSet<TbFlow> TbFlows { get; set; }

    public virtual DbSet<TbGlnumber> TbGlnumbers { get; set; }

    public virtual DbSet<TbItem> TbItems { get; set; }

    public virtual DbSet<TbItemPrice> TbItemPrices { get; set; }

    public virtual DbSet<TbMailTemplate> TbMailTemplates { get; set; }

    public virtual DbSet<TbMicRuning> TbMicRunings { get; set; }

    public virtual DbSet<TbPaymentCondition> TbPaymentConditions { get; set; }

    public virtual DbSet<TbPrRequest> TbPrRequests { get; set; }

    public virtual DbSet<TbPrRequestItem> TbPrRequestItems { get; set; }

    public virtual DbSet<TbPrReviewer> TbPrReviewers { get; set; }

    public virtual DbSet<TbShipVium> TbShipVia { get; set; }

    public virtual DbSet<TbSupplierAddress> TbSupplierAddresses { get; set; }

    public virtual DbSet<TbTermAndCondition> TbTermAndConditions { get; set; }

    public virtual DbSet<TbWarehouse> TbWarehouses { get; set; }

    public virtual DbSet<TbWhLocation> TbWhLocations { get; set; }

    public virtual DbSet<Ttdpur400551> Ttdpur400551s { get; set; }

    public virtual DbSet<Ttdpur401551> Ttdpur401551s { get; set; }

    public virtual DbSet<VwInsertErp> VwInsertErps { get; set; }

    public virtual DbSet<VwPrReviewer> VwPrReviewers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { 
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        modelBuilder.Entity<TbCostCenter>(entity =>
        {
            entity.HasKey(e => new { e.TDtype, e.TDimx });

            entity.ToTable("tb_CostCenter");

            entity.Property(e => e.TDtype).HasColumnName("t_dtype");
            entity.Property(e => e.TDimx)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_dimx");
            entity.Property(e => e.TDesc)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("t_desc");
            entity.Property(e => e.TSkey)
                .IsRequired()
                .HasMaxLength(16)
                .IsUnicode(false)
                .HasColumnName("t_skey");
        });

        modelBuilder.Entity<TbCurrency>(entity =>
        {
            entity.HasKey(e => new { e.TBcur, e.TCcur, e.TRtyp, e.TStdt });

            entity.ToTable("tb_Currency");

            entity.Property(e => e.TBcur)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_bcur");
            entity.Property(e => e.TCcur)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_ccur");
            entity.Property(e => e.TRtyp)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_rtyp");
            entity.Property(e => e.TStdt)
                .HasColumnType("datetime")
                .HasColumnName("t_stdt");
            entity.Property(e => e.TRate).HasColumnName("t_rate");
        });

        modelBuilder.Entity<TbCusSup>(entity =>
        {
            entity.HasKey(e => e.TBpid);

            entity.ToTable("tb_cus_sup");

            entity.Property(e => e.TBpid)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_bpid");
            entity.Property(e => e.TCcur)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_ccur");
            entity.Property(e => e.TNama)
                .IsRequired()
                .HasMaxLength(35)
                .IsUnicode(false)
                .HasColumnName("t_nama");
            entity.Property(e => e.TSeak)
                .IsRequired()
                .HasMaxLength(16)
                .IsUnicode(false)
                .HasColumnName("t_seak");
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
        });

        modelBuilder.Entity<TbGlnumber>(entity =>
        {
            entity.HasKey(e => e.TLeac);

            entity.ToTable("tb_GLNumber");

            entity.Property(e => e.TLeac)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("t_leac");
            entity.Property(e => e.TDesc)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("t_desc");
            entity.Property(e => e.TSkey)
                .HasMaxLength(16)
                .IsUnicode(false)
                .HasColumnName("t_skey");
        });

        modelBuilder.Entity<TbItem>(entity =>
        {
            entity.HasKey(e => e.TItem);

            entity.ToTable("tb_item");

            entity.Property(e => e.TItem)
                .HasMaxLength(47)
                .IsUnicode(false)
                .HasColumnName("t_item");
            entity.Property(e => e.TCitg)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_citg");
            entity.Property(e => e.TDsca)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("t_dsca");
        });

        modelBuilder.Entity<TbItemPrice>(entity =>
        {
            entity.HasKey(e => e.TItem);

            entity.ToTable("tb_Item_Price");

            entity.Property(e => e.TItem)
                .HasMaxLength(47)
                .IsUnicode(false)
                .HasColumnName("t_item");
            entity.Property(e => e.TCcur)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("t_ccur");
            entity.Property(e => e.TCuqp)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("t_cuqp");
            entity.Property(e => e.TPrip).HasColumnName("t_prip");
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

        modelBuilder.Entity<TbMicRuning>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tb_mic_runing");

            entity.Property(e => e.Createdate)
                .HasColumnType("date")
                .HasColumnName("createdate");
            entity.Property(e => e.DocType)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("doc_type");
            entity.Property(e => e.Month)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("month");
            entity.Property(e => e.OuCode)
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("ou_code");
            entity.Property(e => e.RuningNo)
                .HasColumnType("numeric(18, 0)")
                .HasColumnName("runing_no");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("date")
                .HasColumnName("update_date");
            entity.Property(e => e.Year)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("year");
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
            entity.HasKey(e => e.UPrId);

            entity.ToTable("TB_PR_Request");

            entity.Property(e => e.UPrId)
                .ValueGeneratedNever()
                .HasColumnName("uPR_ID");
            entity.Property(e => e.BIsVat).HasColumnName("bIsVat");
            entity.Property(e => e.DConvertToPo)
                .HasColumnType("datetime")
                .HasColumnName("dConvertToPO");
            entity.Property(e => e.DCreated)
                .HasColumnType("datetime")
                .HasColumnName("dCreated");
            entity.Property(e => e.DDeliveryDate)
                .HasColumnType("datetime")
                .HasColumnName("dDeliveryDate");
            entity.Property(e => e.DUpdated)
                .HasColumnType("datetime")
                .HasColumnName("dUpdated");
            entity.Property(e => e.FRate).HasColumnName("fRate");
            entity.Property(e => e.FSumAmtCurrency).HasColumnName("fSum_Amt_Currency");
            entity.Property(e => e.FSumAmtThb).HasColumnName("fSum_Amt_THB");
            entity.Property(e => e.NStatus).HasColumnName("nStatus");
            entity.Property(e => e.SAssentName)
                .HasMaxLength(100)
                .HasColumnName("sAssent_Name");
            entity.Property(e => e.SAssetNo)
                .HasMaxLength(100)
                .HasColumnName("sAsset_No");
            entity.Property(e => e.SCapexNo)
                .HasMaxLength(100)
                .HasColumnName("sCapex_No");
            entity.Property(e => e.SCategory)
                .HasMaxLength(50)
                .HasColumnName("sCategory");
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
            entity.Property(e => e.SPoNo)
                .HasMaxLength(20)
                .HasColumnName("sPO_No");
            entity.Property(e => e.SPostion)
                .HasMaxLength(50)
                .HasColumnName("sPostion");
            entity.Property(e => e.SPrNo)
                .HasMaxLength(20)
                .HasColumnName("sPR_No");
            entity.Property(e => e.SProduct)
                .HasMaxLength(20)
                .HasColumnName("sProduct");
            entity.Property(e => e.SReason)
                .HasColumnType("text")
                .HasColumnName("sReason");
            entity.Property(e => e.SSupplierCode)
                .HasMaxLength(10)
                .HasColumnName("sSupplier_Code");
            entity.Property(e => e.SSupplierName)
                .HasMaxLength(250)
                .HasColumnName("sSupplier_Name");
            entity.Property(e => e.STypeAsset)
                .HasMaxLength(10)
                .HasColumnName("sType_Asset");
            entity.Property(e => e.SWh)
                .HasMaxLength(50)
                .HasColumnName("sWH");
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
            entity.Property(e => e.DRequestDate)
                .HasColumnType("date")
                .HasColumnName("dRequest_Date");
            entity.Property(e => e.FAmount).HasColumnName("fAmount");
            entity.Property(e => e.FUnitCost).HasColumnName("fUnit_Cost");
            entity.Property(e => e.NNo).HasColumnName("nNo");
            entity.Property(e => e.NQty).HasColumnName("nQty");
            entity.Property(e => e.NStatus).HasColumnName("nStatus");
            entity.Property(e => e.SCostCenter)
                .HasMaxLength(150)
                .HasColumnName("sCost_Center");
            entity.Property(e => e.SCurrency)
                .HasMaxLength(50)
                .HasColumnName("sCurrency");
            entity.Property(e => e.SGlCode)
                .HasMaxLength(150)
                .HasColumnName("sGL_Code");
            entity.Property(e => e.SItem)
                .HasMaxLength(100)
                .HasColumnName("sItem");
            entity.Property(e => e.SItemDesc)
                .HasMaxLength(150)
                .HasColumnName("sItem_Desc");
            entity.Property(e => e.SPrNo)
                .HasMaxLength(20)
                .HasColumnName("sPR_No");
            entity.Property(e => e.SUom)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("sUom");
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
            entity.Property(e => e.SPrNo)
                .HasMaxLength(20)
                .HasColumnName("sPR_No");
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

        modelBuilder.Entity<TbShipVium>(entity =>
        {
            entity.HasKey(e => e.TSfbp);

            entity.ToTable("tbShipVia");

            entity.Property(e => e.TSfbp)
                .HasMaxLength(50)
                .HasColumnName("t_sfbp");
            entity.Property(e => e.TDsca)
                .HasMaxLength(250)
                .HasColumnName("t_dsca");
        });

        modelBuilder.Entity<TbSupplierAddress>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tbSupplierAddress");

            entity.Property(e => e.TBpid)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("t_bpid");
            entity.Property(e => e.TInet)
                .HasMaxLength(250)
                .HasColumnName("t_inet");
            entity.Property(e => e.TInfo)
                .HasMaxLength(250)
                .HasColumnName("t_info");
            entity.Property(e => e.TLn01)
                .HasMaxLength(250)
                .HasColumnName("t_ln01");
            entity.Property(e => e.TLn02)
                .HasMaxLength(250)
                .HasColumnName("t_ln02");
            entity.Property(e => e.TLn03)
                .HasMaxLength(250)
                .HasColumnName("t_ln03");
            entity.Property(e => e.TLn04)
                .HasMaxLength(250)
                .HasColumnName("t_ln04");
            entity.Property(e => e.TLn05)
                .HasMaxLength(250)
                .HasColumnName("t_ln05");
            entity.Property(e => e.TLn06)
                .HasMaxLength(250)
                .HasColumnName("t_ln06");
            entity.Property(e => e.TNama)
                .HasMaxLength(250)
                .HasColumnName("t_nama");
            entity.Property(e => e.TNamc)
                .HasMaxLength(250)
                .HasColumnName("t_namc");
            entity.Property(e => e.TTelp)
                .HasMaxLength(250)
                .HasColumnName("t_telp");
            entity.Property(e => e.TTelx)
                .HasMaxLength(250)
                .HasColumnName("t_telx");
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

        modelBuilder.Entity<TbWarehouse>(entity =>
        {
            entity.HasKey(e => e.TCwar);

            entity.ToTable("tb_Warehouse");

            entity.Property(e => e.TCwar)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_cwar");
            entity.Property(e => e.TDsca)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("t_dsca");
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

        modelBuilder.Entity<Ttdpur400551>(entity =>
        {
            entity.HasKey(e => e.TOrno).HasName("Ittdpur400551_1a");

            entity.ToTable("ttdpur400551");

            entity.Property(e => e.TOrno)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_orno");
            entity.Property(e => e.TAkcd)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("t_akcd");
            entity.Property(e => e.TBpcl)
                .IsRequired()
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("t_bpcl");
            entity.Property(e => e.TBppr)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_bppr");
            entity.Property(e => e.TBptx)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_bptx");
            entity.Property(e => e.TCadr)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_cadr");
            entity.Property(e => e.TCbrn)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_cbrn");
            entity.Property(e => e.TCcon)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_ccon");
            entity.Property(e => e.TCcrs)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_ccrs");
            entity.Property(e => e.TCcty)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_ccty");
            entity.Property(e => e.TCcur)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_ccur");
            entity.Property(e => e.TCdec)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdec");
            entity.Property(e => e.TCfrw)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cfrw");
            entity.Property(e => e.TCofc)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_cofc");
            entity.Property(e => e.TComm).HasColumnName("t_comm");
            entity.Property(e => e.TCorg).HasColumnName("t_corg");
            entity.Property(e => e.TCosn)
                .IsRequired()
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("t_cosn");
            entity.Property(e => e.TCotp)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cotp");
            entity.Property(e => e.TCpay)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cpay");
            entity.Property(e => e.TCplp)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cplp");
            entity.Property(e => e.TCrcd)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_crcd");
            entity.Property(e => e.TCreg)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_creg");
            entity.Property(e => e.TCtcd)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_ctcd");
            entity.Property(e => e.TCtrj)
                .IsRequired()
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("t_ctrj");
            entity.Property(e => e.TCvyn).HasColumnName("t_cvyn");
            entity.Property(e => e.TCwar)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_cwar");
            entity.Property(e => e.TDdat)
                .HasColumnType("datetime")
                .HasColumnName("t_ddat");
            entity.Property(e => e.TDdtc)
                .HasColumnType("datetime")
                .HasColumnName("t_ddtc");
            entity.Property(e => e.TEgen).HasColumnName("t_egen");
            entity.Property(e => e.TFdpt)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_fdpt");
            entity.Property(e => e.THdst).HasColumnName("t_hdst");
            entity.Property(e => e.THism).HasColumnName("t_hism");
            entity.Property(e => e.THisp).HasColumnName("t_hisp");
            entity.Property(e => e.TIafc).HasColumnName("t_iafc");
            entity.Property(e => e.TIebp).HasColumnName("t_iebp");
            entity.Property(e => e.TIfad)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_ifad");
            entity.Property(e => e.TIfbp)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_ifbp");
            entity.Property(e => e.TIfcn)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_ifcn");
            entity.Property(e => e.TMcfr).HasColumnName("t_mcfr");
            entity.Property(e => e.TOamt).HasColumnName("t_oamt");
            entity.Property(e => e.TOdat)
                .HasColumnType("datetime")
                .HasColumnName("t_odat");
            entity.Property(e => e.TOdis).HasColumnName("t_odis");
            entity.Property(e => e.TOdno)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_odno");
            entity.Property(e => e.TOdty).HasColumnName("t_odty");
            entity.Property(e => e.TOtad)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_otad");
            entity.Property(e => e.TOtbp)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_otbp");
            entity.Property(e => e.TOtcn)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_otcn");
            entity.Property(e => e.TPaft).HasColumnName("t_paft");
            entity.Property(e => e.TPlnr)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_plnr");
            entity.Property(e => e.TPrno)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_prno");
            entity.Property(e => e.TPtad)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_ptad");
            entity.Property(e => e.TPtbp)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_ptbp");
            entity.Property(e => e.TPtcn)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_ptcn");
            entity.Property(e => e.TPtpa)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_ptpa");
            entity.Property(e => e.TRagr)
                .IsRequired()
                .HasMaxLength(16)
                .IsUnicode(false)
                .HasColumnName("t_ragr");
            entity.Property(e => e.TRatd)
                .HasColumnType("datetime")
                .HasColumnName("t_ratd");
            entity.Property(e => e.TRatf1).HasColumnName("t_ratf_1");
            entity.Property(e => e.TRatf2).HasColumnName("t_ratf_2");
            entity.Property(e => e.TRatf3).HasColumnName("t_ratf_3");
            entity.Property(e => e.TRatp1).HasColumnName("t_ratp_1");
            entity.Property(e => e.TRatp2).HasColumnName("t_ratp_2");
            entity.Property(e => e.TRatp3).HasColumnName("t_ratp_3");
            entity.Property(e => e.TRatt)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_ratt");
            entity.Property(e => e.TRaur).HasColumnName("t_raur");
            entity.Property(e => e.TRefa)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("t_refa");
            entity.Property(e => e.TRefb)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("t_refb");
            entity.Property(e => e.TRefcntd).HasColumnName("t_Refcntd");
            entity.Property(e => e.TRefcntu).HasColumnName("t_Refcntu");
            entity.Property(e => e.TRetr)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_retr");
            entity.Property(e => e.TSbim).HasColumnName("t_sbim");
            entity.Property(e => e.TSbmt)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_sbmt");
            entity.Property(e => e.TSfad)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_sfad");
            entity.Property(e => e.TSfbp)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_sfbp");
            entity.Property(e => e.TSfcn)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_sfcn");
            entity.Property(e => e.TSorn)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("t_sorn");
            entity.Property(e => e.TTxta).HasColumnName("t_txta");
            entity.Property(e => e.TTxtb).HasColumnName("t_txtb");
        });

        modelBuilder.Entity<Ttdpur401551>(entity =>
        {
            entity.HasKey(e => new { e.TOrno, e.TPono, e.TSqnb }).HasName("Ittdpur401551_1a");

            entity.ToTable("ttdpur401551");

            entity.Property(e => e.TOrno)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_orno");
            entity.Property(e => e.TPono).HasColumnName("t_pono");
            entity.Property(e => e.TSqnb).HasColumnName("t_sqnb");
            entity.Property(e => e.TActi)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("t_acti");
            entity.Property(e => e.TAkcd)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("t_akcd");
            entity.Property(e => e.TAmld).HasColumnName("t_amld");
            entity.Property(e => e.TAmod).HasColumnName("t_amod");
            entity.Property(e => e.TAppr).HasColumnName("t_appr");
            entity.Property(e => e.TBpcl)
                .IsRequired()
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("t_bpcl");
            entity.Property(e => e.TBptc)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_bptc");
            entity.Property(e => e.TBtsp).HasColumnName("t_btsp");
            entity.Property(e => e.TBtx1).HasColumnName("t_btx1");
            entity.Property(e => e.TBtx2).HasColumnName("t_btx2");
            entity.Property(e => e.TCact)
                .IsRequired()
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("t_cact");
            entity.Property(e => e.TCadr)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_cadr");
            entity.Property(e => e.TCasi)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_casi");
            entity.Property(e => e.TCcco)
                .IsRequired()
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("t_ccco");
            entity.Property(e => e.TCcof)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_ccof");
            entity.Property(e => e.TCcty)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_ccty");
            entity.Property(e => e.TCdec)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdec");
            entity.Property(e => e.TCdis1)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdis_1");
            entity.Property(e => e.TCdis10)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdis_10");
            entity.Property(e => e.TCdis11)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdis_11");
            entity.Property(e => e.TCdis2)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdis_2");
            entity.Property(e => e.TCdis3)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdis_3");
            entity.Property(e => e.TCdis4)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdis_4");
            entity.Property(e => e.TCdis5)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdis_5");
            entity.Property(e => e.TCdis6)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdis_6");
            entity.Property(e => e.TCdis7)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdis_7");
            entity.Property(e => e.TCdis8)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdis_8");
            entity.Property(e => e.TCdis9)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cdis_9");
            entity.Property(e => e.TCeno)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("t_ceno");
            entity.Property(e => e.TCfrw)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cfrw");
            entity.Property(e => e.TCitt)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_citt");
            entity.Property(e => e.TClot)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("t_clot");
            entity.Property(e => e.TClyn).HasColumnName("t_clyn");
            entity.Property(e => e.TCmnf)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_cmnf");
            entity.Property(e => e.TCnig).HasColumnName("t_cnig");
            entity.Property(e => e.TComm).HasColumnName("t_comm");
            entity.Property(e => e.TCono)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_cono");
            entity.Property(e => e.TCoop1).HasColumnName("t_coop_1");
            entity.Property(e => e.TCoop2).HasColumnName("t_coop_2");
            entity.Property(e => e.TCoop3).HasColumnName("t_coop_3");
            entity.Property(e => e.TCopr1).HasColumnName("t_copr_1");
            entity.Property(e => e.TCopr2).HasColumnName("t_copr_2");
            entity.Property(e => e.TCopr3).HasColumnName("t_copr_3");
            entity.Property(e => e.TCorg).HasColumnName("t_corg");
            entity.Property(e => e.TCosn)
                .IsRequired()
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("t_cosn");
            entity.Property(e => e.TCpay)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cpay");
            entity.Property(e => e.TCpcl)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_cpcl");
            entity.Property(e => e.TCpcp)
                .IsRequired()
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("t_cpcp");
            entity.Property(e => e.TCpln)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_cpln");
            entity.Property(e => e.TCpon).HasColumnName("t_cpon");
            entity.Property(e => e.TCprj)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_cprj");
            entity.Property(e => e.TCpva).HasColumnName("t_cpva");
            entity.Property(e => e.TCrbn).HasColumnName("t_crbn");
            entity.Property(e => e.TCrcd)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_crcd");
            entity.Property(e => e.TCrej)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_crej");
            entity.Property(e => e.TCrit)
                .IsRequired()
                .HasMaxLength(35)
                .IsUnicode(false)
                .HasColumnName("t_crit");
            entity.Property(e => e.TCrrf).HasColumnName("t_crrf");
            entity.Property(e => e.TCsgp)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_csgp");
            entity.Property(e => e.TCspa)
                .IsRequired()
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("t_cspa");
            entity.Property(e => e.TCstl)
                .IsRequired()
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("t_cstl");
            entity.Property(e => e.TCtcd)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_ctcd");
            entity.Property(e => e.TCtrj)
                .IsRequired()
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("t_ctrj");
            entity.Property(e => e.TCubp)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cubp");
            entity.Property(e => e.TCupp)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cupp");
            entity.Property(e => e.TCuqp)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_cuqp");
            entity.Property(e => e.TCuva).HasColumnName("t_cuva");
            entity.Property(e => e.TCvat)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_cvat");
            entity.Property(e => e.TCvbp).HasColumnName("t_cvbp");
            entity.Property(e => e.TCvpp).HasColumnName("t_cvpp");
            entity.Property(e => e.TCvqp).HasColumnName("t_cvqp");
            entity.Property(e => e.TCwar)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_cwar");
            entity.Property(e => e.TDamt).HasColumnName("t_damt");
            entity.Property(e => e.TDdon).HasColumnName("t_ddon");
            entity.Property(e => e.TDdta)
                .HasColumnType("datetime")
                .HasColumnName("t_ddta");
            entity.Property(e => e.TDdtb)
                .HasColumnType("datetime")
                .HasColumnName("t_ddtb");
            entity.Property(e => e.TDdtc)
                .HasColumnType("datetime")
                .HasColumnName("t_ddtc");
            entity.Property(e => e.TDdtd)
                .HasColumnType("datetime")
                .HasColumnName("t_ddtd");
            entity.Property(e => e.TDdte)
                .HasColumnType("datetime")
                .HasColumnName("t_ddte");
            entity.Property(e => e.TDdtf)
                .HasColumnType("datetime")
                .HasColumnName("t_ddtf");
            entity.Property(e => e.TDino)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("t_dino");
            entity.Property(e => e.TDisc1).HasColumnName("t_disc_1");
            entity.Property(e => e.TDisc10).HasColumnName("t_disc_10");
            entity.Property(e => e.TDisc11).HasColumnName("t_disc_11");
            entity.Property(e => e.TDisc2).HasColumnName("t_disc_2");
            entity.Property(e => e.TDisc3).HasColumnName("t_disc_3");
            entity.Property(e => e.TDisc4).HasColumnName("t_disc_4");
            entity.Property(e => e.TDisc5).HasColumnName("t_disc_5");
            entity.Property(e => e.TDisc6).HasColumnName("t_disc_6");
            entity.Property(e => e.TDisc7).HasColumnName("t_disc_7");
            entity.Property(e => e.TDisc8).HasColumnName("t_disc_8");
            entity.Property(e => e.TDisc9).HasColumnName("t_disc_9");
            entity.Property(e => e.TDmde1)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_dmde_1");
            entity.Property(e => e.TDmde10)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_dmde_10");
            entity.Property(e => e.TDmde11)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_dmde_11");
            entity.Property(e => e.TDmde2)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_dmde_2");
            entity.Property(e => e.TDmde3)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_dmde_3");
            entity.Property(e => e.TDmde4)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_dmde_4");
            entity.Property(e => e.TDmde5)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_dmde_5");
            entity.Property(e => e.TDmde6)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_dmde_6");
            entity.Property(e => e.TDmde7)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_dmde_7");
            entity.Property(e => e.TDmde8)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_dmde_8");
            entity.Property(e => e.TDmde9)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_dmde_9");
            entity.Property(e => e.TDmse1).HasColumnName("t_dmse_1");
            entity.Property(e => e.TDmse10).HasColumnName("t_dmse_10");
            entity.Property(e => e.TDmse11).HasColumnName("t_dmse_11");
            entity.Property(e => e.TDmse2).HasColumnName("t_dmse_2");
            entity.Property(e => e.TDmse3).HasColumnName("t_dmse_3");
            entity.Property(e => e.TDmse4).HasColumnName("t_dmse_4");
            entity.Property(e => e.TDmse5).HasColumnName("t_dmse_5");
            entity.Property(e => e.TDmse6).HasColumnName("t_dmse_6");
            entity.Property(e => e.TDmse7).HasColumnName("t_dmse_7");
            entity.Property(e => e.TDmse8).HasColumnName("t_dmse_8");
            entity.Property(e => e.TDmse9).HasColumnName("t_dmse_9");
            entity.Property(e => e.TDmth1).HasColumnName("t_dmth_1");
            entity.Property(e => e.TDmth10).HasColumnName("t_dmth_10");
            entity.Property(e => e.TDmth11).HasColumnName("t_dmth_11");
            entity.Property(e => e.TDmth2).HasColumnName("t_dmth_2");
            entity.Property(e => e.TDmth3).HasColumnName("t_dmth_3");
            entity.Property(e => e.TDmth4).HasColumnName("t_dmth_4");
            entity.Property(e => e.TDmth5).HasColumnName("t_dmth_5");
            entity.Property(e => e.TDmth6).HasColumnName("t_dmth_6");
            entity.Property(e => e.TDmth7).HasColumnName("t_dmth_7");
            entity.Property(e => e.TDmth8).HasColumnName("t_dmth_8");
            entity.Property(e => e.TDmth9).HasColumnName("t_dmth_9");
            entity.Property(e => e.TDmty1).HasColumnName("t_dmty_1");
            entity.Property(e => e.TDmty10).HasColumnName("t_dmty_10");
            entity.Property(e => e.TDmty11).HasColumnName("t_dmty_11");
            entity.Property(e => e.TDmty2).HasColumnName("t_dmty_2");
            entity.Property(e => e.TDmty3).HasColumnName("t_dmty_3");
            entity.Property(e => e.TDmty4).HasColumnName("t_dmty_4");
            entity.Property(e => e.TDmty5).HasColumnName("t_dmty_5");
            entity.Property(e => e.TDmty6).HasColumnName("t_dmty_6");
            entity.Property(e => e.TDmty7).HasColumnName("t_dmty_7");
            entity.Property(e => e.TDmty8).HasColumnName("t_dmty_8");
            entity.Property(e => e.TDmty9).HasColumnName("t_dmty_9");
            entity.Property(e => e.TDorg1).HasColumnName("t_dorg_1");
            entity.Property(e => e.TDorg10).HasColumnName("t_dorg_10");
            entity.Property(e => e.TDorg11).HasColumnName("t_dorg_11");
            entity.Property(e => e.TDorg2).HasColumnName("t_dorg_2");
            entity.Property(e => e.TDorg3).HasColumnName("t_dorg_3");
            entity.Property(e => e.TDorg4).HasColumnName("t_dorg_4");
            entity.Property(e => e.TDorg5).HasColumnName("t_dorg_5");
            entity.Property(e => e.TDorg6).HasColumnName("t_dorg_6");
            entity.Property(e => e.TDorg7).HasColumnName("t_dorg_7");
            entity.Property(e => e.TDorg8).HasColumnName("t_dorg_8");
            entity.Property(e => e.TDorg9).HasColumnName("t_dorg_9");
            entity.Property(e => e.TDtrm).HasColumnName("t_dtrm");
            entity.Property(e => e.TEffn).HasColumnName("t_effn");
            entity.Property(e => e.TElgb).HasColumnName("t_elgb");
            entity.Property(e => e.TExmt).HasColumnName("t_exmt");
            entity.Property(e => e.TFire).HasColumnName("t_fire");
            entity.Property(e => e.TGefo).HasColumnName("t_gefo");
            entity.Property(e => e.TGlco)
                .IsRequired()
                .HasMaxLength(62)
                .IsUnicode(false)
                .HasColumnName("t_glco");
            entity.Property(e => e.TIamt).HasColumnName("t_iamt");
            entity.Property(e => e.TInvd)
                .HasColumnType("datetime")
                .HasColumnName("t_invd");
            entity.Property(e => e.TInvn)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_invn");
            entity.Property(e => e.TItem)
                .IsRequired()
                .HasMaxLength(47)
                .IsUnicode(false)
                .HasColumnName("t_item");
            entity.Property(e => e.TLdam1).HasColumnName("t_ldam_1");
            entity.Property(e => e.TLdam10).HasColumnName("t_ldam_10");
            entity.Property(e => e.TLdam11).HasColumnName("t_ldam_11");
            entity.Property(e => e.TLdam2).HasColumnName("t_ldam_2");
            entity.Property(e => e.TLdam3).HasColumnName("t_ldam_3");
            entity.Property(e => e.TLdam4).HasColumnName("t_ldam_4");
            entity.Property(e => e.TLdam5).HasColumnName("t_ldam_5");
            entity.Property(e => e.TLdam6).HasColumnName("t_ldam_6");
            entity.Property(e => e.TLdam7).HasColumnName("t_ldam_7");
            entity.Property(e => e.TLdam8).HasColumnName("t_ldam_8");
            entity.Property(e => e.TLdam9).HasColumnName("t_ldam_9");
            entity.Property(e => e.TLdat)
                .HasColumnType("datetime")
                .HasColumnName("t_ldat");
            entity.Property(e => e.TLeng).HasColumnName("t_leng");
            entity.Property(e => e.TLsel).HasColumnName("t_lsel");
            entity.Property(e => e.TLseq).HasColumnName("t_lseq");
            entity.Property(e => e.TMitm)
                .IsRequired()
                .HasMaxLength(35)
                .IsUnicode(false)
                .HasColumnName("t_mitm");
            entity.Property(e => e.TMpnr)
                .IsRequired()
                .HasMaxLength(35)
                .IsUnicode(false)
                .HasColumnName("t_mpnr");
            entity.Property(e => e.TMpsn)
                .IsRequired()
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasColumnName("t_mpsn");
            entity.Property(e => e.TOamt).HasColumnName("t_oamt");
            entity.Property(e => e.TOdat)
                .HasColumnType("datetime")
                .HasColumnName("t_odat");
            entity.Property(e => e.TOltp).HasColumnName("t_oltp");
            entity.Property(e => e.TOtbp)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_otbp");
            entity.Property(e => e.TOwns).HasColumnName("t_owns");
            entity.Property(e => e.TPaft).HasColumnName("t_paft");
            entity.Property(e => e.TPaya)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_paya");
            entity.Property(e => e.TPmde)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_pmde");
            entity.Property(e => e.TPmnd).HasColumnName("t_pmnd");
            entity.Property(e => e.TPmni).HasColumnName("t_pmni");
            entity.Property(e => e.TPmnt).HasColumnName("t_pmnt");
            entity.Property(e => e.TPmse).HasColumnName("t_pmse");
            entity.Property(e => e.TPmsk)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("t_pmsk");
            entity.Property(e => e.TPorg).HasColumnName("t_porg");
            entity.Property(e => e.TPric).HasColumnName("t_pric");
            entity.Property(e => e.TPseq).HasColumnName("t_pseq");
            entity.Property(e => e.TPtpa)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_ptpa");
            entity.Property(e => e.TPtpe)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_ptpe");
            entity.Property(e => e.TQbbc).HasColumnName("t_qbbc");
            entity.Property(e => e.TQiap).HasColumnName("t_qiap");
            entity.Property(e => e.TQibo).HasColumnName("t_qibo");
            entity.Property(e => e.TQibp).HasColumnName("t_qibp");
            entity.Property(e => e.TQidl).HasColumnName("t_qidl");
            entity.Property(e => e.TQiiv).HasColumnName("t_qiiv");
            entity.Property(e => e.TQips).HasColumnName("t_qips");
            entity.Property(e => e.TQirj).HasColumnName("t_qirj");
            entity.Property(e => e.TQoor).HasColumnName("t_qoor");
            entity.Property(e => e.TQual).HasColumnName("t_qual");
            entity.Property(e => e.TRcno)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_rcno");
            entity.Property(e => e.TRcod)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_rcod");
            entity.Property(e => e.TRdta)
                .HasColumnType("datetime")
                .HasColumnName("t_rdta");
            entity.Property(e => e.TRefcntd).HasColumnName("t_Refcntd");
            entity.Property(e => e.TRefcntu).HasColumnName("t_Refcntu");
            entity.Property(e => e.TRevi)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_revi");
            entity.Property(e => e.TRseq).HasColumnName("t_rseq");
            entity.Property(e => e.TSbim).HasColumnName("t_sbim");
            entity.Property(e => e.TSbmt)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_sbmt");
            entity.Property(e => e.TSdsc).HasColumnName("t_sdsc");
            entity.Property(e => e.TServ)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("t_serv");
            entity.Property(e => e.TSfad)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_sfad");
            entity.Property(e => e.TSfbp)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_sfbp");
            entity.Property(e => e.TSfcn)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("t_sfcn");
            entity.Property(e => e.TSfwh)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("t_sfwh");
            entity.Property(e => e.TSpcn)
                .IsRequired()
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasColumnName("t_spcn");
            entity.Property(e => e.TStdc).HasColumnName("t_stdc");
            entity.Property(e => e.TStsc).HasColumnName("t_stsc");
            entity.Property(e => e.TStsd).HasColumnName("t_stsd");
            entity.Property(e => e.TSubc).HasColumnName("t_subc");
            entity.Property(e => e.TTaxp).HasColumnName("t_taxp");
            entity.Property(e => e.TThic).HasColumnName("t_thic");
            entity.Property(e => e.TTxta).HasColumnName("t_txta");
            entity.Property(e => e.TVryn).HasColumnName("t_vryn");
            entity.Property(e => e.TWidt).HasColumnName("t_widt");
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
            entity.Property(e => e.SPrNo)
                .HasMaxLength(20)
                .HasColumnName("sPR_No");
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
            entity.Property(e => e.SSupplierCode)
                .HasMaxLength(10)
                .HasColumnName("sSupplier_Code");
            entity.Property(e => e.SSupplierName)
                .HasMaxLength(250)
                .HasColumnName("sSupplier_Name");
            entity.Property(e => e.URwId).HasColumnName("uRw_ID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
