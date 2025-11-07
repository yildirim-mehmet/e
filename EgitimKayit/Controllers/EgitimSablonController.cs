using EgitimKayit.Data;
using EgitimKayit.Models;
using EgitimKayit.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EgitimKayit.Controllers
{
    public class EgitimSablonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EgitimSablonController> _logger;

        public EgitimSablonController(ApplicationDbContext context, ILogger<EgitimSablonController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region EgitimSablon Listesi - Talepçi ve üstü yetkililer görebilir
        [HttpGet]
        public async Task<IActionResult> Index(int? dershaneId, int? egitimTipId)
        {
            // Yetki kontrolü - talepçi ve üstü yetkililer
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            // ✨ LAZY LOADING YERINE EXPLICIT LOADING KULLANALIM
            var egitimSablonlari = _context.EgitimSablon
                .Include(es => es.Dershane)
                .Include(es => es.EgitimTip)
                // .Include(es => es.Yaratan) // ✨ GEÇİCİ OLARAK KAPATIYORUZ
                // .Include(es => es.EgitimProgramlari) // ✨ GEÇİCİ OLARAK KAPATIYORUZ
                .AsQueryable();

            // Dershane filtresi
            if (dershaneId.HasValue)
            {
                egitimSablonlari = egitimSablonlari.Where(es => es.DerId == dershaneId.Value);
            }

            // EgitimTip filtresi
            if (egitimTipId.HasValue)
            {
                egitimSablonlari = egitimSablonlari.Where(es => es.EtId == egitimTipId.Value);
            }

            var sablonListesi = await egitimSablonlari.OrderByDescending(es => es.Tarih).ToListAsync();

            // ✨ MANUEL OLARAK YARATAN BİLGİLERİNİ DOLDURALIM
            foreach (var sablon in sablonListesi)
            {
                if (!string.IsNullOrEmpty(sablon.YaratanTc))
                {
                    sablon.Yaratan = await _context.Personel
                        .FirstOrDefaultAsync(p => p.Tc == sablon.YaratanTc);
                }
            }

            var model = new EgitimSablonIndexViewModel
            {
                EgitimSablonlari = sablonListesi,
                Dershaneler = await _context.Dershane.Where(d => d.Durum == 1).ToListAsync(),
                EgitimTipleri = await _context.EgitimTip
                    .Include(et => et.Dershane)
                    .Where(et => et.Dershane != null && et.Dershane.Durum == 1)
                    .ToListAsync(),
                SeciliDershaneId = dershaneId,
                SeciliEgitimTipId = egitimTipId
            };

            return View(model);
        }
        #endregion

        //#region EgitimSablon Listesi - Talepçi ve üstü yetkililer görebilir
        //[HttpGet]
        //public async Task<IActionResult> Index(int? dershaneId, int? egitimTipId)
        //{
        //    // Yetki kontrolü - talepçi ve üstü yetkililer
        //    var currentUserTip = HttpContext.Session.GetString("PersonelTip");
        //    if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
        //        currentUserTip != "sorumlu" && currentUserTip != "yonetici")
        //    {
        //        TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
        //        return RedirectToAction("Dashboard", "Home");
        //    }

        //    var egitimSablonlari = _context.EgitimSablon
        //        .Include(es => es.Dershane)
        //        .Include(es => es.EgitimTip)
        //        .Include(es => es.Yaratan)
        //        .Include(es => es.EgitimProgramlari)
        //        .AsQueryable();

        //    // Dershane filtresi
        //    if (dershaneId.HasValue)
        //    {
        //        egitimSablonlari = egitimSablonlari.Where(es => es.DerId == dershaneId.Value);
        //    }

        //    // EgitimTip filtresi
        //    if (egitimTipId.HasValue)
        //    {
        //        egitimSablonlari = egitimSablonlari.Where(es => es.EtId == egitimTipId.Value);
        //    }

        //    var model = new EgitimSablonIndexViewModel
        //    {
        //        EgitimSablonlari = await egitimSablonlari.OrderByDescending(es => es.Tarih).ToListAsync(),
        //        Dershaneler = await _context.Dershane.Where(d => d.Durum == 1).ToListAsync(),
        //        EgitimTipleri = await _context.EgitimTip
        //            .Include(et => et.Dershane)
        //            .Where(et => et.Dershane != null && et.Dershane.Durum == 1)
        //            .ToListAsync(),
        //        SeciliDershaneId = dershaneId,
        //        SeciliEgitimTipId = egitimTipId
        //    };

        //    return View(model);
        //}
        //#endregion

        #region EgitimSablon Oluşturma - GET (Talepçi ve üstü)
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

            var model = new EgitimSablonViewModel();
            await FillDropdownLists(model);
            return View(model);
        }
        #endregion

        #region EgitimSablon Oluşturma - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EgitimSablonViewModel model)
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

            try
            {
                // Aynı isimde şablon kontrolü (aynı eğitim tipi içinde)
                if (await _context.EgitimSablon.AnyAsync(es => es.EtId == model.EtId && es.Ad == model.Ad))
                {
                    ModelState.AddModelError("Ad", "Bu eğitim tipinde aynı isimde şablon bulunmaktadır.");
                    await FillDropdownLists(model);
                    return View(model);
                }

                var yeniSablon = new EgitimSablon
                {
                    Ad = model.Ad,
                    DerId = model.DerId,
                    EtId = model.EtId,
                    Aciklama = model.Aciklama,
                    YaratanTc = HttpContext.Session.GetString("PersonelTc"),
                    Tarih = DateTime.Now
                };

                _context.EgitimSablon.Add(yeniSablon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Eğitim şablonu başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EgitimSablon oluşturma hatası - Ad: {Ad}, EgitimTipId: {EtId}", model.Ad, model.EtId);
                ModelState.AddModelError("", "Eğitim şablonu oluşturulurken hata oluştu: " + ex.Message);
                await FillDropdownLists(model);
                return View(model);
            }
        }
        #endregion


        #region EgitimSablon Detay Görüntüleme
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

            // ✨ EXPLICIT LOADING - Sadece gerekli navigation'ları yükle
            var egitimSablon = await _context.EgitimSablon
                .Include(es => es.Dershane)
                .Include(es => es.EgitimTip)
                // .Include(es => es.Yaratan) // GEÇİCİ OLARAK KAPAT
                // .Include(es => es.EgitimProgramlari) // GEÇİCİ OLARAK KAPAT
                .FirstOrDefaultAsync(es => es.Id == id);

            if (egitimSablon == null)
            {
                return NotFound();
            }

            // ✨ MANUEL OLARAK YARATAN BİLGİSİNİ YÜKLE
            if (!string.IsNullOrEmpty(egitimSablon.YaratanTc))
            {
                egitimSablon.Yaratan = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == egitimSablon.YaratanTc);
            }

            return View(egitimSablon);
        }
        #endregion

        #region EgitimSablon Düzenleme - GET (Sadece yaratıcı veya yönetici/sorumlu)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // ✨ EXPLICIT LOADING
            var egitimSablon = await _context.EgitimSablon
                // .Include(es => es.Yaratan) // GEÇİCİ OLARAK KAPAT
                .FirstOrDefaultAsync(es => es.Id == id);

            if (egitimSablon == null)
            {
                return NotFound();
            }

            // ✨ MANUEL OLARAK YARATAN BİLGİSİNİ YÜKLE
            if (!string.IsNullOrEmpty(egitimSablon.YaratanTc))
            {
                egitimSablon.Yaratan = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == egitimSablon.YaratanTc);
            }

            // Yetki kontrolü - sadece yaratıcı, sorumlu veya yönetici düzenleyebilir
            var currentUserTc = HttpContext.Session.GetString("PersonelTc");
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");

            if (egitimSablon.YaratanTc != currentUserTc &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu şablonu düzenleme yetkiniz yok.";
                return RedirectToAction("Details", new { id });
            }

            var model = new EgitimSablonViewModel
            {
                Id = egitimSablon.Id,
                DerId = egitimSablon.DerId,
                EtId = egitimSablon.EtId,
                Ad = egitimSablon.Ad,
                Aciklama = egitimSablon.Aciklama
            };

            await FillDropdownLists(model);
            return View(model);
        }
        #endregion

        #region EgitimSablon Düzenleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EgitimSablonViewModel model)
        {
            var egitimSablon = await _context.EgitimSablon
                .FirstOrDefaultAsync(es => es.Id == model.Id);

            if (egitimSablon == null)
            {
                return NotFound();
            }

            // Yetki kontrolü - sadece yaratıcı, sorumlu veya yönetici düzenleyebilir
            var currentUserTc = HttpContext.Session.GetString("PersonelTc");
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");

            if (egitimSablon.YaratanTc != currentUserTc &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu şablonu düzenleme yetkiniz yok.";
                return RedirectToAction("Details", new { id = model.Id });
            }

            if (!ModelState.IsValid)
            {
                await FillDropdownLists(model);
                return View(model);
            }

            try
            {
                // Aynı isimde başka şablon kontrolü (kendisi hariç)
                if (await _context.EgitimSablon.AnyAsync(es => es.EtId == model.EtId && es.Ad == model.Ad && es.Id != model.Id))
                {
                    ModelState.AddModelError("Ad", "Bu eğitim tipinde aynı isimde şablon bulunmaktadır.");
                    await FillDropdownLists(model);
                    return View(model);
                }

                egitimSablon.DerId = model.DerId;
                egitimSablon.EtId = model.EtId;
                egitimSablon.Ad = model.Ad;
                egitimSablon.Aciklama = model.Aciklama;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Eğitim şablonu başarıyla güncellendi.";
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EgitimSablon düzenleme hatası - ID: {Id}, Ad: {Ad}", model.Id, model.Ad);
                ModelState.AddModelError("", "Eğitim şablonu güncellenirken hata oluştu: " + ex.Message);
                await FillDropdownLists(model);
                return View(model);
            }
        }
        #endregion

        #region EgitimSablon Silme (Sadece yaratıcı veya yönetici/sorumlu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // ✨ EXPLICIT LOADING
            var egitimSablon = await _context.EgitimSablon
                // .Include(es => es.Yaratan) // GEÇİCİ OLARAK KAPAT
                // .Include(es => es.EgitimProgramlari) // GEÇİCİ OLARAK KAPAT
                .FirstOrDefaultAsync(es => es.Id == id);

            if (egitimSablon == null)
            {
                return NotFound();
            }

            // ✨ MANUEL OLARAK YARATAN BİLGİSİNİ YÜKLE
            if (!string.IsNullOrEmpty(egitimSablon.YaratanTc))
            {
                egitimSablon.Yaratan = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == egitimSablon.YaratanTc);
            }

            // Eğitim programlarını manuel kontrol et
            var egitimProgramlari = await _context.EgitimProgram
                .Where(ep => ep.EsId == id)
                .ToListAsync();

            // Yetki kontrolü - sadece yaratıcı, sorumlu veya yönetici silebilir
            var currentUserTc = HttpContext.Session.GetString("PersonelTc");
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");

            if (egitimSablon.YaratanTc != currentUserTc &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu şablonu silme yetkiniz yok.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                // Eğer şablona bağlı eğitim programları varsa silinemez
                if (egitimProgramlari.Any())
                {
                    TempData["ErrorMessage"] = "Bu şablona bağlı eğitim programları bulunmaktadır. Önce eğitim programlarını silmelisiniz.";
                    return RedirectToAction("Details", new { id });
                }

                _context.EgitimSablon.Remove(egitimSablon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Eğitim şablonu başarıyla silindi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EgitimSablon silme hatası - ID: {Id}", id);
                TempData["ErrorMessage"] = "Eğitim şablonu silinirken hata oluştu: " + ex.Message;
                return RedirectToAction("Details", new { id });
            }
        }
        #endregion



     

        #region EgitimSablon Düzenleme - GET (Sadece yaratıcı veya yönetici/sorumlu)
        [HttpGet]
        
        #endregion

        

        




        #region AJAX - Dershaneye göre eğitim tiplerini getir
        [HttpGet]
        public async Task<JsonResult> GetEgitimTipleriByDershane(int dershaneId)
        {
            var egitimTipleri = await _context.EgitimTip
                .Where(et => et.DerId == dershaneId)
                .OrderBy(et => et.Ad)
                .Select(et => new { id = et.Id, ad = et.Ad })
                .ToListAsync();

            return Json(egitimTipleri);
        }
        #endregion

        #region Yardımcı Metod - Dropdown Listelerini Doldur
        private async Task FillDropdownLists(EgitimSablonViewModel model)
        {
            model.Dershaneler = await _context.Dershane
                .Where(d => d.Durum == 1)
                .OrderBy(d => d.Ad)
                .ToListAsync();

            // Eğer dershane seçilmişse, o dershaneye ait eğitim tiplerini de getir
            if (model.DerId > 0)
            {
                model.EgitimTipleri = await _context.EgitimTip
                    .Where(et => et.DerId == model.DerId)
                    .OrderBy(et => et.Ad)
                    .ToListAsync();
            }
            else
            {
                model.EgitimTipleri = new List<EgitimTip>();
            }
        }
        #endregion
    }
}


//using EgitimKayit.Data;
//using EgitimKayit.Models;
//using EgitimKayit.ViewModels;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//namespace EgitimKayit.Controllers
//{
//    public class EgitimSablonController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ILogger<EgitimSablonController> _logger;

//        public EgitimSablonController(ApplicationDbContext context, ILogger<EgitimSablonController> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        #region EgitimSablon Listesi - Talepçi ve üstü yetkililer görebilir
//        [HttpGet]
//        public async Task<IActionResult> Index(int? dershaneId, int? egitimTipId)
//        {
//            //dershaneId = 1; //test
//            //egitimTipId= 1; //test 
//            // Yetki kontrolü - talepçi ve üstü yetkililer
//            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
//            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
//                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
//            {
//                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
//                return RedirectToAction("Dashboard", "Home");
//            }

//            var egitimSablonlari = _context.EgitimSablon
//                .Include(es => es.Dershane)
//                .Include(es => es.EgitimTip)
//               // .Include(es => es.Yaratan)
//                .Include(es => es.EgitimProgramlari)
//                .AsQueryable();

//            // Dershane filtresi
//            if (dershaneId.HasValue)
//            {
//                egitimSablonlari = egitimSablonlari.Where(es => es.DerId == dershaneId.Value);
//            }

//            // EgitimTip filtresi
//            if (egitimTipId.HasValue)
//            {
//                egitimSablonlari = egitimSablonlari.Where(es => es.EtId == egitimTipId.Value);
//            }

//            var model = new EgitimSablonIndexViewModel
//            {
//                EgitimSablonlari = await egitimSablonlari.OrderByDescending(es => es.Tarih).ToListAsync(),
//                Dershaneler = await _context.Dershane.Where(d => d.Durum == 1).ToListAsync(),
//                EgitimTipleri = await _context.EgitimTip
//                    .Include(et => et.Dershane)
//                    .Where(et => et.Dershane != null && et.Dershane.Durum == 1)
//                    .ToListAsync(),
//                SeciliDershaneId = dershaneId,
//                SeciliEgitimTipId = egitimTipId
//            };

//            return View(model);
//        }
//        #endregion

//        #region EgitimSablon Oluşturma - GET (Talepçi ve üstü)
//        [HttpGet]
//        public async Task<IActionResult> Create()
//        {
//            // Yetki kontrolü - talepçi ve üstü yetkililer
//            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
//            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
//                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
//            {
//                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
//                return RedirectToAction("Index");
//            }

//            var model = new EgitimSablonViewModel();
//            await FillDropdownLists(model);
//            return View(model);
//        }
//        #endregion

//        #region EgitimSablon Oluşturma - POST
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(EgitimSablonViewModel model)
//        {
//            // Yetki kontrolü - talepçi ve üstü yetkililer
//            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
//            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
//                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
//            {
//                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
//                return RedirectToAction("Index");
//            }

//            if (!ModelState.IsValid)
//            {
//                await FillDropdownLists(model);
//                return View(model);
//            }

//            try
//            {
//                // Aynı isimde şablon kontrolü (aynı eğitim tipi içinde)
//                if (await _context.EgitimSablon.AnyAsync(es => es.EtId == model.EtId && es.Ad == model.Ad))
//                {
//                    ModelState.AddModelError("Ad", "Bu eğitim tipinde aynı isimde şablon bulunmaktadır.");
//                    await FillDropdownLists(model);
//                    return View(model);
//                }

//                var yeniSablon = new EgitimSablon
//                {
//                    Ad = model.Ad,
//                    DerId = model.DerId,
//                    EtId = model.EtId,
//                    Aciklama = model.Aciklama,
//                    YaratanTc = HttpContext.Session.GetString("PersonelTc"),
//                    Tarih = DateTime.Now
//                };

//                _context.EgitimSablon.Add(yeniSablon);
//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Eğitim şablonu başarıyla oluşturuldu.";
//                return RedirectToAction("Index");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "EgitimSablon oluşturma hatası - Ad: {Ad}, EgitimTipId: {EtId}", model.Ad, model.EtId);
//                ModelState.AddModelError("", "Eğitim şablonu oluşturulurken hata oluştu: " + ex.Message);
//                await FillDropdownLists(model);
//                return View(model);
//            }
//        }
//        #endregion

//        #region EgitimSablon Detay Görüntüleme
//        [HttpGet]
//        public async Task<IActionResult> Details(int id)
//        {
//            // Yetki kontrolü - talepçi ve üstü yetkililer
//            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
//            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
//                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
//            {
//                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
//                return RedirectToAction("Dashboard", "Home");
//            }

//            var egitimSablon = await _context.EgitimSablon
//                .Include(es => es.Dershane)
//                .Include(es => es.EgitimTip)
//                .Include(es => es.Yaratan)
//                .Include(es => es.EgitimProgramlari)
//                    .ThenInclude(ep => ep.Katilimcilar)
//                .FirstOrDefaultAsync(es => es.Id == id);

//            if (egitimSablon == null)
//            {
//                return NotFound();
//            }

//            return View(egitimSablon);
//        }
//        #endregion

//        #region EgitimSablon Düzenleme - GET (Sadece yaratıcı veya yönetici/sorumlu)
//        [HttpGet]
//        public async Task<IActionResult> Edit(int id)
//        {
//            var egitimSablon = await _context.EgitimSablon
//                .Include(es => es.Yaratan)
//                .FirstOrDefaultAsync(es => es.Id == id);

//            if (egitimSablon == null)
//            {
//                return NotFound();
//            }

//            // Yetki kontrolü - sadece yaratıcı, sorumlu veya yönetici düzenleyebilir
//            var currentUserTc = HttpContext.Session.GetString("PersonelTc");
//            var currentUserTip = HttpContext.Session.GetString("PersonelTip");

//            if (egitimSablon.YaratanTc != currentUserTc &&
//                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
//            {
//                TempData["ErrorMessage"] = "Bu şablonu düzenleme yetkiniz yok.";
//                return RedirectToAction("Details", new { id });
//            }

//            var model = new EgitimSablonViewModel
//            {
//                Id = egitimSablon.Id,
//                DerId = egitimSablon.DerId,
//                EtId = egitimSablon.EtId,
//                Ad = egitimSablon.Ad,
//                Aciklama = egitimSablon.Aciklama
//            };

//            await FillDropdownLists(model);
//            return View(model);
//        }
//        #endregion

//        #region EgitimSablon Düzenleme - POST
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(EgitimSablonViewModel model)
//        {
//            var egitimSablon = await _context.EgitimSablon
//                .FirstOrDefaultAsync(es => es.Id == model.Id);

//            if (egitimSablon == null)
//            {
//                return NotFound();
//            }

//            // Yetki kontrolü - sadece yaratıcı, sorumlu veya yönetici düzenleyebilir
//            var currentUserTc = HttpContext.Session.GetString("PersonelTc");
//            var currentUserTip = HttpContext.Session.GetString("PersonelTip");

//            if (egitimSablon.YaratanTc != currentUserTc &&
//                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
//            {
//                TempData["ErrorMessage"] = "Bu şablonu düzenleme yetkiniz yok.";
//                return RedirectToAction("Details", new { id = model.Id });
//            }

//            if (!ModelState.IsValid)
//            {
//                await FillDropdownLists(model);
//                return View(model);
//            }

//            try
//            {
//                // Aynı isimde başka şablon kontrolü (kendisi hariç)
//                if (await _context.EgitimSablon.AnyAsync(es => es.EtId == model.EtId && es.Ad == model.Ad && es.Id != model.Id))
//                {
//                    ModelState.AddModelError("Ad", "Bu eğitim tipinde aynı isimde şablon bulunmaktadır.");
//                    await FillDropdownLists(model);
//                    return View(model);
//                }

//                egitimSablon.DerId = model.DerId;
//                egitimSablon.EtId = model.EtId;
//                egitimSablon.Ad = model.Ad;
//                egitimSablon.Aciklama = model.Aciklama;

//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Eğitim şablonu başarıyla güncellendi.";
//                return RedirectToAction("Details", new { id = model.Id });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "EgitimSablon düzenleme hatası - ID: {Id}, Ad: {Ad}", model.Id, model.Ad);
//                ModelState.AddModelError("", "Eğitim şablonu güncellenirken hata oluştu: " + ex.Message);
//                await FillDropdownLists(model);
//                return View(model);
//            }
//        }
//        #endregion

//        #region EgitimSablon Silme (Sadece yaratıcı veya yönetici/sorumlu)
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var egitimSablon = await _context.EgitimSablon
//                .Include(es => es.Yaratan)
//                .Include(es => es.EgitimProgramlari)
//                .FirstOrDefaultAsync(es => es.Id == id);

//            if (egitimSablon == null)
//            {
//                return NotFound();
//            }

//            // Yetki kontrolü - sadece yaratıcı, sorumlu veya yönetici silebilir
//            var currentUserTc = HttpContext.Session.GetString("PersonelTc");
//            var currentUserTip = HttpContext.Session.GetString("PersonelTip");

//            if (egitimSablon.YaratanTc != currentUserTc &&
//                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
//            {
//                TempData["ErrorMessage"] = "Bu şablonu silme yetkiniz yok.";
//                return RedirectToAction("Details", new { id });
//            }

//            try
//            {
//                // Eğer şablona bağlı eğitim programları varsa silinemez
//                if (egitimSablon.EgitimProgramlari != null && egitimSablon.EgitimProgramlari.Any())
//                {
//                    TempData["ErrorMessage"] = "Bu şablona bağlı eğitim programları bulunmaktadır. Önce eğitim programlarını silmelisiniz.";
//                    return RedirectToAction("Details", new { id });
//                }

//                _context.EgitimSablon.Remove(egitimSablon);
//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Eğitim şablonu başarıyla silindi.";
//                return RedirectToAction("Index");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "EgitimSablon silme hatası - ID: {Id}", id);
//                TempData["ErrorMessage"] = "Eğitim şablonu silinirken hata oluştu: " + ex.Message;
//                return RedirectToAction("Details", new { id });
//            }
//        }
//        #endregion

//        #region AJAX - Dershaneye göre eğitim tiplerini getir
//        [HttpGet]
//        public async Task<JsonResult> GetEgitimTipleriByDershane(int dershaneId)
//        {
//            var egitimTipleri = await _context.EgitimTip
//                .Where(et => et.DerId == dershaneId)
//                .OrderBy(et => et.Ad)
//                .Select(et => new { id = et.Id, ad = et.Ad })
//                .ToListAsync();

//            return Json(egitimTipleri);
//        }
//        #endregion

//        #region Yardımcı Metod - Dropdown Listelerini Doldur
//        private async Task FillDropdownLists(EgitimSablonViewModel model)
//        {
//            model.Dershaneler = await _context.Dershane
//                .Where(d => d.Durum == 1)
//                .OrderBy(d => d.Ad)
//                .ToListAsync();

//            // Eğer dershane seçilmişse, o dershaneye ait eğitim tiplerini de getir
//            if (model.DerId > 0)
//            {
//                model.EgitimTipleri = await _context.EgitimTip
//                    .Where(et => et.DerId == model.DerId)
//                    .OrderBy(et => et.Ad)
//                    .ToListAsync();
//            }
//            else
//            {
//                model.EgitimTipleri = new List<EgitimTip>();
//            }
//        }
//        #endregion
//    }
//}