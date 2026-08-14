using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Repository.Interfaces
{
    public interface ILocationRepository
    {
        Task<List<LocationViewModel>> GetAllAsync();
        Task<LocationViewModel?> GetByIdAsync(int id);
        Task AddAsync(LocationViewModel entity);
        Task UpdateAsync(LocationViewModel entity,int id);
    }
}
