using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<List<UserViewModel>> GetGetUsersAsync();
    }
}
