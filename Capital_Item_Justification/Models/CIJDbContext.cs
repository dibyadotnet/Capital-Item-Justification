using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Data;

namespace Capital_Item_Justification.Models;

public partial class CIJDbContext : IdentityDbContext
{
    public CIJDbContext()
    {
    }
    public CIJDbContext(DbContextOptions<CIJDbContext> options)
           : base(options)
    {
    }

    public virtual DbSet<CijApprovalHistory> CijApprovalHistories { get; set; }

    public virtual DbSet<CijAttachment> CijAttachments { get; set; }

    public virtual DbSet<CijCommitteeComment> CijCommitteeComments { get; set; }

    public virtual DbSet<CijDepartment> CijDepartments { get; set; }

    public virtual DbSet<CijEquipment> CijEquipments { get; set; }

    public virtual DbSet<CijItemType> CijItemTypes { get; set; }

    public virtual DbSet<CijItemTypeMaster> CijItemTypeMasters { get; set; }

    public virtual DbSet<CijJustification> CijJustifications { get; set; }

    public virtual DbSet<CijRequest> CijRequests { get; set; }

    public virtual DbSet<CijVendorQuotation> CijVendorQuotations { get; set; }

    public virtual DbSet<CijWorkflow> CijWorkflows { get; set; }

    public virtual DbSet<CijWorkflowStatus> CijWorkflowStatuses { get; set; }

    public virtual DbSet<CijWorkflowStep> CijWorkflowSteps { get; set; }

    public virtual DbSet<CijWorkflowTransaction> CijWorkflowTransactions { get; set; }
    public virtual DbSet<CijPurchasePurpose> CijPurchasePurposes { get; set; }

    public virtual DbSet<CijOldEquipmemtTreatment> CijOldEquipmemtTreatments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:CIJConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<CijApprovalHistory>(entity =>
        {
            entity.Property(e => e.ApprovedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Cij).WithMany(p => p.CijApprovalHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CIJ_ApprovalHistory_CIJ_Request");
        });

        modelBuilder.Entity<CijAttachment>(entity =>
        {
            entity.Property(e => e.AttachmentId).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UploadedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Cij).WithMany()
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CIJ_Attachment_CIJ_Request");
        });

        modelBuilder.Entity<CijCommitteeComment>(entity =>
        {
            entity.Property(e => e.CommentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Cij).WithMany(p => p.CijCommitteeComments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CIJ_CommitteeComment_CIJ_Request");
        });

        modelBuilder.Entity<CijDepartment>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BED83533B5A");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<CijEquipment>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Cij).WithMany(p => p.CijEquipments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CIJ_Equipment_CIJ_Request");
        });

        modelBuilder.Entity<CijItemType>(entity =>
        {
            entity.HasKey(e => e.ItemTypeId).HasName("PK__CIJ_Item__F51540FB576BFFD3");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<CijItemTypeMaster>(entity =>
        {
            entity.HasKey(e => e.ItemTypeId).HasName("PK__CIJ_Item__F51540FBE263A252");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<CijJustification>(entity =>
        {
            entity.HasKey(e => e.JustificationId).HasName("PK__CIJ_Just__AAE047E5AF2F0FEB");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsPurchasedEarlier).HasDefaultValue(false);
            entity.Property(e => e.Roirequired).HasDefaultValue(false);

            entity.HasOne(d => d.Cij).WithMany(p => p.CijJustifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CIJ_Justification_CIJ_Request");
        });

        modelBuilder.Entity<CijRequest>(entity =>
        {
            entity.HasKey(e => e.Cijid).HasName("PK__CIJ_Requ__A565D879748590FD");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OldEquipmentCost).HasDefaultValue(0m);
            entity.Property(e => e.TotalEquipmentCost).HasDefaultValue(0m);
        });

        modelBuilder.Entity<CijVendorQuotation>(entity =>
        {
            entity.HasKey(e => e.QuotationId).HasName("PK__CIJ_Vend__E19752936965F394");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsSelected).HasDefaultValue(false);

            entity.HasOne(d => d.Cij).WithMany(p => p.CijVendorQuotations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CIJ_VendorQuotation_CIJ_Request");
        });

        modelBuilder.Entity<CijWorkflow>(entity =>
        {
            entity.HasKey(e => e.WorkflowId).HasName("PK__CIJ_Work__5704A66A6F498BAA");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<CijWorkflowStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__CIJ_Work__C8EE206394E076A8");

            entity.Property(e => e.StatusId).ValueGeneratedNever();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsFinalStatus).HasDefaultValue(false);
        });

        modelBuilder.Entity<CijWorkflowStep>(entity =>
        {
            entity.HasKey(e => e.WorkflowStepId).HasName("PK__Workflow__36121461ED6EB169");

            entity.Property(e => e.CanApprove).HasDefaultValue(true);
            entity.Property(e => e.CanReject).HasDefaultValue(true);
            entity.Property(e => e.CanReturn).HasDefaultValue(true);
            entity.Property(e => e.CanSkip).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsFinalStep).HasDefaultValue(false);

            entity.HasOne(d => d.Workflow).WithMany(p => p.CijWorkflowSteps)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Workflow___Workf__797309D9");
        });

        modelBuilder.Entity<CijWorkflowTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__CIJ_Work__55433A6B84DD6B6D");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
        modelBuilder.Entity<CijPurchasePurpose>(entity =>
        {
            entity.HasKey(e => e.PurposeId).HasName("PK__CIJ_Purc__79E6A19479E3D91C");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
        modelBuilder.Entity<CijOldEquipmemtTreatment>(entity =>
        {
            entity.HasKey(e => e.TreatmentId).HasName("PK__CIJ_OldE__1A57B7F13526EBD5");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
