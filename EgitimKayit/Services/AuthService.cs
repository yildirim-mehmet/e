using Microsoft.EntityFrameworkCore;
using EgitimKayit.Data;
using EgitimKayit.Models;
using EgitimKayit.ViewModels;
using Microsoft.Extensions.Logging;

namespace EgitimKayit.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Login İşlemi - TC ve şifre kontrolü
        public async Task<Personel?> LoginAsync(LoginViewModel model)
        {
            _logger.LogDebug("Login işlemi başlatıldı - TC: {Tc}", model.Tc);

            try
            {
                // TC'ye göre personeli bul (Aktif olanlar)
                var personel = await _context.Personel
                    .Include(p => p.StatuBilgi)
                    .FirstOrDefaultAsync(p => p.Tc == model.Tc && p.Aktif == 1);

                if (personel == null)
                {
                    _logger.LogWarning("Personel bulunamadı veya aktif değil - TC: {Tc}", model.Tc);
                    return null;
                }

                // Şifre kontrolü
                if (!VerifyPassword(model.Sifre, personel.Sifre))
                {
                    _logger.LogWarning("Şifre doğrulama başarısız - TC: {Tc}", model.Tc);
                    return null;
                }

                _logger.LogInformation("Login başarılı - Personel: {Adlar}, TC: {Tc}", personel.Adlar, personel.Tc);
                return personel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login işleminde hata - TC: {Tc}", model.Tc);
                throw;
            }
        }
        #endregion

        #region Şifre Değiştirme
        public async Task<bool> ChangePasswordAsync(string tc, string currentPassword, string newPassword)
        {
            _logger.LogDebug("Şifre değiştirme işlemi - TC: {Tc}", tc);

            try
            {
                var personel = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == tc && p.Aktif == 1);

                if (personel == null || !VerifyPassword(currentPassword, personel.Sifre))
                {
                    _logger.LogWarning("Şifre değiştirme başarısız - Personel bulunamadı veya mevcut şifre hatalı - TC: {Tc}", tc);
                    return false;
                }

                // Yeni şifreyi hashle ve kaydet
                personel.Sifre = HashPassword(newPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Şifre başarıyla değiştirildi - TC: {Tc}", tc);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre değiştirme işleminde hata - TC: {Tc}", tc);
                throw;
            }
        }
        #endregion

        #region Şifre Hashleme ve Doğrulama (BCrypt)
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        #endregion
    }
}