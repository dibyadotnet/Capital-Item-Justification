using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Repository.Interfaces
{
    public interface IFunctionHeadDepartmentRepository
    {
        Task<List<FunctionHeadDepartmentViewModel>> GetAllAsync();
        Task<FunctionHeadDepartmentViewModel?> GetByIdAsync(int id);
        Task AddAsync(FunctionHeadDepartmentViewModel entity, string userId);
        Task UpdateAsync(FunctionHeadDepartmentViewModel entity, int id, string userId);
        Task DeleteAsync(int id, string userId);
        Task<bool> DeptExistsAsync(int? DeptId);
    }
}
