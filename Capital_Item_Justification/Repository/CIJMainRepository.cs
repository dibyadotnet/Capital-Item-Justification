using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IConfiguration _configuration;
        public CIJMainRepository(CIJDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            int statusId = _context.CijStatuses.Where(a => a.IsActive == true && a.StatusName == "Draft").Select(a => a.StatusId).FirstOrDefault();

            // Save Request
            var request = new CijRequest
            {
                Cijnumber = model?.CIJRequest?.CIJSNumber, //model?.CIJRequest?.CIJSNumber ?? "S-01",
                ProjectId = model?.CIJRequest?.ProjectId,
                CostCenterId = model?.CIJRequest?.CostCenterId,
                BudgetAvailable = model.CIJRequest.BudgetProvision,
                BudgetAmount = model.CIJRequest.BudgetAmount,
                ItemTypeId = model.CIJRequest.ItemtypeId,
                TotalEquipmentCost = model.CIJRequest.TotalEquipmentCost,
                RequestDate = model.CIJRequest.RequestDate,
                ProjectCost = model.CIJRequest.ProjectCost,
                Scehcost = model.CIJRequest.Scehcost,
                PurchasePurposeId = model.CIJRequest.PurchasePurposeId,
                OldEquipmentTreatmentId = model.CIJRequest.OldEquipmentTreatmentId,
                OldEquipmentCost = model.CIJRequest.OldEquipmentCost,
                WaitingPeriod = model.CIJRequest.WaitingPeriod,
                StatusId = statusId, //model.CIJRequest.StatusId, 1- Draft
                CurrentWorkflowStepId = model.CIJRequest.CurrentWorkflowStepId,
                BeneficiaryDepartmentId = model.CIJRequest.BeneficiaryDepartmentId,
                BeneficiaryLocationId = model.CIJRequest.BeneficiaryLocationId,
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
                        PreferenceOrder = item.PreferenceOrder,
                        IsActive = true
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

            // 2. Save attachments
            if (model?.Attachments != null && model.Attachments.Count > 0)
            {
                await SaveAttachmentsAsync(cijId, model.Attachments);
            }

            return "Success";
        }
        public async Task<string> UpdateCIJ(CIJMainViewModel model)
        {
            try
            {

                if (model == null)
                    return string.Empty;

                var request = await _context.CijRequests
                    .FirstOrDefaultAsync(x => x.Cijid == model.CIJRequest.Cijid);

                if (request == null)
                    return "CIJ record not found.";

                // Update Request
                request.Cijnumber = model.CIJRequest.CIJSNumber;
                request.ProjectId = model.CIJRequest.ProjectId;
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
                //request.StatusId = model.CIJRequest.StatusId;
                request.CurrentWorkflowStepId = model.CIJRequest.CurrentWorkflowStepId;
                request.BeneficiaryDepartmentId = model.CIJRequest.BeneficiaryDepartmentId;
                request.BeneficiaryLocationId = model.CIJRequest.BeneficiaryLocationId;

                await _context.SaveChangesAsync();

                int cijId = request.Cijid;

                //Update Equipment
                // Existing equipment IDs coming from UI
                var equipmentIds = model?.Equipments?.Where(x => x.EquipmentId > 0).Select(x => x.EquipmentId).ToList();


                // Delete removed equipments
                var removedEquipments = await _context.CijEquipments
                    .Where(x => x.Cijid == cijId && !equipmentIds.Contains(x.EquipmentId))
                    .ToListAsync();

                if (removedEquipments.Any())
                {
                    //_context.CijEquipments.RemoveRange(removedEquipments);
                    if (removedEquipments.Any())
                    {
                        foreach (var equipment in removedEquipments)
                        {
                            equipment.IsActive = false;
                            equipment.ModifiedDate = DateTime.Now;
                            // equipment.ModifiedBy = userId;        
                        }
                        _context.CijEquipments.UpdateRange(removedEquipments);
                    }
                }

                foreach (var item in model.Equipments)
                {
                    if (item.EquipmentId > 0)
                    {
                        var existingEquipment = await _context.CijEquipments.Where(x => x.EquipmentId == item.EquipmentId).FirstOrDefaultAsync();
                        if (existingEquipment != null)
                        {
                            //existingEquipment.Cijid = cijId;
                            existingEquipment.EquipmentName = item.EquipmentName;
                            existingEquipment.Qty = item.EquipmentQty;
                            existingEquipment.Make = item.Make;
                            existingEquipment.Model = item.Model;
                            existingEquipment.EquipmentCost = item.EquipmentCost;
                            existingEquipment.PreferenceOrder = item.PreferenceOrder;
                        }
                    }
                    else
                    {
                        CijEquipment cijEquipment = new CijEquipment
                        {
                            Cijid = cijId,
                            EquipmentName = item.EquipmentName,
                            Qty = item.EquipmentQty,
                            Make = item.Make,
                            Model = item.Model,
                            EquipmentCost = item.EquipmentCost,
                            PreferenceOrder = item.PreferenceOrder,
                            IsActive = true
                        };
                        _context.CijEquipments.Add(cijEquipment);
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

                var committeeComment = await _context.CijCommitteeComments.FirstOrDefaultAsync(x => x.Cijid == cijId);

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

                // 2. Save attachments
                if (model.Attachments != null && model.Attachments.Count > 0)
                {
                    await SaveAttachmentsAsync(cijId, model.Attachments);
                }

                return "Success";

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<DashboardViewModel>> GetDashboard()
        {
            List<DashboardViewModel> dashboardViewModels = new();

            dashboardViewModels = await (
                       from r in _context.CijRequests

                       join it in _context.CijItemTypes
                       on r.ItemTypeId equals it.ItemTypeId into itemGroup
                       from it in itemGroup.DefaultIfEmpty()

                       join proj in _context.CijProjects
                       on r.ProjectId equals proj.ProjectId into projGroup
                       from proj in projGroup.DefaultIfEmpty()

                       join loc in _context.CijLocations
                       on r.CostCenterId equals loc.LocationId into locGroup
                       from loc in locGroup.DefaultIfEmpty()

                       join st in _context.CijStatuses
                      on r.StatusId equals st.StatusId into stGroup
                       from st in stGroup.DefaultIfEmpty()

                       select new DashboardViewModel
                       {
                           CIJId = r.Cijid,
                           CIJNumber = r.Cijnumber,
                           RequestDate = r.RequestDate,
                           ProjectName = proj != null ? proj.ProjectCode : "",
                           ItemType = it != null ? it.ItemTypeName : "",
                           CostCenter = loc != null ? loc.LocationName : "",
                           TotalEquipmentCost = r.TotalEquipmentCost,
                           Status = st != null ? st.StatusName : ""
                       }).AsNoTracking().ToListAsync();

            return dashboardViewModels;
        }
        public async Task<List<CijLocation>> GetLocation()
        {
            return await _context.CijLocations.Where(a => a.IsActive == true).Select(a => new CijLocation()
            {
                LocationId = a.LocationId,
                LocationName = a.LocationName,
                Prefix = a.Prefix
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
                ProjectId = request.ProjectId,
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
                CurrentWorkflowStepId = request.CurrentWorkflowStepId,
                BeneficiaryDepartmentId = request.BeneficiaryDepartmentId,
                BeneficiaryLocationId = request.BeneficiaryLocationId
            };

            model.Equipments = await _context.CijEquipments
                .Where(x => x.Cijid == cijId && x.IsActive == true)
                .Select(x => new CIJEquipmentViewModel
                {
                    EquipmentId = x.EquipmentId,
                    EquipmentName = x.EquipmentName,
                    EquipmentQty = x.Qty,
                    Make = x.Make,
                    Model = x.Model,
                    EquipmentCost = x.EquipmentCost,
                    PreferenceOrder = x.PreferenceOrder,
                    Cijid = x.Cijid,
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

        public async Task<string> GenerateCIJNumber(int? locationId)
        {
            // Financial Year
            var today = DateTime.Today;
            int startYear = today.Month >= 4 ? today.Year : today.Year - 1;
            int endYear = startYear + 1;
            string financialYear = $"{startYear}-{endYear.ToString().Substring(2)}";

            //get location Prefix text
            var locations = await GetLocation();
            string? locPrefix = locations.Where(a => a.LocationId == locationId).Select(a => a.Prefix).FirstOrDefault();

            // Last CIJ Number for this location and FY
            var lastCIJ = await _context.CijRequests
                .Where(x => x.Cijnumber.StartsWith($"{locPrefix}/{financialYear}/"))
                .OrderByDescending(x => x.Cijnumber)
                .Select(x => x.Cijnumber)
                .FirstOrDefaultAsync();

            int nextSequence = 1;

            if (!string.IsNullOrEmpty(lastCIJ))
            {
                string lastSeq = lastCIJ.Split('/').Last();
                nextSequence = int.Parse(lastSeq) + 1;
            }

            return $"{locPrefix}/{financialYear}/{nextSequence:D5}";
        }
        public async Task<List<CijProject>> GetProjectCode()
        {
            var projects = await _context.CijProjects.Where(a => a.IsActive == true).Select(a => new CijProject()
            {
                ProjectId = a.ProjectId,
                ProjectCode = a.ProjectCode,
                ProjectName = a.ProjectName
            }).OrderBy(a => a.ProjectId).ToListAsync();

            return projects;
        }

        public async Task SaveAttachmentsAsync(int cijId, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return;


            var configuredPath = _configuration["FileStorage:CIJAttachmentPath"];

            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                throw new InvalidOperationException(
                    "CIJ attachment storage path is not configured.");
            }


            //// Physical root outside wwwroot
            //var basePath = Path.Combine(
            //    _environment.ContentRootPath,
            //    configuredPath);


            //// CIJ-specific folder
            //var cijFolder = Path.Combine(
            //    basePath,
            //    cijId.ToString());

            if (!Directory.Exists(configuredPath))
            {
                Directory.CreateDirectory(configuredPath);
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg" };

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                // Original file name
                var originalFileName = Path.GetFileName(file.FileName);
                // Extension
                var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

                // Validate extension
                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException(
                        $"File type '{extension}' is not allowed.");
                }

                // Unique physical file name
                var storedFileName = $"{Guid.NewGuid():N}{extension}";

                // Physical file path
                var physicalFilePath = Path.Combine(configuredPath, storedFileName);

                // Save physical file
                await using (var stream = new FileStream(physicalFilePath, FileMode.CreateNew))
                {
                    await file.CopyToAsync(stream);
                }
                // Relative path stored in DB
                var relativePath = Path.Combine(configuredPath, storedFileName).Replace("\\", "/");


                // Database record
                var attachment = new CijAttachment
                {
                    Cijid = cijId,
                    DocumentTypeId = 1,
                    FileName = originalFileName,
                    FilePath = relativePath,
                    UploadedBy = 1,
                    UploadedDate = DateTime.Now,
                    IsActive = true,
                    CreatedBy = 1,
                    CreatedDate = DateTime.Now
                };
                _context.CijAttachments.Add(attachment);
            }
            await _context.SaveChangesAsync();
        }
        public async Task<List<AttachmentViewModel>> GetAttachmentsById(int cijId)
        {
            List<AttachmentViewModel> attachments = new();
            attachments = await _context.CijAttachments.Where(a => a.Cijid == cijId && a.IsActive == true).Select(a=> new AttachmentViewModel()
            {
                AttachmentId=a.AttachmentId,
                Cijid=a.Cijid,
                FileName=a.FileName,
                FilePath=a.FilePath, 
            }).ToListAsync();

            return attachments;
        }
        public async Task<int> DeleteAttachment(int attachmentId)
        {
            int deleteStatus = 1;
            var attachment = await _context.CijAttachments.FirstOrDefaultAsync(x => x.AttachmentId == attachmentId && x.IsActive==true);

            if (attachment == null)
            {
               return deleteStatus = 0;
            }

            attachment.IsActive = false;
            await _context.SaveChangesAsync();

            // Delete physical file
            if (!string.IsNullOrEmpty(attachment.FilePath) &&
                System.IO.File.Exists(attachment.FilePath))
            {
                System.IO.File.Delete(attachment.FilePath);
            }
            return deleteStatus;
        }
    }
}
