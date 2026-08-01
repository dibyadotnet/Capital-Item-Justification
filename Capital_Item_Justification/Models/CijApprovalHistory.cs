using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_ApprovalHistory")]
public partial class CijApprovalHistory
{
    [Key]
    public int ApprovalHistoryId { get; set; }

    [Column("CIJId")]
    public int Cijid { get; set; }

    public int WorkflowStepId { get; set; }

    public int ApproverId { get; set; }

    public int ActionId { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public DateTime ApprovedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("Cijid")]
    [InverseProperty("CijApprovalHistories")]
    public virtual CijRequest Cij { get; set; } = null!;
}
