using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Data;

[Table("CIJ_Location")]
public partial class CijLocation
{
    [Key]
    public int LocationId { get; set; }

    [StringLength(100)]
    public string LocationName { get; set; } = null!;

    [StringLength(10)]
    public string? Prefix { get; set; }

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
