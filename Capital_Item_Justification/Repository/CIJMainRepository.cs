using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            int cijId = request.Cijid;
            //Save Equipment
            if (model != null && model.Equipments != null && model.Equipments.Count > 0)
            {
                foreach (var item in model.Equipments)
                {
                    CijEquipment cijEquipment = new CijEquipment
                    {
                        Cijid = cijId,
                        EquipmentName = item.EquipmentName,
                        Qty = item.EquipmentQty,
                        Make = item.Make,
                        Model = item.Model,
                        EquipmentCost = item.EquipmentCost,
                        PreferenceOrder = item.PreferenceOrder
                    };
                    _context.CijEquipments.Add(cijEquipment);
                }
                _context.SaveChanges();
            }
            //Save Justification/ Committee Comment
            if (model != null && model.Justification != null)
            {
                CijJustification cijJustification = new CijJustification
                {
                    Cijid = cijId,
                    Roinumber = model.Justification.Roinumber,
                    IsPurchasedEarlier = model.Justification.IsPurchasedEarlier,
                    Justification = model.Justification.Justification,
                    Remarks = model.Justification.Remarks,
                };
                _context.CijJustifications.Add(cijJustification);

                _context.SaveChanges();
            }
            if (model != null && model.CommitteeComment != null)
            {
                CijCommitteeComment cijCommittee = new CijCommitteeComment
                {
                    Cijid = cijId,
                    CommentDate = DateTime.Now,
                    Comments = model.CommitteeComment.Comments
                };
                _context.CijCommitteeComments.Add(cijCommittee);

                _context.SaveChanges();
            }

            return null;
        }
        public async Task<string> UpdateCIJ(CIJMainViewModel model)
        {
            if (model == null)
                return string.Empty;

            var request = await _context.CijRequests
                .FirstOrDefaultAsync(x => x.Cijid == model.CIJRequest.Cijid);

            if (request == null)
                return "CIJ record not found.";

            // Update Request
            request.Cijnumber = model.CIJRequest.CIJSNumber;
            request.ProjectName = model.CIJRequest.ProjectName;
            request.CostCenterId = model.CIJRequest.CostCenterId;
            request.BudgetAvailable = model.CIJRequest.BudgetProvision;
            request.BudgetAmount = model.CIJRequest.BudgetAmount;
            request.ItemTypeId = model.CIJRequest.ItemtypeId;
            request.TotalEquipmentCost = model.CIJRequest.TotalEquipmentCost;
            request.RequestDate = model.CIJRequest.RequestDate;
            request.ProjectCost = model.CIJRequest.ProjectCost;
            request.Scehcost = model.CIJRequest.Scehcost;
            request.PurchasePurposeId = model.CIJRequest.PurchasePurposeId;
            request.OldEquipmentTreatmentId = model.CIJRequest.OldEquipmentTreatmentId;
            request.OldEquipmentCost = model.CIJRequest.OldEquipmentCost;
            request.WaitingPeriod = model.CIJRequest.WaitingPeriod;
            request.StatusId = model.CIJRequest.StatusId;
            request.CurrentWorkflowStepId = model.CIJRequest.CurrentWorkflowStepId;

            await _context.SaveChangesAsync();

            int cijId = request.Cijid;

            //Update Equipment

            var existingEquipments = await _context.CijEquipments
                .Where(x => x.Cijid == cijId)
                .ToListAsync();

            _context.CijEquipments.RemoveRange(existingEquipments);

            if (model.Equipments != null && model.Equipments.Any())
            {
                foreach (var item in model.Equipments)
                {
                    _context.CijEquipments.Add(new CijEquipment
                    {
                        Cijid = cijId,
                        EquipmentName = item.EquipmentName,
                        Qty = item.EquipmentQty,
                        Make = item.Make,
                        Model = item.Model,
                        EquipmentCost = item.EquipmentCost,
                        PreferenceOrder = item.PreferenceOrder
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Update Justification

            var justification = await _context.CijJustifications
                .FirstOrDefaultAsync(x => x.Cijid == cijId);

            if (justification == null)
            {
                justification = new CijJustification
                {
                    Cijid = cijId
                };

                _context.CijJustifications.Add(justification);
            }

            justification.Roinumber = model.Justification.Roinumber;
            justification.IsPurchasedEarlier = model.Justification.IsPurchasedEarlier;
            justification.Justification = model.Justification.Justification;
            justification.Remarks = model.Justification.Remarks;

            await _context.SaveChangesAsync();

            //Update Committee Comment

            var committeeComment = await _context.CijCommitteeComments
                .FirstOrDefaultAsync(x => x.Cijid == cijId);

            if (committeeComment == null)
            {
                committeeComment = new CijCommitteeComment
                {
                    Cijid = cijId
                };

                _context.CijCommitteeComments.Add(committeeComment);
            }

            committeeComment.CommentDate = DateTime.Now;
            committeeComment.Comments = model.CommitteeComment.Comments;

            await _context.SaveChangesAsync();

            return "Success";
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

        public async Task<CIJMainViewModel> GetCIJById(int cijId)
        {
            CIJMainViewModel model = new CIJMainViewModel();
            var request = await _context.CijRequests.FirstOrDefaultAsync(x => x.Cijid == cijId);

            if (request == null)
            {
                return null;
            }
            model.CIJRequest = new CIJRequestViewModel
            {
                Cijid = request.Cijid,
                CIJSNumber = request.Cijnumber,
                ProjectName = request.ProjectName,
                CostCenterId = request.CostCenterId,
                BudgetProvision = request.BudgetAvailable,
                BudgetAmount = request.BudgetAmount,
                ItemtypeId = request.ItemTypeId,
                TotalEquipmentCost = request.TotalEquipmentCost,
                RequestDate = request.RequestDate,
                ProjectCost = request.ProjectCost,
                Scehcost = request.Scehcost,
                PurchasePurposeId = request.PurchasePurposeId,
                OldEquipmentTreatmentId = request.OldEquipmentTreatmentId,
                OldEquipmentCost = request.OldEquipmentCost,
                WaitingPeriod = request.WaitingPeriod,
                StatusId = request.StatusId,
                CurrentWorkflowStepId = request.CurrentWorkflowStepId
            };

            model.Equipments = await _context.CijEquipments
                .Where(x => x.Cijid == cijId)
                .Select(x => new CIJEquipmentViewModel
                {
                    EquipmentId = x.EquipmentId,
                    EquipmentName = x.EquipmentName,
                    EquipmentQty = x.Qty,
                    Make = x.Make,
                    Model = x.Model,
                    EquipmentCost = x.EquipmentCost,
                    PreferenceOrder = x.PreferenceOrder
                }).ToListAsync();

            model.EquipmentJson = JsonSerializer.Serialize(model.Equipments);

            model.Justification = await _context.CijJustifications
            .Where(x => x.Cijid == cijId)
            .Select(x => new CIJJustificationViewModel
            {
                Roinumber = x.Roinumber,
                IsPurchasedEarlier = x.IsPurchasedEarlier,
                Justification = x.Justification,
                Remarks = x.Remarks,
            }).FirstOrDefaultAsync();

            model.CommitteeComment = await _context.CijCommitteeComments
                .Where(x => x.Cijid == cijId)
                .Select(x => new CommitteeCommentViewModel
                {
                    CommentDate = x.CommentDate,
                    Comments = x.Comments,

                }).FirstOrDefaultAsync();

            return model;

        }
    }

}
