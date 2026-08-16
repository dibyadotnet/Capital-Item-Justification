using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Data;

[Table("CIJ_WorkflowApprovalHistory")]
public partial class CijWorkflowApprovalHistory
{
    [Key]
    public int WorkflowApprovalHistoryId { get; set; }

    public long TransactionId { get; set; }

    [Column("CIJId")]
    public int Cijid { get; set; }

    [StringLength(100)]
    public string StepName { get; set; } = null!;

    public int? DepartmentId { get; set; }

    [StringLength(450)]
    public string? ApproverUserId { get; set; }

    [StringLength(100)]
    public string? ApproverRole { get; set; }

    public int? StatusId { get; set; }

    [StringLength(1000)]
    public string? Comments { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActionOn { get; set; }

    public int? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedOn { get; set; }

    public int? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }
}
