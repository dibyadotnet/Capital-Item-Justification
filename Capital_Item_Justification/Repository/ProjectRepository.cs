using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly CIJDbContext _context;

        public ProjectRepository(CIJDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectViewModel>> GetAllAsync()
        {
            return await _context.CijProjects.Select(x => new ProjectViewModel()
            {
                ProjectId = x.ProjectId,
                ProjectCode = x.ProjectCode,
                ProjectName=x.ProjectName,
                IsActive = x.IsActive,
            }).OrderBy(x => x.ProjectCode).ToListAsync();
        }

        public async Task<ProjectViewModel?> GetByIdAsync(int id)
        {
            return await _context.CijProjects.Where(x => x.IsActive == true && x.ProjectId == id).Select(x => new ProjectViewModel()
            {
                ProjectId = x.ProjectId,
                ProjectCode = x.ProjectCode,
                ProjectName = x.ProjectName,
                IsActive = x.IsActive,
            }).OrderBy(x => x.ProjectCode).FirstOrDefaultAsync();
        }

        public async Task AddAsync(ProjectViewModel vm, string userId)
        {
            CijProject entity = new();
            entity.ProjectName = vm.ProjectName;
            entity.ProjectCode = vm.ProjectCode;
            entity.IsActive = true;
            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            await _context.CijProjects.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProjectViewModel vm, int id, string userId)
        {
            var entity = _context.CijProjects.Where(x => x.IsActive == true && x.ProjectId == id).FirstOrDefault();
            if (entity == null)
                throw new InvalidOperationException();

            entity.ProjectName = vm.ProjectName;
            entity.ProjectCode = vm.ProjectCode;
            entity.ModifiedBy = userId;
            entity.ModifiedDate = DateTime.Now;

            _context.CijProjects.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var entity = _context.CijProjects.Where(x => x.ProjectId == id).FirstOrDefault();

            if (entity == null)
                throw new InvalidOperationException();

            entity.IsActive = !entity.IsActive;
            entity.ModifiedBy = userId;
            entity.ModifiedDate = DateTime.Now;

            _context.CijProjects.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> ProjectExistsAsync(string ProjectCode, int? ProjectId)
        {
            return await _context.CijProjects.AnyAsync(x =>
            x.ProjectCode.ToLower() == ProjectCode.ToLower()
            && (!ProjectId.HasValue || x.ProjectId != ProjectId.Value));
        }
    }
}
