using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_Item_Type")]
[Index("ItemTypeCode", Name = "UQ__CIJ_Item__6C3E523016AB92D3", IsUnique = true)]
public partial class CijItemType
{
    [Key]
    public int ItemTypeId { get; set; }

    [StringLength(20)]
    public string ItemTypeCode { get; set; } = null!;

    [StringLength(100)]
    public string ItemTypeName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }
}
