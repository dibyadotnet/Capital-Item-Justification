using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Capital_Item_Justification.Models;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Data;

[Table("CIJ_RoleBudgetLimit")]
[Index("RoleId", Name = "UQ_RoleBudgetLimit_RoleId", IsUnique = true)]
public partial class CijRoleBudgetLimit
{
    [Key]
    public int RoleBudgetLimitId { get; set; }

    public string RoleId { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal BudgetLimit { get; set; }

    public bool IsActive { get; set; }

    [StringLength(450)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [StringLength(450)]
    public string? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }
    public int? ItemTypeId { get; set; }
    public virtual ApplicationRole? Role { get; set; }
}
