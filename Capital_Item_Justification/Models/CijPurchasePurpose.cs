using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Data;

[Table("CIJ_Purchase_Purpose")]
[Index("PurposeName", Name = "UQ__CIJ_Purc__C44E9BC399F724CA", IsUnique = true)]
public partial class CijPurchasePurpose
{
    [Key]
    public int PurposeId { get; set; }

    [StringLength(200)]
    public string PurposeName { get; set; } = null!;

    public bool? IsActive { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedOn { get; set; }

    [StringLength(100)]
    public string? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }
}
