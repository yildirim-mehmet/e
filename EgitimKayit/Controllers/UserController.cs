using EgitimKayit.Data;
using EgitimKayit.Models;
using EgitimKayit.Services;
using EgitimKayit.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EgitimKayit.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;
        private readonly IAuthService _authService;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger, IAuthService authService)
        {
            _context = context;
            _logger = logger;
            _authService = authService;
        }

        #region Kullanıcı Listesi - Sadece yetkililer görebilir
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Yetki kontrolü
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            var kullanicilar = await _context.Personel
                .Include(p => p.StatuBilgi)
                .Where(p => p.Aktif == 1)
                .OrderByDescending(p => p.Tarih)
                .ToListAsync();

            return View(kullanicilar);
        }
        #endregion

        #region Kullanıcı Detay Görüntüleme
        [HttpGet]
        public async Task<IActionResult> Details(string tc)
        {
            // Yetki kontrolü
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            if (string.IsNullOrEmpty(tc))
            {
                return NotFound();
            }

            var kullanici = await _context.Personel
                .Include(p => p.StatuBilgi)
                .FirstOrDefaultAsync(p => p.Tc == tc && p.Aktif == 1);

            if (kullanici == null)
            {
                return NotFound();
            }

            return View(kullanici);
        }
        #endregion

        #region Kullanıcı Düzenleme - GET
        [HttpGet]
        public async Task<IActionResult> Edit(string tc)
        {
            // Yetki kontrolü
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            if (string.IsNullOrEmpty(tc))
            {
                return NotFound();
            }

            var kullanici = await _context.Personel
                .FirstOrDefaultAsync(p => p.Tc == tc && p.Aktif == 1);

            if (kullanici == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Tc = kullanici.Tc,
                Adlar = kullanici.Adlar,
                StatuId = kullanici.Statu,
                Kuvvet = kullanici.Kuvvet,
                Sinif = kullanici.Sinif,
                Sicil = kullanici.Sicil,
                Birim1 = kullanici.Birim1,
                Birim2 = kullanici.Birim2,
                Birim3 = kullanici.Birim3,
                Tip = kullanici.Tip
            };

            // Dropdown listelerini doldur
            await FillDropdownLists(model);

            return View(model);
        }
        #endregion

        #region Kullanıcı Düzenleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            // Yetki kontrolü
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            if (!ModelState.IsValid)
            {
                await FillDropdownLists(model);
                return View(model);
            }

            try
            {
                var kullanici = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == model.Tc && p.Aktif == 1);

                if (kullanici == null)
                {
                    return NotFound();
                }

                // Kullanıcı tipi değişikliği kontrolü
                if (currentUserTip != "yonetici" && model.Tip != kullanici.Tip)
                {
                    ModelState.AddModelError("Tip", "Sadece yöneticiler kullanıcı tipini değiştirebilir.");
                    await FillDropdownLists(model);
                    return View(model);
                }

                // Bilgileri güncelle
                kullanici.Adlar = model.Adlar;
                kullanici.Statu = model.StatuId;
                kullanici.Kuvvet = model.Kuvvet;
                kullanici.Sinif = model.Sinif;
                kullanici.Sicil = model.Sicil;
                kullanici.Birim1 = model.Birim1;
                kullanici.Birim2 = model.Birim2;
                kullanici.Birim3 = model.Birim3;
                kullanici.Tip = model.Tip;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kullanıcı bilgileri başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı düzenleme hatası - TC: {Tc}", model.Tc);
                ModelState.AddModelError("", "Kullanıcı güncellenirken hata oluştu: " + ex.Message);
                await FillDropdownLists(model);
                return View(model);
            }
        }
        #endregion

        #region Şifre Sıfırlama - GET
        [HttpGet]
        public IActionResult ResetPassword(string tc)
        {
            // Yetki kontrolü
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            var model = new ResetPasswordViewModel { Tc = tc };
            return View(model);
        }
        #endregion

        #region Şifre Sıfırlama - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            // Yetki kontrolü
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var kullanici = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == model.Tc && p.Aktif == 1);

                if (kullanici == null)
                {
                    return NotFound();
                }

                // Şifreyi hashle ve kaydet
                kullanici.Sifre = _authService.HashPassword(model.NewPassword);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Şifre başarıyla sıfırlandı.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre sıfırlama hatası - TC: {Tc}", model.Tc);
                ModelState.AddModelError("", "Şifre sıfırlanırken hata oluştu: " + ex.Message);
                return View(model);
            }
        }
        #endregion

        #region Kullanıcı Silme (Pasifleştirme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string tc)
        {
            // Yetki kontrolü - sadece yönetici silebilir
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yönetici yetkisi gereklidir.";
                return RedirectToAction("Index");
            }

            try
            {
                var kullanici = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == tc && p.Aktif == 1);

                if (kullanici == null)
                {
                    return NotFound();
                }

                // Silme yerine pasifleştirme
                kullanici.Aktif = 0;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kullanıcı başarıyla pasifleştirildi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı silme hatası - TC: {Tc}", tc);
                TempData["ErrorMessage"] = "Kullanıcı silinirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Yardımcı Metod - Dropdown Listelerini Doldur
        private async Task FillDropdownLists(EditUserViewModel model)
        {
            model.StatuList = await _context.Statu.ToListAsync();
            model.Birim1List = await _context.Birim.Where(b => b.BirimSeviye == 1).ToListAsync();
        }
        #endregion
    }
}