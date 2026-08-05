using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class CIJRequestViewModel
    {
        public int Cijid { get; set; }
        public string CIJSNumber { get; set; } = null!;
        public DateOnly RequestDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public string? ProjectName{ get; set; }
        public int? CostCenterId { get; set; }
        public string? BudgetProvision { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? BudgetAmount { get; set; }
        public int? ItemtypeId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalEquipmentCost { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ProjectCost { get; set; }

        [Column("SCEHCost", TypeName = "decimal(18, 2)")]
        public decimal? Scehcost { get; set; }

        public int? PurchasePurposeId { get; set; }
        public int? OldEquipmentTreatmentId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? OldEquipmentCost { get; set; }

        public int? BeneficiaryDepartmentId { get; set; }

        public int? BeneficiaryLocationId { get; set; }

        [StringLength(100)]
        public string? WaitingPeriod { get; set; }

        public int StatusId { get; set; }

        public int? CurrentWorkflowStepId { get; set; }

        public List<SelectListItem> ItemTypes { get; set; } = new();
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Locations { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> BudgetProvisionList { get; set; } = new();
        public List<SelectListItem> PurchagePurposeList { get; set; } = new();
        public List<SelectListItem> OldEqupTreatmentList { get; set; } = new();
    }
}
