
using Microsoft.AspNetCore.Identity;

namespace Capital_Item_Justification.Models
{
    public class ApplicationRole : IdentityRole
    {
        public bool IsActive { get; set; } = true;

        public string? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }
}
