using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Data;

[Table("CIJ_EmailConfiguration")]
public partial class CijEmailConfiguration
{
    [Key]
    public int EmailConfigurationId { get; set; }

    [StringLength(250)]
    public string SmtpServer { get; set; } = null!;

    public int SmtpPort { get; set; }

    [StringLength(250)]
    public string SenderEmail { get; set; } = null!;

    [StringLength(250)]
    public string? Username { get; set; }

    [StringLength(500)]
    public string? Password { get; set; }

    public bool IsActive { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    [StringLength(100)]
    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
