using EgitimKayit.Data;
using EgitimKayit.Models;
using EgitimKayit.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EgitimKayit.Controllers
{
    public class YetkiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<YetkiController> _logger;

        public YetkiController(ApplicationDbContext context, ILogger<YetkiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Yetki Listesi - Sadece yetkililer görebilir
        [HttpGet]
        public async Task<IActionResult> Index(int? dershaneId)
        {
            // Yetki kontrolü - sadece sorumlu ve yönetici
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            var yetkiler = _context.Yetki
                .Include(y => y.Personel)
                .Include(y => y.Dershane)
                .Include(y => y.Yaratan)
                .AsQueryable();

            // Dershane filtresi
            if (dershaneId.HasValue)
            {
                yetkiler = yetkiler.Where(y => y.DerId == dershaneId.Value);
            }

            var model = new YetkiIndexViewModel
            {
                Yetkiler = await yetkiler.OrderByDescending(y => y.Tarih).ToListAsync(),
                Dershaneler = await _context.Dershane.Where(d => d.Durum == 1).ToListAsync(),
                SeciliDershaneId = dershaneId
            };

            return View(model);
        }
        #endregion

        #region Yetki Atama - GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Yetki kontrolü - sadece sorumlu ve yönetici
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var model = new YetkiViewModel();
            await FillDropdownLists(model);
            return View(model);
        }
        #endregion

        #region Yetki Atama - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(YetkiViewModel model)
        {
            // Yetki kontrolü - sadece sorumlu ve yönetici
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                await FillDropdownLists(model);
                return View(model);
            }

            try
            {
                // Aynı kişi aynı dershanede zaten yetkili mi kontrolü
                if (await _context.Yetki.AnyAsync(y => y.PerTc == model.PerTc && y.DerId == model.DerId))
                {
                    ModelState.AddModelError("", "Bu personel zaten bu dershanede yetkilidir.");
                    await FillDropdownLists(model);
                    return View(model);
                }

                // Personelin öğretmen olup olmadığını kontrol et
                var personel = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == model.PerTc && p.Aktif == 1);

                if (personel == null || (personel.Tip != "ogretmen" && personel.Tip != "sorumlu" && personel.Tip != "yonetici"))
                {
                    ModelState.AddModelError("PerTc", "Sadece öğretmen, sorumlu veya yönetici tipindeki personellere yetki atanabilir.");
                    await FillDropdownLists(model);
                    return View(model);
                }

                var yeniYetki = new Yetki
                {
                    PerTc = model.PerTc,
                    DerId = model.DerId,
                    YaratanTc = HttpContext.Session.GetString("PersonelTc"),
                    Tarih = DateTime.Now
                };

                _context.Yetki.Add(yeniYetki);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Yetki başarıyla atandı.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yetki atama hatası - PersonelTC: {PerTc}, DershaneId: {DerId}", model.PerTc, model.DerId);
                ModelState.AddModelError("", "Yetki atanırken hata oluştu: " + ex.Message);
                await FillDropdownLists(model);
                return View(model);
            }
        }
        #endregion

        #region Yetki Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Yetki kontrolü - sadece sorumlu ve yönetici
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index");
            }

            try
            {
                var yetki = await _context.Yetki
                    .Include(y => y.Personel)
                    .Include(y => y.Dershane)
                    .FirstOrDefaultAsync(y => y.Id == id);

                if (yetki == null)
                {
                    return NotFound();
                }

                // Kendi yetkisini silme kontrolü (opsiyonel - güvenlik için)
                var currentUserTc = HttpContext.Session.GetString("PersonelTc");
                if (yetki.PerTc == currentUserTc)
                {
                    TempData["ErrorMessage"] = "Kendi yetkinizi silemezsiniz.";
                    return RedirectToAction("Index");
                }

                _context.Yetki.Remove(yetki);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{yetki.Personel?.Adlar} kişisinin {yetki.Dershane?.Ad} dershanesindeki yetkisi kaldırıldı.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yetki silme hatası - ID: {Id}", id);
                TempData["ErrorMessage"] = "Yetki silinirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Personel AJAX - TC'ye göre personel bilgisi getir
        [HttpGet]
        public async Task<JsonResult> GetPersonelByTc(string tc)
        {
            var personel = await _context.Personel
                .Include(p => p.StatuBilgi)
                .FirstOrDefaultAsync(p => p.Tc == tc && p.Aktif == 1);

            if (personel == null)
            {
                return Json(new { success = false, message = "Personel bulunamadı." });
            }

            // Sadece öğretmen, sorumlu veya yönetici tipindeki personellere yetki atanabilir
            if (personel.Tip != "ogretmen" && personel.Tip != "sorumlu" && personel.Tip != "yonetici")
            {
                return Json(new
                {
                    success = false,
                    message = "Bu personel yetkili atanabilecek tipte değil. (Öğretmen, Sorumlu veya Yönetici olmalı)"
                });
            }

            return Json(new
            {
                success = true,
                adlar = personel.Adlar,
                tip = personel.Tip,
                statu = personel.StatuBilgi?.Anlam,
                birim1 = personel.Birim1
            });
        }
        #endregion

        #region Yardımcı Metod - Dropdown Listelerini Doldur
        private async Task FillDropdownLists(YetkiViewModel model)
        {
            model.Dershaneler = await _context.Dershane
                .Where(d => d.Durum == 1)
                .OrderBy(d => d.Ad)
                .ToListAsync();
        }
        #endregion
    }
}