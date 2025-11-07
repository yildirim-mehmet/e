using EgitimKayit.Data;
using EgitimKayit.Models;
using EgitimKayit.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EgitimKayit.Controllers
{
    public class EgitimTipController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EgitimTipController> _logger;

        public EgitimTipController(ApplicationDbContext context, ILogger<EgitimTipController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region EgitimTip Listesi - Sadece yetkililer görebilir
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

            var egitimTipleri = _context.EgitimTip
                .Include(et => et.Dershane)
                .Include(et => et.Yaratan)
                .Where(et => et.Dershane != null && et.Dershane.Durum == 1)
                .AsQueryable();

            // Dershane filtresi
            if (dershaneId.HasValue)
            {
                egitimTipleri = egitimTipleri.Where(et => et.DerId == dershaneId.Value);
            }

            var model = new EgitimTipIndexViewModel
            {
                EgitimTipleri = await egitimTipleri.OrderByDescending(et => et.Tarih).ToListAsync(),
                Dershaneler = await _context.Dershane.Where(d => d.Durum == 1).ToListAsync(),
                SeciliDershaneId = dershaneId
            };

            return View(model);
        }
        #endregion

        #region EgitimTip Oluşturma - GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Yetki kontrolü - sadece yönetici
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yönetici yetkisi gereklidir.";
                return RedirectToAction("Index");
            }

            var model = new EgitimTipViewModel();
            await FillDropdownLists(model);
            return View(model);
        }
        #endregion

        #region EgitimTip Oluşturma - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EgitimTipViewModel model)
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
                await FillDropdownLists(model);
                return View(model);
            }

            try
            {
                // Aynı dershanede aynı isimde eğitim tipi kontrolü
                if (await _context.EgitimTip.AnyAsync(et => et.DerId == model.DerId && et.Ad == model.Ad))
                {
                    ModelState.AddModelError("Ad", "Bu dershanede aynı isimde eğitim tipi bulunmaktadır.");
                    await FillDropdownLists(model);
                    return View(model);
                }

                var yeniEgitimTip = new EgitimTip
                {
                    DerId = model.DerId,
                    Ad = model.Ad,
                    YaratanTc = HttpContext.Session.GetString("PersonelTc"),
                    Tarih = DateTime.Now
                };

                _context.EgitimTip.Add(yeniEgitimTip);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Eğitim tipi başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EgitimTip oluşturma hatası - Ad: {Ad}, DershaneId: {DerId}", model.Ad, model.DerId);
                ModelState.AddModelError("", "Eğitim tipi oluşturulurken hata oluştu: " + ex.Message);
                await FillDropdownLists(model);
                return View(model);
            }
        }
        #endregion

        #region EgitimTip Düzenleme - GET
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

            var egitimTip = await _context.EgitimTip
                .Include(et => et.Dershane)
                .FirstOrDefaultAsync(et => et.Id == id);

            if (egitimTip == null)
            {
                return NotFound();
            }

            var model = new EgitimTipViewModel
            {
                Id = egitimTip.Id,
                DerId = egitimTip.DerId,
                Ad = egitimTip.Ad
            };

            await FillDropdownLists(model);
            return View(model);
        }
        #endregion

        #region EgitimTip Düzenleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EgitimTipViewModel model)
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
                await FillDropdownLists(model);
                return View(model);
            }

            try
            {
                // Aynı dershanede aynı isimde başka eğitim tipi kontrolü (kendisi hariç)
                if (await _context.EgitimTip.AnyAsync(et => et.DerId == model.DerId && et.Ad == model.Ad && et.Id != model.Id))
                {
                    ModelState.AddModelError("Ad", "Bu dershanede aynı isimde eğitim tipi bulunmaktadır.");
                    await FillDropdownLists(model);
                    return View(model);
                }

                var egitimTip = await _context.EgitimTip
                    .FirstOrDefaultAsync(et => et.Id == model.Id);

                if (egitimTip == null)
                {
                    return NotFound();
                }

                egitimTip.DerId = model.DerId;
                egitimTip.Ad = model.Ad;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Eğitim tipi başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EgitimTip düzenleme hatası - ID: {Id}, Ad: {Ad}", model.Id, model.Ad);
                ModelState.AddModelError("", "Eğitim tipi güncellenirken hata oluştu: " + ex.Message);
                await FillDropdownLists(model);
                return View(model);
            }
        }
        #endregion

        #region EgitimTip Detay Görüntüleme
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

            var egitimTip = await _context.EgitimTip
                .Include(et => et.Dershane)
                .Include(et => et.Yaratan)
                .Include(et => et.EgitimSablonlari)
                .FirstOrDefaultAsync(et => et.Id == id);

            if (egitimTip == null)
            {
                return NotFound();
            }

            return View(egitimTip);
        }
        #endregion

        #region EgitimTip Silme
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
                var egitimTip = await _context.EgitimTip
                    .Include(et => et.EgitimSablonlari)
                    .FirstOrDefaultAsync(et => et.Id == id);

                if (egitimTip == null)
                {
                    return NotFound();
                }

                // Eğitim tipine bağlı eğitim şablonları varsa silinemez
                if (egitimTip.EgitimSablonlari != null && egitimTip.EgitimSablonlari.Any())
                {
                    TempData["ErrorMessage"] = "Bu eğitim tipine bağlı eğitim şablonları bulunmaktadır. Önce eğitim şablonlarını silmelisiniz.";
                    return RedirectToAction("Index");
                }

                _context.EgitimTip.Remove(egitimTip);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Eğitim tipi başarıyla silindi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EgitimTip silme hatası - ID: {Id}", id);
                TempData["ErrorMessage"] = "Eğitim tipi silinirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Yardımcı Metod - Dropdown Listelerini Doldur
        private async Task FillDropdownLists(EgitimTipViewModel model)
        {
            model.Dershaneler = await _context.Dershane
                .Where(d => d.Durum == 1)
                .OrderBy(d => d.Ad)
                .ToListAsync();
        }
        #endregion
    }
}