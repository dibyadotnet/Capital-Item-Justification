using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Services.Interfaces
{
    public interface IDeptService
    {
        Task<List<DepartmentViewModel>> GetAllAsync();
        Task<DepartmentViewModel?> GetByIdAsync(int id);
        Task<bool> SaveAsync(DepartmentViewModel model, string userName);
        Task<bool> DeleteAsync(int id, string userName);
        Task<bool> DeptExistsAsync(string DeptName, int? deptId);
    }
}
