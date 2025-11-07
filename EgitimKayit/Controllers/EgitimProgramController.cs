using EgitimKayit.Data;
using EgitimKayit.Models;
using EgitimKayit.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EgitimKayit.Controllers
{
    public class EgitimProgramController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EgitimProgramController> _logger;

        public EgitimProgramController(ApplicationDbContext context, ILogger<EgitimProgramController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region EgitimProgram Listesi - Talepçi ve üstü yetkililer görebilir
        [HttpGet]
        public async Task<IActionResult> Index(int? egitimSablonId, int? onayDurumu)
        {
            // Yetki kontrolü - talepçi ve üstü yetkililer
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            // ✨ EXPLICIT LOADING - Sadece gerekli navigation'ları yükle
            var egitimProgramlari = _context.EgitimProgram
                .Include(ep => ep.EgitimSablon)
                    .ThenInclude(es => es.Dershane)
                .Include(ep => ep.EgitimSablon)
                    .ThenInclude(es => es.EgitimTip)
                // .Include(ep => ep.Ogretmen) // GEÇİCİ OLARAK KAPAT
                // .Include(ep => ep.Yaratan) // GEÇİCİ OLARAK KAPAT
                .AsQueryable();

            // EgitimSablon filtresi
            if (egitimSablonId.HasValue)
            {
                egitimProgramlari = egitimProgramlari.Where(ep => ep.EsId == egitimSablonId.Value);
            }

            // Onay durumu filtresi
            if (onayDurumu.HasValue)
            {
                egitimProgramlari = egitimProgramlari.Where(ep => ep.Onayli == onayDurumu.Value);
            }

            var programListesi = await egitimProgramlari
                .OrderByDescending(ep => ep.Tarih)
                .ToListAsync();

            // ✨ MANUEL OLARAK YARATAN VE ÖĞRETMEN BİLGİLERİNİ YÜKLE
            foreach (var program in programListesi)
            {
                if (!string.IsNullOrEmpty(program.YaratanTc))
                {
                    program.Yaratan = await _context.Personel
                        .FirstOrDefaultAsync(p => p.Tc == program.YaratanTc);
                }

                if (!string.IsNullOrEmpty(program.PerTc))
                {
                    program.Ogretmen = await _context.Personel
                        .FirstOrDefaultAsync(p => p.Tc == program.PerTc);
                }
            }

            var model = new EgitimProgramIndexViewModel
            {
                EgitimProgramlari = programListesi,
                EgitimSablonlari = await _context.EgitimSablon
                    .Include(es => es.Dershane)
                    .Include(es => es.EgitimTip)
                    .ToListAsync(),
                SeciliEgitimSablonId = egitimSablonId,
                SeciliOnayDurumu = onayDurumu
            };

            return View(model);
        }
        #endregion

        #region EgitimProgram Oluşturma - GET (Talepçi ve üstü)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Yetki kontrolü - talepçi ve üstü yetkililer
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index");
            }

            var model = new EgitimProgramViewModel();
            await FillDropdownLists(model);
            return View(model);
        }
        #endregion

        #region EgitimProgram Oluşturma - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EgitimProgramViewModel model)
        {
            // Yetki kontrolü - talepçi ve üstü yetkililer
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                await FillDropdownLists(model);
                return View(model);
            }

            // Tarih kontrolü
            if (model.BasTar >= model.BitTar)
            {
                ModelState.AddModelError("BitTar", "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
                await FillDropdownLists(model);
                return View(model);
            }

            try
            {
                var yeniProgram = new EgitimProgram
                {
                    EsId = model.EsId,
                    PerTc = model.PerTc,
                    Onayli = null, // Varsayılan olarak beklemede
                    Aciklama = model.Aciklama,
                    BasTar = model.BasTar,
                    BitTar = model.BitTar,
                    YaratanTc = HttpContext.Session.GetString("PersonelTc"),
                    Tarih = DateTime.Now
                };

                _context.EgitimProgram.Add(yeniProgram);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Eğitim programı başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EgitimProgram oluşturma hatası - EgitimSablonId: {EsId}", model.EsId);
                ModelState.AddModelError("", "Eğitim programı oluşturulurken hata oluştu: " + ex.Message);
                await FillDropdownLists(model);
                return View(model);
            }
        }
        #endregion

        #region EgitimProgram Detay Görüntüleme
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Yetki kontrolü - talepçi ve üstü yetkililer
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            // ✨ EXPLICIT LOADING
            var egitimProgram = await _context.EgitimProgram
                .Include(ep => ep.EgitimSablon)
                    .ThenInclude(es => es.Dershane)
                .Include(ep => ep.EgitimSablon)
                    .ThenInclude(es => es.EgitimTip)
                // .Include(ep => ep.Ogretmen) // GEÇİCİ OLARAK KAPAT
                // .Include(ep => ep.Yaratan) // GEÇİCİ OLARAK KAPAT
                // .Include(ep => ep.Katilimcilar) // GEÇİCİ OLARAK KAPAT
                .FirstOrDefaultAsync(ep => ep.Id == id);

            if (egitimProgram == null)
            {
                return NotFound();
            }

            // ✨ MANUEL OLARAK YARATAN VE ÖĞRETMEN BİLGİLERİNİ YÜKLE
            if (!string.IsNullOrEmpty(egitimProgram.YaratanTc))
            {
                egitimProgram.Yaratan = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == egitimProgram.YaratanTc);
            }

            if (!string.IsNullOrEmpty(egitimProgram.PerTc))
            {
                egitimProgram.Ogretmen = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == egitimProgram.PerTc);
            }

            // Katılımcıları manuel yükle
            egitimProgram.Katilimcilar = await _context.Egitilen
                .Where(e => e.EgtProgId == id)
                .ToListAsync();

            // Katılımcıların personel bilgilerini yükle
            foreach (var katilimci in egitimProgram.Katilimcilar)
            {
                if (!string.IsNullOrEmpty(katilimci.PerTc))
                {
                    katilimci.Katilimci = await _context.Personel
                        .FirstOrDefaultAsync(p => p.Tc == katilimci.PerTc);
                }
            }

            return View(egitimProgram);
        }
        #endregion

        #region EgitimProgram Düzenleme - GET (Sadece yaratıcı veya yönetici/sorumlu)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // ✨ EXPLICIT LOADING
            var egitimProgram = await _context.EgitimProgram
                // .Include(ep => ep.Yaratan) // GEÇİCİ OLARAK KAPAT
                .FirstOrDefaultAsync(ep => ep.Id == id);

            if (egitimProgram == null)
            {
                return NotFound();
            }

            // ✨ MANUEL OLARAK YARATAN BİLGİSİNİ YÜKLE
            if (!string.IsNullOrEmpty(egitimProgram.YaratanTc))
            {
                egitimProgram.Yaratan = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == egitimProgram.YaratanTc);
            }

            // Yetki kontrolü - sadece yaratıcı, sorumlu veya yönetici düzenleyebilir
            var currentUserTc = HttpContext.Session.GetString("PersonelTc");
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");

            if (egitimProgram.YaratanTc != currentUserTc &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu programı düzenleme yetkiniz yok.";
                return RedirectToAction("Details", new { id });
            }

            var model = new EgitimProgramViewModel
            {
                Id = egitimProgram.Id,
                EsId = egitimProgram.EsId,
                PerTc = egitimProgram.PerTc,
                Onayli = egitimProgram.Onayli,
                Aciklama = egitimProgram.Aciklama,
                BasTar = egitimProgram.BasTar,
                BitTar = egitimProgram.BitTar
            };

            await FillDropdownLists(model);
            return View(model);
        }
        #endregion

        #region EgitimProgram Düzenleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EgitimProgramViewModel model)
        {
            var egitimProgram = await _context.EgitimProgram
                .FirstOrDefaultAsync(ep => ep.Id == model.Id);

            if (egitimProgram == null)
            {
                return NotFound();
            }

            // ✨ MANUEL OLARAK YARATAN BİLGİSİNİ YÜKLE
            if (!string.IsNullOrEmpty(egitimProgram.YaratanTc))
            {
                egitimProgram.Yaratan = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == egitimProgram.YaratanTc);
            }

            // Yetki kontrolü - sadece yaratıcı, sorumlu veya yönetici düzenleyebilir
            var currentUserTc = HttpContext.Session.GetString("PersonelTc");
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");

            if (egitimProgram.YaratanTc != currentUserTc &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu programı düzenleme yetkiniz yok.";
                return RedirectToAction("Details", new { id = model.Id });
            }

            if (!ModelState.IsValid)
            {
                await FillDropdownLists(model);
                return View(model);
            }

            // Tarih kontrolü
            if (model.BasTar >= model.BitTar)
            {
                ModelState.AddModelError("BitTar", "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
                await FillDropdownLists(model);
                return View(model);
            }

            try
            {
                egitimProgram.EsId = model.EsId;
                egitimProgram.PerTc = model.PerTc;
                egitimProgram.Onayli = model.Onayli;
                egitimProgram.Aciklama = model.Aciklama;
                egitimProgram.BasTar = model.BasTar;
                egitimProgram.BitTar = model.BitTar;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Eğitim programı başarıyla güncellendi.";
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EgitimProgram düzenleme hatası - ID: {Id}", model.Id);
                ModelState.AddModelError("", "Eğitim programı güncellenirken hata oluştu: " + ex.Message);
                await FillDropdownLists(model);
                return View(model);
            }
        }
        #endregion

        #region EgitimProgram Silme (Sadece yaratıcı veya yönetici/sorumlu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // ✨ EXPLICIT LOADING
            var egitimProgram = await _context.EgitimProgram
                // .Include(ep => ep.Yaratan) // GEÇİCİ OLARAK KAPAT
                // .Include(ep => ep.Katilimcilar) // GEÇİCİ OLARAK KAPAT
                .FirstOrDefaultAsync(ep => ep.Id == id);

            if (egitimProgram == null)
            {
                return NotFound();
            }

            // ✨ MANUEL OLARAK YARATAN BİLGİSİNİ YÜKLE
            if (!string.IsNullOrEmpty(egitimProgram.YaratanTc))
            {
                egitimProgram.Yaratan = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == egitimProgram.YaratanTc);
            }

            // Katılımcıları manuel kontrol et
            var katilimcilar = await _context.Egitilen
                .Where(e => e.EgtProgId == id)
                .ToListAsync();

            // Yetki kontrolü - sadece yaratıcı, sorumlu veya yönetici silebilir
            var currentUserTc = HttpContext.Session.GetString("PersonelTc");
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");

            if (egitimProgram.YaratanTc != currentUserTc &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu programı silme yetkiniz yok.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                // Eğer programa bağlı katılımcılar varsa silinemez
                if (katilimcilar.Any())
                {
                    TempData["ErrorMessage"] = "Bu programa bağlı katılımcılar bulunmaktadır. Önce katılımcıları silmelisiniz.";
                    return RedirectToAction("Details", new { id });
                }

                _context.EgitimProgram.Remove(egitimProgram);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Eğitim programı başarıyla silindi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EgitimProgram silme hatası - ID: {Id}", id);
                TempData["ErrorMessage"] = "Eğitim programı silinirken hata oluştu: " + ex.Message;
                return RedirectToAction("Details", new { id });
            }
        }
        #endregion

        #region EgitimProgram Onaylama (Öğretmen ve üstü)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id, int onayDurumu)
        {
            // Yetki kontrolü - öğretmen ve üstü yetkililer
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "ogretmen" && currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Details", new { id });
            }

            var egitimProgram = await _context.EgitimProgram
                .FirstOrDefaultAsync(ep => ep.Id == id);

            if (egitimProgram == null)
            {
                return NotFound();
            }

            try
            {
                egitimProgram.Onayli = onayDurumu;
                await _context.SaveChangesAsync();

                var durumMesaji = onayDurumu == 1 ? "onaylandı" : "reddedildi";
                TempData["SuccessMessage"] = $"Eğitim programı başarıyla {durumMesaji}.";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EgitimProgram onaylama hatası - ID: {Id}, Durum: {OnayDurumu}", id, onayDurumu);
                TempData["ErrorMessage"] = "Eğitim programı onaylanırken hata oluştu: " + ex.Message;
                return RedirectToAction("Details", new { id });
            }
        }
        #endregion

        #region Yardımcı Metod - Dropdown Listelerini Doldur
        private async Task FillDropdownLists(EgitimProgramViewModel model)
        {
            model.EgitimSablonlari = await _context.EgitimSablon
                .Include(es => es.Dershane)
                .Include(es => es.EgitimTip)
                .OrderBy(es => es.Dershane.Ad)
                .ThenBy(es => es.EgitimTip.Ad)
                .ThenBy(es => es.Ad)
                .ToListAsync();

            model.Ogretmenler = await _context.Personel
                .Where(p => p.Tip == "ogretmen" && p.Aktif == 1)
                .OrderBy(p => p.Adlar)
                .ToListAsync();
        }
        #endregion
    }
}