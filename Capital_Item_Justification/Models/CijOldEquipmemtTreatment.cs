using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Data;

[Table("CIJ_OldEquipmemt_Treatment")]
[Index("TreatmentName", Name = "UQ__CIJ_OldE__F6BA318D52779840", IsUnique = true)]
public partial class CijOldEquipmemtTreatment
{
    [Key]
    public int TreatmentId { get; set; }

    [StringLength(200)]
    public string TreatmentName { get; set; } = null!;

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
