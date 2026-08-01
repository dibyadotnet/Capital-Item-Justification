using Microsoft.AspNetCore.Identity;

namespace Capital_Item_Justification.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
