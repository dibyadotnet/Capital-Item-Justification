using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        public async Task<string> SaveCIJ(CIJMainViewModel model)
        {
            if (model == null)
                return string.Empty;

            // Save Request

            var request = new CijRequest
            {
                Cijnumber = model?.CIJRequest?.CIJSNumber ?? "S-01",
                ProjectName = model?.CIJRequest?.ProjectName,
                CostCenterId = model?.CIJRequest?.CostCenterId,
                BudgetAvailable = model.CIJRequest.BudgetProvision,
                BudgetAmount = model.CIJRequest.BudgetAmount,
                //HospitalLocationId = model.CIJRequest.BeneficiaryLocationId,
                ItemTypeId = model.CIJRequest.ItemtypeId,
                TotalEquipmentCost = model.CIJRequest.TotalEquipmentCost,
                RequestDate = model.CIJRequest.RequestDate,
                //RequestDepartmentId = model.CIJRequest.BeneficiaryDepartmentId,               
                ProjectCost = model.CIJRequest.ProjectCost,
                Scehcost = model.CIJRequest.Scehcost,
                PurchasePurposeId = model.CIJRequest.PurchasePurposeId,
                OldEquipmentTreatmentId = model.CIJRequest.OldEquipmentTreatmentId,
                OldEquipmentCost = model.CIJRequest.OldEquipmentCost,
                WaitingPeriod = model.CIJRequest.WaitingPeriod,
                StatusId = model.CIJRequest.StatusId,
                CurrentWorkflowStepId = model.CIJRequest.CurrentWorkflowStepId,
            };
            _context.CijRequests.Add(request);
            int row = await _context.SaveChangesAsync();

            return null;
        }

        public async Task<List<DashboardViewModel>> GetDashboard()
        {
            List<DashboardViewModel> dashboardViewModels = new();

            dashboardViewModels = await (
                       from r in _context.CijRequests
                       join it in _context.CijItemTypes
                       on r.ItemTypeId equals it.ItemTypeId into itemGroup
                       from it in itemGroup.DefaultIfEmpty()
                       select new DashboardViewModel
                       {
                           CIJId = r.Cijid,
                           CIJNumber = r.Cijnumber,
                           RequestDate = r.RequestDate,
                           ProjectName = r.ProjectName,
                           ItemType = it != null ? it.ItemTypeName : "",
                           //CostCenter = cc != null ? cc.CostCenterName : "",
                           TotalEquipmentCost = r.TotalEquipmentCost,
                           //Status = s != null ? s.StatusName : ""
                       }).AsNoTracking().ToListAsync();


            return dashboardViewModels;
        }
        public async Task<List<CijLocation>> GetLocation()
        {
            return await _context.CijLocations.Where(a => a.IsActive == true).Select(a => new CijLocation()
            {
                LocationId = a.LocationId,
                LocationName = a.LocationName
            }).ToListAsync();
        }
        public async Task<List<CijDepartment>> GetDepartment()
        {
            return await _context.CijDepartments.Where(a => a.IsActive == true).Select(a => new CijDepartment()
            {
                DepartmentId = a.DepartmentId,
                DepartmentName = a.DepartmentName
            }).ToListAsync();
        }
    }

}
