using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_Workflow_Step")]
public partial class CijWorkflowStep
{
    [Key]
    public int WorkflowStepId { get; set; }

    public int WorkflowId { get; set; }

    public int StepOrder { get; set; }

    [StringLength(200)]
    public string StepName { get; set; } = null!;

    public int? RoleId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string ApprovalType { get; set; } = null!;

    public int? ReturnStepId { get; set; }

    public bool? CanApprove { get; set; }

    public bool? CanReject { get; set; }

    public bool? CanReturn { get; set; }

    public bool? CanSkip { get; set; }

    public bool? IsFinalStep { get; set; }

    public bool? IsActive { get; set; }

    [ForeignKey("WorkflowId")]
    [InverseProperty("CijWorkflowSteps")]
    public virtual CijWorkflow Workflow { get; set; } = null!;
}
