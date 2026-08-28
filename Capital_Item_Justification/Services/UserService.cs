using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<List<UserViewModel>> GetGetUsersAsync()
        {
            return await _userRepository.GetGetUsersAsync();
        }
    }
}
