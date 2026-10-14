using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Services.Interfaces
{
    public interface IFunctionHeadDepartmentService
    {
        Task<List<FunctionHeadDepartmentViewModel>> GetAllAsync();
        Task<FunctionHeadDepartmentViewModel?> GetByIdAsync(int id);
        Task<bool> SaveAsync(FunctionHeadDepartmentViewModel model, string userName);
        Task<bool> DeleteAsync(int id, string userName);
        Task<bool> DeptExistsAsync(int? deptId);
    }
}
