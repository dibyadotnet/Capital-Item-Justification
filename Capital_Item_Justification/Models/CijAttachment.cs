using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Keyless]
[Table("CIJ_Attachment")]
public partial class CijAttachment
{
    [Key]
    public int AttachmentId { get; set; }

    [Column("CIJId")]
    public int Cijid { get; set; }

    public int DocumentTypeId { get; set; }

    [StringLength(255)]
    public string FileName { get; set; } = null!;

    [StringLength(500)]
    public string FilePath { get; set; } = null!;

    public int UploadedBy { get; set; }

    public DateTime UploadedDate { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public string ModifiedBy { get; set; } = string.Empty;

    public DateTime? ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    public string? ModuleName { get; set; }

    [ForeignKey("Cijid")]
    public virtual CijRequest Cij { get; set; } = null!;
}
