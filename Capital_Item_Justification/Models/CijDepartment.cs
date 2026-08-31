using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_Department")]
[Index("DepartmentCode", Name = "UQ__Departme__6EA8896DBB5ABB2E", IsUnique = true)]
public partial class CijDepartment
{
    [Key]
    public int DepartmentId { get; set; }

    [StringLength(100)]
    public string? DepartmentCode { get; set; } = null!;

    [StringLength(200)]
    public string DepartmentName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }
}
