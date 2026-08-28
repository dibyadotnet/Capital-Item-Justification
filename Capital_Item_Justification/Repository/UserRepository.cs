using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Capital_Item_Justification.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly CIJDbContext _context;
        public UserRepository(CIJDbContext context)
        {
            _context = context;
        }
        public async Task<List<UserViewModel>> GetGetUsersAsync()
        {

            List<UserViewModel> vm = await (from user in _context.Users
                                      join dept in _context.CijDepartments
                                          on user.DepartmentId equals dept.DepartmentId into departmentGroup
                                      from dept in departmentGroup.DefaultIfEmpty()

                                      join location in _context.CijLocations
                                          on user.LocationId equals location.LocationId into locationGroup
                                      from location in locationGroup.DefaultIfEmpty()

                                      join userRole in _context.UserRoles
                                      on user.Id equals userRole.UserId into userRoleGroup
                                      from userRole in userRoleGroup.DefaultIfEmpty()

                                      join role in _context.Roles
                                          on userRole.RoleId equals role.Id into roleGroup
                                      from role in roleGroup.DefaultIfEmpty()
                                      select new UserViewModel
                                      {
                                          Id = user.Id,
                                          FullName = user.FullName ?? "",
                                          EmployeeCode = user.EmployeeCode ?? "",
                                          Email = user.Email ?? "",
                                          DepartmentName = dept != null ? dept.DepartmentName : null,
                                          LocationName = location != null ? location.LocationName : null,
                                          IsActive = user.IsActive,
                                          RoleName = role != null? role.Name: null,
                                      }).OrderBy(x => x.FullName).ToListAsync();

            return vm;
        }
    }
}
