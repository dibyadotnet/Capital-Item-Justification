using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class CIJJustificationViewModel
    {
        public int JustificationId { get; set; }
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

    }
}
