using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;

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
        public async Task<string> SaveCIJ(CIJMainViewModel cIJMainViewModel)
        {
            return await _cijMainRepository.SaveCIJ(cIJMainViewModel);
        }
        public async Task<List<DashboardViewModel>> GetDashboard()
        {
            return await _cijMainRepository.GetDashboard();
        }
        public async Task<List<CijLocation>> GetLocation()
        {
            return await _cijMainRepository.GetLocation();
        }
        public async Task<List<CijDepartment>> GetDepartment()
        {
            return await _cijMainRepository.GetDepartment();
        }
    }
}
