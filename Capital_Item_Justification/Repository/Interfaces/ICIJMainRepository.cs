using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Repository.Interfaces
{
    public interface ICIJMainRepository
    {
        Task<List<CijItemType>> GetItemType();
        Task<List<CijPurchasePurpose>> GetPurchasePurpose();
        Task<List<CijOldEquipmemtTreatment>> GetOldEquipmentTreatment();
        Task<string> SaveCIJ(CIJMainViewModel cIJMainViewModel);
        Task<List<DashboardViewModel>> GetDashboard();
        Task<List<CijLocation>> GetLocation();
        Task<List<CijDepartment>> GetDepartment();
        Task<CIJMainViewModel> GetCIJById(int cijId);
        Task<string> UpdateCIJ(CIJMainViewModel cIJMainViewModel);
    }
}
