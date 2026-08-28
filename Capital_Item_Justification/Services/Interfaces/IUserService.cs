using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserViewModel>> GetGetUsersAsync();
    }
}
