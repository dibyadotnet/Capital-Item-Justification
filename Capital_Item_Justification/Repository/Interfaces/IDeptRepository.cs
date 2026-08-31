using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Repository.Interfaces
{
    public interface IDeptRepository
    {
        Task<List<DepartmentViewModel>> GetAllAsync();
        Task<DepartmentViewModel?> GetByIdAsync(int id);
        Task AddAsync(DepartmentViewModel entity, string userId);
        Task UpdateAsync(DepartmentViewModel entity, int id, string userId);
        Task DeleteAsync(int id, string userId);
        Task<bool> DeptExistsAsync(string DeptName, int? DeptId);
    }
}
