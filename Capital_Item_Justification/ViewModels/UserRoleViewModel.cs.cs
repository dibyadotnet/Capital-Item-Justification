namespace Capital_Item_Justification.ViewModels
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<string> AssignedRoles { get; set; } = new();
        public List<string> AvailableRoles { get; set; } = new();
    }
}
