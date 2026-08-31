using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Services
{
    public class DeptService:IDeptService
    {
        private readonly IDeptRepository _repository;

        public DeptService(IDeptRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DepartmentViewModel>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<DepartmentViewModel?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
        public async Task<bool> SaveAsync(DepartmentViewModel model, string userId)
        {
            if (model.DepartmentId == 0)
            {
                await _repository.AddAsync(model, userId);
            }
            else
            {
                await _repository.UpdateAsync(model, model.DepartmentId, userId);
            }

            return true;
        }
        public async Task<bool> DeleteAsync(int id, string userId)
        {
            await _repository.DeleteAsync(id, userId);
            return true;
        }
        public async Task<bool> DeptExistsAsync(string DeptName, int? DeptId)
        {
            return await _repository.DeptExistsAsync(DeptName, DeptId);
        }
    }
}
