using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class ClJVendorViewModel
    {
        public int QuotationId { get; set; }
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
    }
}
