using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_Workflow")]
[Index("WorkflowCode", Name = "UQ__CIJ_Work__4A3F365EA5DF5731", IsUnique = true)]
public partial class CijWorkflow
{
    [Key]
    public int WorkflowId { get; set; }

    [StringLength(20)]
    public string WorkflowCode { get; set; } = null!;

    [StringLength(100)]
    public string WorkflowName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [InverseProperty("Workflow")]
    public virtual ICollection<CijWorkflowStep> CijWorkflowSteps { get; set; } = new List<CijWorkflowStep>();
}
