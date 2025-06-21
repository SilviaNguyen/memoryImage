using MemoryImage.Models;
using MemoryImage.Models.ViewModels;

namespace MemoryImage.Business.Services
{
    public interface IAuthService
    {
        Task<User> LoginAsync(LoginViewModel model);
        Task<User> RegisterAsync(RegisterViewModel model);
        Task<bool> LogoutAsync();
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}