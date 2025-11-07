using EgitimKayit.Models;
using EgitimKayit.ViewModels;

namespace EgitimKayit.Services
{
    public interface IAuthService
    {
        Task<Personel?> LoginAsync(LoginViewModel model);
        Task<bool> ChangePasswordAsync(string tc, string currentPassword, string newPassword);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}