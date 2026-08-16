using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Data;

[Table("CIJ_WorkflowApproval")]
public partial class CijWorkflowApproval
{
    [Key]
    public long ApprovalId { get; set; }

    public long TransactionId { get; set; }

    [Column("CIJId")]
    public int Cijid { get; set; }

    [StringLength(100)]
    public string StepName { get; set; } = null!;

    [StringLength(100)]
    public string? ApproverRole { get; set; }

    public int? DepartmentId { get; set; }

    [StringLength(450)]
    public string? ApproverUserId { get; set; }

    public int StatusId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? AssignedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActionDate { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    [StringLength(450)]
    public string CreatedBy { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [StringLength(450)]
    public string? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }
}
