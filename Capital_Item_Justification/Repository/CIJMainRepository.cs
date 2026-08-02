using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Repository
{
    public class CIJMainRepository : ICIJMainRepository
    {
        private readonly CIJDbContext _context;
        public CIJMainRepository(CIJDbContext context)
        {
            _context = context;
        }
        public async Task<List<CijItemType>> GetItemType()
        {
            var itemTypes = await _context.CijItemTypes.Where(a => a.IsActive == true).Select(a => new CijItemType()
            {
                ItemTypeId = a.ItemTypeId,
                ItemTypeCode = a.ItemTypeCode
            }).OrderBy(a => a.ItemTypeId).ToListAsync();
            return itemTypes;
        }
        public async Task<List<CijPurchasePurpose>> GetPurchasePurpose()
        {
            var purchasePurposes = await _context.CijPurchasePurposes.Where(a => a.IsActive == true).Select(a => new CijPurchasePurpose()
            {
                PurposeId = a.PurposeId,
                PurposeName = a.PurposeName
            }).OrderBy(a => a.PurposeId).ToListAsync();

            return purchasePurposes;
        }
        public async Task<List<CijOldEquipmemtTreatment>> GetOldEquipmentTreatment()
        {
            var treatments = await _context.CijOldEquipmemtTreatments.Where(a => a.IsActive == true).Select(a => new CijOldEquipmemtTreatment()
            {
                TreatmentId = a.TreatmentId,
                TreatmentName = a.TreatmentName
            }).OrderBy(a => a.TreatmentId).ToListAsync();

            return treatments;
        }
    }
}
