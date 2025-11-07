using EgitimKayit.Data;
using EgitimKayit.Models;
using EgitimKayit.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EgitimKayit.Controllers
{
    public class DershaneController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DershaneController> _logger;

        public DershaneController(ApplicationDbContext context, ILogger<DershaneController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Dershane Listesi - Sadece yetkililer görebilir
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Yetki kontrolü - sadece sorumlu ve yönetici
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            var dershaneler = await _context.Dershane
                .Include(d => d.Yaratan)
                .Where(d => d.Durum == 1)
                .OrderByDescending(d => d.Tarih)
                .ToListAsync();

            return View(dershaneler);
        }
        #endregion

        #region Dershane Oluşturma - GET
        [HttpGet]
        public IActionResult Create()
        {
            // Yetki kontrolü - sadece yönetici
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yönetici yetkisi gereklidir.";
                return RedirectToAction("Index");
            }

            return View();
        }
        #endregion

        #region Dershane Oluşturma - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DershaneViewModel model)
        {
            // Yetki kontrolü - sadece yönetici
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yönetici yetkisi gereklidir.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Aynı isimde dershane kontrolü
                if (await _context.Dershane.AnyAsync(d => d.Ad == model.Ad && d.Durum == 1))
                {
                    ModelState.AddModelError("Ad", "Bu isimde zaten bir dershane bulunmaktadır.");
                    return View(model);
                }

                var yeniDershane = new Dershane
                {
                    Ad = model.Ad,
                    Aciklama = model.Aciklama,
                    Durum = 1,
                    YaratanTc = HttpContext.Session.GetString("PersonelTc"),
                    Tarih = DateTime.Now
                };

                _context.Dershane.Add(yeniDershane);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Dershane başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dershane oluşturma hatası - Ad: {Ad}", model.Ad);
                ModelState.AddModelError("", "Dershane oluşturulurken hata oluştu: " + ex.Message);
                return View(model);
            }
        }
        #endregion

        #region Dershane Düzenleme - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Yetki kontrolü - sadece yönetici
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yönetici yetkisi gereklidir.";
                return RedirectToAction("Index");
            }

            var dershane = await _context.Dershane
                .FirstOrDefaultAsync(d => d.Id == id && d.Durum == 1);

            if (dershane == null)
            {
                return NotFound();
            }

            var model = new DershaneViewModel
            {
                Id = dershane.Id,
                Ad = dershane.Ad,
                Aciklama = dershane.Aciklama
            };

            return View(model);
        }
        #endregion

        #region Dershane Düzenleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DershaneViewModel model)
        {
            // Yetki kontrolü - sadece yönetici
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yönetici yetkisi gereklidir.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Aynı isimde başka dershane kontrolü (kendisi hariç)
                if (await _context.Dershane.AnyAsync(d => d.Ad == model.Ad && d.Id != model.Id && d.Durum == 1))
                {
                    ModelState.AddModelError("Ad", "Bu isimde zaten bir dershane bulunmaktadır.");
                    return View(model);
                }

                var dershane = await _context.Dershane
                    .FirstOrDefaultAsync(d => d.Id == model.Id && d.Durum == 1);

                if (dershane == null)
                {
                    return NotFound();
                }

                dershane.Ad = model.Ad;
                dershane.Aciklama = model.Aciklama;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Dershane başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dershane düzenleme hatası - ID: {Id}, Ad: {Ad}", model.Id, model.Ad);
                ModelState.AddModelError("", "Dershane güncellenirken hata oluştu: " + ex.Message);
                return View(model);
            }
        }
        #endregion

        #region Dershane Detay Görüntüleme
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Yetki kontrolü - sadece sorumlu ve yönetici
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            var dershane = await _context.Dershane
                .Include(d => d.Yaratan)
                .Include(d => d.EgitimTipleri)
                .FirstOrDefaultAsync(d => d.Id == id && d.Durum == 1);

            if (dershane == null)
            {
                return NotFound();
            }

            return View(dershane);
        }
        #endregion

        #region Dershane Silme (Pasifleştirme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Yetki kontrolü - sadece yönetici
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yönetici yetkisi gereklidir.";
                return RedirectToAction("Index");
            }

            try
            {
                var dershane = await _context.Dershane
                    .Include(d => d.EgitimTipleri)
                    .FirstOrDefaultAsync(d => d.Id == id && d.Durum == 1);

                if (dershane == null)
                {
                    return NotFound();
                }

                // Eğer dershaneye bağlı eğitim tipleri varsa silinemez
                if (dershane.EgitimTipleri != null && dershane.EgitimTipleri.Any())
                {
                    TempData["ErrorMessage"] = "Bu dershaneye bağlı eğitim tipleri bulunmaktadır. Önce eğitim tiplerini silmelisiniz.";
                    return RedirectToAction("Index");
                }

                // Silme yerine pasifleştirme
                dershane.Durum = 0;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Dershane başarıyla silindi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dershane silme hatası - ID: {Id}", id);
                TempData["ErrorMessage"] = "Dershane silinirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        #endregion
    }
}