using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_WorkflowTransaction")]
public partial class CijWorkflowTransaction
{
    [Key]
    public long TransactionId { get; set; }

    [Column("CIJId")]
    public int Cijid { get; set; }

    public int WorkflowId { get; set; }

    public int StepId { get; set; }

    public int ApproverUserId { get; set; }

    public int ApproverRoleId { get; set; }

    public int StatusId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActionDate { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public int? PreviousStepId { get; set; }

    public int? NextStepId { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedOn { get; set; }

    public int? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }
}
