using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Repository.Interfaces
{
    public interface ILocationRepository
    {
        Task<List<LocationViewModel>> GetAllAsync();
        Task<LocationViewModel?> GetByIdAsync(int id);
        Task AddAsync(LocationViewModel entity,string userId);
        Task UpdateAsync(LocationViewModel entity,int id, string userId);
        Task DeleteAsync(int id, string userId);
        Task<bool> LocationExistsAsync(string locationName, int? locationId);
    }
}
