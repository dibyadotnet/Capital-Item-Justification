using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Capital_Item_Justification.Services.Interfaces
{
    public interface ICIJRequestService
    {
        Task<List<CijItemType>> GetItemType();
        Task<List<CijPurchasePurpose>> GetPurchasePurpose();
        Task<List<CijOldEquipmemtTreatment>> GetOldEquipmentTreatment();
        Task<int> SaveCIJ(CIJMainViewModel cIJMainViewModel);
        Task<List<DashboardViewModel>> GetDashboard();
        Task<List<CijLocation>> GetLocation();
        Task<List<CijDepartment>> GetDepartment();
        Task<CIJMainViewModel> GetCIJById(int cijId);
        Task<string> UpdateCIJ(CIJMainViewModel cIJMainViewModel);
        Task<string> GenerateCIJNumber(int? locationId);
        Task<List<CijProject>> GetProjectCode();
        Task<List<AttachmentViewModel>> GetAttachmentsById(int cijId);
        Task<int> DeleteAttachment(int attachmentId);
        Task<List<CijCostCenter>> GetCostCenter();
        Task<List<CijBudgetType>> GetBudgetType();
    }
}
