using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Data;

[Table("CIJ_WorkflowClarification")]
public partial class CijWorkflowClarification
{
    [Key]
    public long ClarificationId { get; set; }

    public int Cijid { get; set; }

    public long TransactionId { get; set; }

    public long ApprovalId { get; set; }

    public int StepId { get; set; }

    [StringLength(450)]
    public string RaisedBy { get; set; } = null!;

    [StringLength(200)]
    public string? RaisedByRole { get; set; }

    public string ClarificationPoint { get; set; } = null!;

    public DateTime RaisedOn { get; set; }

    [StringLength(450)]
    public string? AnsweredBy { get; set; }

    [StringLength(450)]
    public string? AnsweredByRole { get; set; }

    public string? Answer { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? AnsweredOn { get; set; }

    public int StatusId { get; set; }

    [StringLength(450)]
    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    [StringLength(450)]
    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string? TargetRoleId {  get; set; }
    public string? TargetRoleName { get; set; }
}
