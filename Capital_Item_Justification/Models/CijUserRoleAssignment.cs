using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Data;

[Table("CIJ_UserRoleAssignments")]
public partial class CijUserRoleAssignment
{
    [Key]
    public int AssignmentId { get; set; }

    [StringLength(450)]
    public string UserId { get; set; } = null!;

    [StringLength(450)]
    public string RoleId { get; set; } = null!;

    public int? DepartmentId { get; set; }

    public int? LocationId { get; set; }

    [StringLength(30)]
    public string ScopeType { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    [StringLength(450)]
    public string CreatedBy { get; set; } = null!;

    public DateTime? ModifiedOn { get; set; }

    [StringLength(450)]
    public string? ModifiedBy { get; set; }
}
