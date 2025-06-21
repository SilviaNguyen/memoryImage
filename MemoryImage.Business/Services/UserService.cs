using MemoryImage.Models;
using MemoryImage.Data.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MemoryImage.Business.Services
{
    public partial class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHostEnvironment _hostEnvironment;

        public UserService(IUserRepository userRepository, IHostEnvironment hostEnvironment)
        {
            _userRepository = userRepository;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<bool> UpdateProfilePictureAsync(int userId, IFormFile imageFile)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            
            if (!string.IsNullOrWhiteSpace(user.ProfilePicture))
            {
                DeleteImage(user.ProfilePicture);
            }

            string? newImageUrl = await SaveImageAsync(imageFile, "profile_pictures");
            user.ProfilePicture = newImageUrl;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> RemoveProfilePictureAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(user.ProfilePicture))
            {
                DeleteImage(user.ProfilePicture);
            }

            user.ProfilePicture = null;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UpdateUserBioAsync(int userId, string bio)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            user.Bio = bio;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        private async Task<string?> SaveImageAsync(IFormFile imageFile, string subfolder)
        {
            if (imageFile == null || imageFile.Length == 0) return null;

            string uploadsFolder = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "images", subfolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return $"/images/{subfolder}/{uniqueFileName}";
        }
        
        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;

            string fullPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", imageUrl.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        // Implement phương thức xóa tài khoản
        public async Task<bool> DeleteAccountAsync(int userId)
        {
            // Do đã cấu hình Cascade Delete, chỉ cần gọi phương thức xóa vĩnh viễn của repository
            return await _userRepository.DeletePermanentlyAsync(userId);
        }
    }
}
