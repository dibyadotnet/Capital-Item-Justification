using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Repository
{
    public class LocationRepository : ILocationRepository
    {
        private readonly CIJDbContext _context;

        public LocationRepository(CIJDbContext context)
        {
            _context = context;
        }

        public async Task<List<LocationViewModel>> GetAllAsync()
        {
            return await _context.CijLocations.Where(x => x.IsActive).Select(x => new LocationViewModel()
            {
                LocationId = x.LocationId,
                LocationName = x.LocationName,
                Prefix = x.Prefix,
                IsActive = x.IsActive,
                LocationType = x.LocationType
            }).OrderBy(x => x.LocationName).ToListAsync();
        }

        public async Task<LocationViewModel?> GetByIdAsync(int id)
        {
            return await _context.CijLocations.Where(x => x.IsActive && x.LocationId == id).Select(x => new LocationViewModel()
            {
                LocationId = x.LocationId,
                LocationName = x.LocationName,
                Prefix = x.Prefix,
                IsActive = x.IsActive,
                LocationType=x.LocationType
            }).OrderBy(x => x.LocationName).FirstOrDefaultAsync();
        }

        public async Task AddAsync(LocationViewModel vm, string userId)
        {
            CijLocation entity = new();
            entity.LocationName = vm.LocationName;
            entity.Prefix = vm.Prefix;
            entity.LocationType= vm.LocationType;
            entity.IsActive = true;
            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            await _context.CijLocations.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LocationViewModel vm, int id, string userId)
        {
            var entity = _context.CijLocations.Where(x => x.IsActive && x.LocationId == id).FirstOrDefault();
            if (entity == null)
                throw new InvalidOperationException();

            entity.LocationName = vm.LocationName;
            entity.LocationType=vm.LocationType;
            entity.Prefix = vm.Prefix;
            entity.ModifiedBy = userId;
            entity.ModifiedDate = DateTime.Now;

            _context.CijLocations.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var entity = _context.CijLocations.Where(x => x.IsActive && x.LocationId == id).FirstOrDefault();
            if (entity == null)
                throw new InvalidOperationException();

            entity.IsActive = false;
            entity.ModifiedBy = userId;
            entity.ModifiedDate = DateTime.Now;

            _context.CijLocations.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> LocationExistsAsync(string locationName, int? locationId)
        {
            return await _context.CijLocations.AnyAsync(x =>
            x.LocationName.ToLower() == locationName.ToLower()
            && (!locationId.HasValue ||
                x.LocationId != locationId.Value));
        }
    }
}
