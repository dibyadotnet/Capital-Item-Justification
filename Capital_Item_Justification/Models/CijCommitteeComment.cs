using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Models;

[Table("CIJ_CommitteeComment")]
public partial class CijCommitteeComment
{
    [Key]
    public int CommentId { get; set; }

    [Column("CIJId")]
    public int Cijid { get; set; }

    public string Comments { get; set; } = null!;

    public int CommentedBy { get; set; }

    public DateTime CommentDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("Cijid")]
    [InverseProperty("CijCommitteeComments")]
    public virtual CijRequest Cij { get; set; } = null!;
}
