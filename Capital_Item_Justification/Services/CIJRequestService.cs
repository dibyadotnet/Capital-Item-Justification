using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Services.Interfaces;

namespace Capital_Item_Justification.Services
{
    public class CIJRequestService : ICIJRequestService
    {
        private readonly ICIJMainRepository _cijMainRepository;
        public CIJRequestService(ICIJMainRepository cijMainRepository)
        {
            _cijMainRepository = cijMainRepository;
        }
        public async Task<List<CijItemType>> GetItemType()
        {
            return await _cijMainRepository.GetItemType();
        }
        public async Task<List<CijPurchasePurpose>> GetPurchasePurpose()
        {
            return await _cijMainRepository.GetPurchasePurpose();
        }
        public async Task<List<CijOldEquipmemtTreatment>> GetOldEquipmentTreatment()
        {
            return await _cijMainRepository.GetOldEquipmentTreatment();
        }
    }
}
