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

    public int? WorkflowId { get; set; }

    [StringLength(100)]
    public string? CurrentApproverRole { get; set; }//need to be discussed

    public int CurrentStatusId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? StartDate { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public bool? IsActive { get; set; }//2

    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CompletionDate { get; set; }

    [StringLength(200)]
    public string? CurrentStep { get; set; }
    public int? StepId { get; set; }
}
