namespace Capital_Item_Justification.ViewModels
{
    public class UserListRequest
    {
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public int? departmentId { get; set; }
        public int? locationId { get; set; }
        public string? roleId { get; set; }
    }
}
