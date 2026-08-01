using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_Request")]
public partial class CijRequest
{
    [Key]
    [Column("CIJId")]
    public int Cijid { get; set; }

    [Column("CIJNumber")]
    [StringLength(30)]
    public string Cijnumber { get; set; } = null!;

    public DateOnly RequestDate { get; set; }

    public int? ProjectId { get; set; }

    public int HospitalLocationId { get; set; }

    public bool BudgetAvailable { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? BudgetAmount { get; set; }

    public int? BudgetTypeId { get; set; }

    public int RequestDepartmentId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalEquipmentCost { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? ProjectCost { get; set; }

    [Column("SCEHCost", TypeName = "decimal(18, 2)")]
    public decimal? Scehcost { get; set; }

    public string? Purpose { get; set; }

    [StringLength(100)]
    public string? OldEquipmentTreatment { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? OldEquipmentCost { get; set; }

    public int? BeneficiaryDepartmentId { get; set; }

    public int? BeneficiaryLocationId { get; set; }

    [StringLength(100)]
    public string? WaitingPeriod { get; set; }

    public int StatusId { get; set; }

    public int? CurrentWorkflowStepId { get; set; }

    public int CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("Cij")]
    public virtual ICollection<CijApprovalHistory> CijApprovalHistories { get; set; } = new List<CijApprovalHistory>();

    [InverseProperty("Cij")]
    public virtual ICollection<CijCommitteeComment> CijCommitteeComments { get; set; } = new List<CijCommitteeComment>();

    [InverseProperty("Cij")]
    public virtual ICollection<CijEquipment> CijEquipments { get; set; } = new List<CijEquipment>();

    [InverseProperty("Cij")]
    public virtual ICollection<CijJustification> CijJustifications { get; set; } = new List<CijJustification>();

    [InverseProperty("Cij")]
    public virtual ICollection<CijVendorQuotation> CijVendorQuotations { get; set; } = new List<CijVendorQuotation>();
}
