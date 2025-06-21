using MemoryImage.Models;
using MemoryImage.Models.ViewModels;
using MemoryImage.Data.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace MemoryImage.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        
        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        public async Task<User> LoginAsync(LoginViewModel model)
        {
            var user = await _userRepository.GetByEmailAsync(model.Email);
            if (user != null && user.IsActive && VerifyPassword(model.Password, user.PasswordHash))
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                return user;
            }
            return null;
        }
        
        public async Task<User> RegisterAsync(RegisterViewModel model)
        {
            if (await _userRepository.ExistsAsync(model.Email))
                return null; // User already exists
                
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                IsActive = true,
                ProfilePicture = "../images/pf.png" 
            };
            
            return await _userRepository.CreateAsync(user);
        }
        
        public async Task<bool> LogoutAsync()
        {
            // Implement logout logic if needed
            return await Task.FromResult(true);
        }
        
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "SaltKey"));
                return Convert.ToBase64String(hashedBytes);
            }
        }
        
        public bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
