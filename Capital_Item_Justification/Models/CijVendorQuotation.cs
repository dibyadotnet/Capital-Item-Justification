using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_VendorQuotation")]
public partial class CijVendorQuotation
{
    [Key]
    public int QuotationId { get; set; }

    [Column("CIJId")]
    public int Cijid { get; set; }

    public int EquipmentId { get; set; }

    public int VendorId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal QuotedAmount { get; set; }

    public DateOnly? QuotationDate { get; set; }

    [StringLength(255)]
    public string? AttachmentFileName { get; set; }

    [StringLength(500)]
    public string? AttachmentPath { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    public bool? IsSelected { get; set; }

    public int? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("Cijid")]
    [InverseProperty("CijVendorQuotations")]
    public virtual CijRequest Cij { get; set; } = null!;
}
