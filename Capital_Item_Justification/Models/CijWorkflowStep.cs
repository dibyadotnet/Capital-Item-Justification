using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Capital_Item_Justification.Models;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Data;

[Table("CIJ_WorkflowStep")]
public partial class CijWorkflowStep
{
    [Key]
    public int WorkflowStepId { get; set; }

    public int WorkflowId { get; set; }

    public int StepNo { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string StepCode { get; set; } = null!;

    [StringLength(150)]
    [Unicode(false)]
    public string StepName { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? RoleName { get; set; }

    public bool IsInitialStep { get; set; }

    public bool IsFinalStep { get; set; }

    public bool IsActive { get; set; }

    [StringLength(450)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [StringLength(450)]
    public string? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }
    [ForeignKey(nameof(WorkflowId))]
    public virtual CijWorkflow CijWorkflow { get; set; } = null!;
}
