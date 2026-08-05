using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class CIJEquipmentViewModel
    {
        public int EquipmentId { get; set; }
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
        public int EquipmentQty { get; set; }
    }
}
