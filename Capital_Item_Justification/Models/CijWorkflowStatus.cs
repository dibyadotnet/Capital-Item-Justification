using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_Workflow_Status")]
[Index("StatusCode", Name = "UQ__CIJ_Work__6A7B44FC96A6AF14", IsUnique = true)]
public partial class CijWorkflowStatus
{
    [Key]
    public int StatusId { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string StatusCode { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string StatusName { get; set; } = null!;

    public bool? IsFinalStatus { get; set; }

    public bool? IsActive { get; set; }
}
