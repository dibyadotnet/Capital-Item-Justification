using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Services.Interfaces
{
    public interface ILocationService
    {
        Task<List<LocationViewModel>> GetAllAsync();
        Task<LocationViewModel?> GetByIdAsync(int id);
        Task<bool> SaveAsync(LocationViewModel model, string userName);
        Task<bool> DeleteAsync(int id, string userName);
        Task<bool> LocationExistsAsync(string locationName, int? locationId);
    }
}
