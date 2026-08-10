using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
        public async Task<int> SaveCIJ(CIJMainViewModel cIJMainViewModel)
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
        public async Task<CIJMainViewModel> GetCIJById(int cijId)
        {
            return await _cijMainRepository.GetCIJById(cijId);
        }
        public async Task<string> UpdateCIJ(CIJMainViewModel cIJMainViewModel)
        {
            return await _cijMainRepository.UpdateCIJ(cIJMainViewModel);
        }
        public async Task<string> GenerateCIJNumber(int? locationId)
        {
            return await _cijMainRepository.GenerateCIJNumber(locationId);
        }
        public async Task<List<CijProject>> GetProjectCode()
        {
            return await _cijMainRepository.GetProjectCode();
        }
        public async Task<List<AttachmentViewModel>> GetAttachmentsById(int cijId)
        {
            return await _cijMainRepository.GetAttachmentsById(cijId);
        }
        public async Task<int> DeleteAttachment(int attachmentId)
        {
            return await _cijMainRepository.DeleteAttachment(attachmentId);
        }
        public async Task<List<CijCostCenter>> GetCostCenter()
        {
            return await _cijMainRepository.GetCostCenter();
        }
        public async Task<List<CijBudgetType>> GetBudgetType()
        {
            return await _cijMainRepository.GetBudgetType();
        }
    }
}
