using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_Equipment")]
public partial class CijEquipment
{
    [Key]
    public int EquipmentId { get; set; }

    [Column("CIJId")]
    public int Cijid { get; set; }

    [StringLength(250)]
    public string EquipmentName { get; set; } = null!;

    [StringLength(200)]
    public string? Make { get; set; }

    [StringLength(200)]
    public string? Model { get; set; }

    public int? PreferenceOrder { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal EquipmentCost { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("Cijid")]
    [InverseProperty("CijEquipments")]
    public virtual CijRequest Cij { get; set; } = null!;

    public int Qty { get; set; }
}
