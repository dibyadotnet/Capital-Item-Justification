using Capital_Item_Justification.ViewModels;
using Capital_Item_Justification.Repository.Interfaces;

namespace Capital_Item_Justification.Services.Interfaces
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _repository;

        public LocationService(ILocationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<LocationViewModel>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<LocationViewModel?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> SaveAsync(LocationViewModel model, string userId)
        {
            if (model.LocationId == 0)
            {
                await _repository.AddAsync(model, userId);
            }
            else
            {
                await _repository.UpdateAsync(model, model.LocationId, userId);
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            await _repository.DeleteAsync(id, userId);
            return true;
        }
        public async Task<bool> LocationExistsAsync(string locationName, int? locationId)
        {
           return await _repository.LocationExistsAsync(locationName, locationId);
        }
    }
}
