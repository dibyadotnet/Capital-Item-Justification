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

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("Cijid")]
    public virtual CijRequest Cij { get; set; } = null!;
}
