using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Services
{
    public class ProjectService:IProjectService
    {
        private readonly IProjectRepository _repository;

        public ProjectService(IProjectRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProjectViewModel>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<ProjectViewModel?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
        public async Task<bool> SaveAsync(ProjectViewModel model, string userId)
        {
            if (model.ProjectId == 0)
            {
                await _repository.AddAsync(model, userId);
            }
            else
            {
                await _repository.UpdateAsync(model, model.ProjectId, userId);
            }

            return true;
        }
        public async Task<bool> DeleteAsync(int id, string userId)
        {
            await _repository.DeleteAsync(id, userId);
            return true;
        }
        public async Task<bool> ProjectExistsAsync(string ProjectCode, int? ProjectId)
        {
            return await _repository.ProjectExistsAsync(ProjectCode, ProjectId);
        }
    }
}
