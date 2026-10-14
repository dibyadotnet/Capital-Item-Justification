using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Data;

[Table("CIJ_FunctionHeadDepartment")]
[Index("DepartmentId", "RoleId", Name = "UQ_CIJ_FunctionHeadDepartment", IsUnique = true)]
public partial class CijFunctionHeadDepartment
{
    [Key]
    public int Id { get; set; }

    public int DepartmentId { get; set; }

    public string RoleId { get; set; } = null!;

    public bool IsActive { get; set; }

    [StringLength(50)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [StringLength(50)]
    public string? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }
}
