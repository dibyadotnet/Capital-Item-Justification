using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Repository.Interfaces
{
    public interface IProjectRepository
    {
        Task<List<ProjectViewModel>> GetAllAsync();
        Task<ProjectViewModel?> GetByIdAsync(int id);
        Task AddAsync(ProjectViewModel entity, string userId);
        Task UpdateAsync(ProjectViewModel entity, int id, string userId);
        Task DeleteAsync(int id, string userId);
        Task<bool> ProjectExistsAsync(string ProjectCode, int? ProjectId);
    }
}
