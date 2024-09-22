using IndProBackend.Entities;

namespace IndProBackend.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user);
        Task<User?> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(int id);

        Task<string?> AuthenticateUserAsync(string usernameOrEmail, string password);
    }
}
