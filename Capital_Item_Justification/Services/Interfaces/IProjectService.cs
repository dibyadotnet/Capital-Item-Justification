using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Services.Interfaces
{
    public interface IProjectService
    {
        Task<List<ProjectViewModel>> GetAllAsync();
        Task<ProjectViewModel?> GetByIdAsync(int id);
        Task<bool> SaveAsync(ProjectViewModel model, string userName);
        Task<bool> DeleteAsync(int id, string userName);
        Task<bool> ProjectExistsAsync(string ProjectCode, int? ProjectId);
    }
}
