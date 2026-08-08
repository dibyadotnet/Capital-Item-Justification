using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Data;

[Table("CIJ_Project")]
public partial class CijProject
{
    [Key]
    public int ProjectId { get; set; }

    [StringLength(50)]
    public string ProjectCode { get; set; } = null!;

    [StringLength(200)]
    public string ProjectName { get; set; } = null!;

    public bool IsActive { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [StringLength(100)]
    public string? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }
}
