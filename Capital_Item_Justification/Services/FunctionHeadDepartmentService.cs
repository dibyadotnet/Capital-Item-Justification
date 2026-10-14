using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Services
{
    public class FunctionHeadDepartmentService:IFunctionHeadDepartmentService
    {
        private readonly IFunctionHeadDepartmentRepository _repository;

        public FunctionHeadDepartmentService(IFunctionHeadDepartmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<FunctionHeadDepartmentViewModel>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<FunctionHeadDepartmentViewModel?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
        public async Task<bool> SaveAsync(FunctionHeadDepartmentViewModel model, string userId)
        {
            if (model.Id == 0)
            {
                await _repository.AddAsync(model, userId);
            }
            else
            {
                await _repository.UpdateAsync(model, model.DepartmentId ?? 0, userId);
            }

            return true;
        }
        public async Task<bool> DeleteAsync(int id, string userId)
        {
            await _repository.DeleteAsync(id, userId);
            return true;
        }
        public async Task<bool> DeptExistsAsync(int? DeptId)
        {
            return await _repository.DeptExistsAsync(DeptId);
        }
    }
}
