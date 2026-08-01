using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_Justification")]
public partial class CijJustification
{
    [Key]
    public int JustificationId { get; set; }

    [Column("CIJId")]
    public int Cijid { get; set; }

    [Column("ROIRequired")]
    public bool? Roirequired { get; set; }

    [Column("ROINumber")]
    [StringLength(100)]
    public string? Roinumber { get; set; }

    public bool? IsPurchasedEarlier { get; set; }

    public int? JustificationTypeId { get; set; }

    public string? Justification { get; set; }

    public string? Remarks { get; set; }

    public int? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("Cijid")]
    [InverseProperty("CijJustifications")]
    public virtual CijRequest Cij { get; set; } = null!;
}
