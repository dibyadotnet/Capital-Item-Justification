using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_ItemType_Master")]
[Index("ItemTypeCode", Name = "UQ__CIJ_Item__6C3E5230E86AEF38", IsUnique = true)]
public partial class CijItemTypeMaster
{
    [Key]
    public int ItemTypeId { get; set; }

    [StringLength(20)]
    public string ItemTypeCode { get; set; } = null!;

    [StringLength(100)]
    public string ItemTypeName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedOn { get; set; }

    public int? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }
}
