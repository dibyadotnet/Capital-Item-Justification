using Microsoft.AspNetCore.Mvc.Rendering;

namespace Capital_Item_Justification.ViewModels
{
    public class RoleBudgetLimitViewModel
    {
        public int RoleBudgetLimitId { get; set; }

        public string RoleId { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public decimal BudgetLimit { get; set; }

        public bool IsActive { get; set; } = true;
        public int? ItemTypeId { get; set; }
        public string? ItemTypeName { get; set; }
        public IEnumerable<SelectListItem> ItemTypes { get; set; } = new List<SelectListItem>();
    }
}
