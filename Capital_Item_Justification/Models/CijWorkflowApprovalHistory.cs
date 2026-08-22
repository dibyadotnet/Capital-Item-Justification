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

    public long? TransactionId { get; set; }

    [Column("CIJId")]
    public int Cijid { get; set; }

    public int? FromStatusId { get; set; }

    public long? ApprovalId { get; set; }

    [StringLength(450)]
    public string? ApproverUserId { get; set; }

    [StringLength(100)]
    public string? ApproverRole { get; set; }

    public int? ToStatusId { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActionOn { get; set; }

    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedOn { get; set; }
    public int? StepId { get; set; }
}
