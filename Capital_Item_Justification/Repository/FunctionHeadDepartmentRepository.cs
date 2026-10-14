using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Repository
{
    public class FunctionHeadDepartmentRepository : IFunctionHeadDepartmentRepository
    {
        private readonly CIJDbContext _context;

        public FunctionHeadDepartmentRepository(CIJDbContext context)
        {
            _context = context;
        }

        public async Task<List<FunctionHeadDepartmentViewModel>> GetAllAsync()
        {
            return await (
                        from fh in _context.CijFunctionHeadDepartments
                        join d in _context.CijDepartments
                            on fh.DepartmentId equals d.DepartmentId
                        select new FunctionHeadDepartmentViewModel
                        {
                            DepartmentId = fh.DepartmentId,
                            DepartmentName = d.DepartmentName,
                            IsActive = fh.IsActive,
                            Id= fh.Id,
                        }
                    ).OrderBy(x => x.DepartmentName).ToListAsync();
        }

        public async Task<FunctionHeadDepartmentViewModel?> GetByIdAsync(int id)
        {
            return await (
                         from fh in _context.CijFunctionHeadDepartments
                         join d in _context.CijDepartments
                             on fh.DepartmentId equals d.DepartmentId
                         where fh.Id == id
                         select new FunctionHeadDepartmentViewModel
                         {
                             DepartmentId = fh.DepartmentId,
                             DepartmentName = d.DepartmentName,
                             IsActive = fh.IsActive
                         }
                     ).OrderBy(x => x.DepartmentName).FirstOrDefaultAsync();
        }

        public async Task AddAsync(FunctionHeadDepartmentViewModel vm, string userId)
        {
            CijFunctionHeadDepartment entity = new();
            var role = _context.Roles.Where(a => a.Name == "Function Head").FirstOrDefault();
            if (role == null)
                throw new InvalidOperationException();
            entity.DepartmentId = vm.DepartmentId ?? 0;
            entity.RoleId = role.Id;
            entity.IsActive = true;
            entity.CreatedBy = userId;
            entity.CreatedOn = DateTime.Now;

            await _context.CijFunctionHeadDepartments.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FunctionHeadDepartmentViewModel vm, int id, string userId)
        {
            var entity = _context.CijFunctionHeadDepartments.Where(x => x.IsActive == true && x.Id == id).FirstOrDefault();
            if (entity == null)
                throw new InvalidOperationException();

            entity.DepartmentId = vm.DepartmentId ?? 0;
            entity.RoleId = "Function Head";
            entity.ModifiedBy = userId;
            entity.ModifiedOn = DateTime.Now;

            _context.CijFunctionHeadDepartments.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var entity = _context.CijFunctionHeadDepartments.Where(x => x.Id == id).FirstOrDefault();

            if (entity == null)
                throw new InvalidOperationException();

            entity.IsActive = !entity.IsActive;
            entity.ModifiedBy = userId;
            entity.ModifiedOn = DateTime.Now;

            _context.CijFunctionHeadDepartments.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeptExistsAsync(int? deptId)
        {
            return await _context.CijFunctionHeadDepartments.AnyAsync(fh => fh.DepartmentId == deptId.Value);
        }
    }
}
