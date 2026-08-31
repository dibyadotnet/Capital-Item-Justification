using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Repository
{
    public class DeptRepository : IDeptRepository
    {
        private readonly CIJDbContext _context;

        public DeptRepository(CIJDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentViewModel>> GetAllAsync()
        {
            return await _context.CijDepartments.Where(x => x.IsActive == true).Select(x => new DepartmentViewModel()
            {
                DepartmentId = x.DepartmentId,
                DepartmentName = x.DepartmentName,
                IsActive = x.IsActive,
            }).OrderBy(x => x.DepartmentName).ToListAsync();
        }

        public async Task<DepartmentViewModel?> GetByIdAsync(int id)
        {
            return await _context.CijDepartments.Where(x => x.IsActive == true && x.DepartmentId == id).Select(x => new DepartmentViewModel()
            {
                DepartmentId = x.DepartmentId,
                DepartmentName = x.DepartmentName,
                IsActive = x.IsActive,
            }).OrderBy(x => x.DepartmentName).FirstOrDefaultAsync();
        }

        public async Task AddAsync(DepartmentViewModel vm, string userId)
        {
            CijDepartment entity = new();
            entity.DepartmentName = vm.DepartmentName;
            entity.DepartmentCode = vm.DepartmentName;
            entity.IsActive = true;
            entity.CreatedBy = userId;
            entity.CreatedOn = DateTime.Now;

            await _context.CijDepartments.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DepartmentViewModel vm, int id, string userId)
        {
            var entity = _context.CijDepartments.Where(x => x.IsActive == true && x.DepartmentId == id).FirstOrDefault();
            if (entity == null)
                throw new InvalidOperationException();

            entity.DepartmentName = vm.DepartmentName;
            entity.ModifiedBy = userId;
            entity.ModifiedOn = DateTime.Now;

            _context.CijDepartments.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var entity = _context.CijDepartments.Where(x => x.IsActive == true && x.DepartmentId == id).FirstOrDefault();
            if (entity == null)
                throw new InvalidOperationException();

            entity.IsActive = false;
            entity.ModifiedBy = userId;
            entity.ModifiedOn = DateTime.Now;

            _context.CijDepartments.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeptExistsAsync(string DeptName, int? DeptId)
        {
            return await _context.CijDepartments.AnyAsync(x =>
            x.DepartmentName.ToLower() == DeptName.ToLower()
            && (!DeptId.HasValue ||
                x.DepartmentId != DeptId.Value));
        }
    }
}
