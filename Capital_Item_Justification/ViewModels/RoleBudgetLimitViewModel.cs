using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class RoleBudgetLimitViewModel
    {
        public int RoleBudgetLimitId { get; set; }

        public string RoleId { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Budget Limit is required.")]
        [Range(1, int.MaxValue,ErrorMessage = "Budget Limit must be greater than 0.")]
        public decimal BudgetLimit { get; set; }

        public bool IsActive { get; set; } = true;
        public int? ItemTypeId { get; set; }
        public string? ItemTypeName { get; set; }
        public IEnumerable<SelectListItem> ItemTypes { get; set; } = new List<SelectListItem>();
    }
}
