using EgitimKayit.Data;
using EgitimKayit.Models;
using EgitimKayit.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Drawing;
using ClosedXML.Excel; // ClosedXML için

namespace EgitimKayit.Controllers
{
    public class EgitilenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EgitilenController> _logger;

        public EgitilenController(ApplicationDbContext context, ILogger<EgitilenController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Egitilen Listesi - Eğitim Programına göre
        [HttpGet]
        public async Task<IActionResult> Index(int egitimProgramId)
        {
            // Yetki kontrolü - talepçi ve üstü yetkililer
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Dashboard", "Home");
            }

            var egitimProgram = await _context.EgitimProgram
                .Include(ep => ep.EgitimSablon)
                    .ThenInclude(es => es.Dershane)
                .Include(ep => ep.EgitimSablon)
                    .ThenInclude(es => es.EgitimTip)
                .Include(ep => ep.Ogretmen)
                .FirstOrDefaultAsync(ep => ep.Id == egitimProgramId);

            if (egitimProgram == null)
            {
                TempData["ErrorMessage"] = "Eğitim programı bulunamadı.";
                return RedirectToAction("Index", "EgitimProgram");
            }

            var egitilenler = await _context.Egitilen
                .Include(e => e.Katilimci)
                .Include(e => e.Ogretmen)
                .Where(e => e.EgtProgId == egitimProgramId)
                .OrderBy(e => e.Katilimci.Adlar)
                .ToListAsync();

            var personeller = await _context.Personel
                .Where(p => p.Aktif == 1)
                .OrderBy(p => p.Adlar)
                .ToListAsync();

            var model = new EgitilenIndexViewModel
            {
                EgitimProgramId = egitimProgramId,
                EgitimProgram = egitimProgram,
                Egitilenler = egitilenler,
                Personeller = personeller
            };

            return View(model);
        }
        #endregion

        #region Personel Ekleme - GET
        [HttpGet]
        #region Personel Ekleme - GET
        [HttpGet]
        public async Task<IActionResult> Create(int egitimProgramId)
        {
            // Yetki kontrolü - talepçi ve üstü yetkililer
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index", "EgitimProgram");
            }

            var egitimProgram = await _context.EgitimProgram
                .Include(ep => ep.EgitimSablon)
                .FirstOrDefaultAsync(ep => ep.Id == egitimProgramId);

            if (egitimProgram == null)
            {
                TempData["ErrorMessage"] = "Eğitim programı bulunamadı.";
                return RedirectToAction("Index", "EgitimProgram");
            }

            var personeller = await _context.Personel
                .Where(p => p.Aktif == 1)
                .OrderBy(p => p.Adlar)
                .ToListAsync();

            var model = new EgitilenViewModel
            {
                EgtProgId = egitimProgramId,
                Personeller = personeller
            };

            return View(model);
        }
        #endregion

        #region Personel Ekleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EgitilenViewModel model)
        {
            // Yetki kontrolü - talepçi ve üstü yetkililer
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index", "EgitimProgram");
            }

            if (!ModelState.IsValid)
            {
                model.Personeller = await _context.Personel
                    .Where(p => p.Aktif == 1)
                    .OrderBy(p => p.Adlar)
                    .ToListAsync();
                return View(model);
            }

            try
            {
                // Aynı kişi aynı eğitim programına zaten eklenmiş mi kontrol et
                if (await _context.Egitilen.AnyAsync(e => e.PerTc == model.PerTc && e.EgtProgId == model.EgtProgId))
                {
                    ModelState.AddModelError("PerTc", "Bu personel zaten bu eğitim programına eklenmiş.");
                    model.Personeller = await _context.Personel
                        .Where(p => p.Aktif == 1)
                        .OrderBy(p => p.Adlar)
                        .ToListAsync();
                    return View(model);
                }

                // Personelin var olduğunu kontrol et
                var personel = await _context.Personel
                    .FirstOrDefaultAsync(p => p.Tc == model.PerTc && p.Aktif == 1);

                if (personel == null)
                {
                    ModelState.AddModelError("PerTc", "Geçerli bir personel seçiniz.");
                    model.Personeller = await _context.Personel
                        .Where(p => p.Aktif == 1)
                        .OrderBy(p => p.Adlar)
                        .ToListAsync();
                    return View(model);
                }

                var yeniEgitilen = new Egitilen
                {
                    PerTc = model.PerTc,
                    EgtProgId = model.EgtProgId,
                    Dk = model.Dk,
                    BasTar = model.BasTar,
                    BitTar = model.BitTar,
                    Aciklama = model.Aciklama,
                    Yapildi = 0, // Varsayılan olarak yapılmadı
                    YaratanTc = HttpContext.Session.GetString("PersonelTc"),
                    Tarih = DateTime.Now
                };

                _context.Egitilen.Add(yeniEgitilen);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Personel eğitim programına başarıyla eklendi.";
                return RedirectToAction("Index", new { egitimProgramId = model.EgtProgId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Egitilen oluşturma hatası - TC: {PerTc}, ProgramId: {EgtProgId}", model.PerTc, model.EgtProgId);
                ModelState.AddModelError("", "Personel eklenirken hata oluştu: " + ex.Message);
                model.Personeller = await _context.Personel
                    .Where(p => p.Aktif == 1)
                    .OrderBy(p => p.Adlar)
                    .ToListAsync();
                return View(model);
            }
        }
        #endregion
        //public async Task<IActionResult> Create(int egitimProgramId)
        //{
        //    // Yetki kontrolü - talepçi ve üstü yetkililer
        //    var currentUserTip = HttpContext.Session.GetString("PersonelTip");
        //    if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
        //        currentUserTip != "sorumlu" && currentUserTip != "yonetici")
        //    {
        //        TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
        //        return RedirectToAction("Index", "EgitimProgram");
        //    }

        //    var egitimProgram = await _context.EgitimProgram
        //        .Include(ep => ep.EgitimSablon)
        //        .FirstOrDefaultAsync(ep => ep.Id == egitimProgramId);

        //    if (egitimProgram == null)
        //    {
        //        TempData["ErrorMessage"] = "Eğitim programı bulunamadı.";
        //        return RedirectToAction("Index", "EgitimProgram");
        //    }

        //    var personeller = await _context.Personel
        //        .Where(p => p.Aktif == 1)
        //        .OrderBy(p => p.Adlar)
        //        .ToListAsync();

        //    var model = new EgitilenViewModel
        //    {
        //        EgtProgId = egitimProgramId,
        //        Personeller = personeller
        //    };

        //    return View(model);
        //}
        #endregion

        #region Personel Ekleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]

        //public async Task<IActionResult> Create(EgitilenViewModel model)
        //{
        //    // Yetki kontrolü - talepçi ve üstü yetkililer
        //    var currentUserTip = HttpContext.Session.GetString("PersonelTip");
        //    if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
        //        currentUserTip != "sorumlu" && currentUserTip != "yonetici")
        //    {
        //        TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
        //        return RedirectToAction("Index", "EgitimProgram");
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        model.Personeller = await _context.Personel
        //            .Where(p => p.Aktif == 1)
        //            .OrderBy(p => p.Adlar)
        //            .ToListAsync();
        //        return View(model);
        //    }

        //    try
        //    {
        //        // Aynı kişi aynı eğitim programına zaten eklenmiş mi kontrol et
        //        if (await _context.Egitilen.AnyAsync(e => e.PerTc == model.PerTc && e.EgtProgId == model.EgtProgId))
        //        {
        //            ModelState.AddModelError("PerTc", "Bu personel zaten bu eğitim programına eklenmiş.");
        //            model.Personeller = await _context.Personel
        //                .Where(p => p.Aktif == 1)
        //                .OrderBy(p => p.Adlar)
        //                .ToListAsync();
        //            return View(model);
        //        }

        //        // Personelin var olduğunu kontrol et
        //        var personel = await _context.Personel
        //            .FirstOrDefaultAsync(p => p.Tc == model.PerTc && p.Aktif == 1);

        //        if (personel == null)
        //        {
        //            ModelState.AddModelError("PerTc", "Geçerli bir personel seçiniz.");
        //            model.Personeller = await _context.Personel
        //                .Where(p => p.Aktif == 1)
        //                .OrderBy(p => p.Adlar)
        //                .ToListAsync();
        //            return View(model);
        //        }

        //        var yeniEgitilen = new Egitilen
        //        {
        //            PerTc = model.PerTc,
        //            EgtProgId = model.EgtProgId,
        //            Dk = model.Dk,
        //            BasTar = model.BasTar,
        //            BitTar = model.BitTar,
        //            Aciklama = model.Aciklama,
        //            Yapildi = 0, // Varsayılan olarak yapılmadı
        //            YaratanTc = HttpContext.Session.GetString("PersonelTc"),
        //            Tarih = DateTime.Now
        //        };

        //        _context.Egitilen.Add(yeniEgitilen);
        //        await _context.SaveChangesAsync();

        //        TempData["SuccessMessage"] = "Personel eğitim programına başarıyla eklendi.";
        //        return RedirectToAction("Index", new { egitimProgramId = model.EgtProgId });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Egitilen oluşturma hatası - TC: {PerTc}, ProgramId: {EgtProgId}", model.PerTc, model.EgtProgId);
        //        ModelState.AddModelError("", "Personel eklenirken hata oluştu: " + ex.Message);
        //        model.Personeller = await _context.Personel
        //            .Where(p => p.Aktif == 1)
        //            .OrderBy(p => p.Adlar)
        //            .ToListAsync();
        //        return View(model);
        //    }
        //}
        #endregion

        #region Toplu Personel Ekleme - GET



        //using ClosedXML.Excel; // ClosedXML için

#region Excel Yükleme - GET
[HttpGet]
    public IActionResult ExcelUpload(int egitimProgramId)
    {
        // Yetki kontrolü - talepçi ve üstü yetkililer
        var currentUserTip = HttpContext.Session.GetString("PersonelTip");
        if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
            currentUserTip != "sorumlu" && currentUserTip != "yonetici")
        {
            TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
            return RedirectToAction("Index", "EgitimProgram");
        }

        var model = new ExcelUploadViewModel
        {
            EgitimProgramId = egitimProgramId
        };

        return View(model);
    }
    #endregion

    #region Excel Yükleme - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcelUpload(ExcelUploadViewModel model)
    {
        // Yetki kontrolü - talepçi ve üstü yetkililer
        var currentUserTip = HttpContext.Session.GetString("PersonelTip");
        if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
            currentUserTip != "sorumlu" && currentUserTip != "yonetici")
        {
            TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
            return RedirectToAction("Index", "EgitimProgram");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Excel dosyasını işle
            var result = await ProcessExcelFile(model.ExcelFile, model.EgitimProgramId, model.FirstRowIsHeader);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Excel dosyası başarıyla işlendi. {result.AddedCount} yeni kayıt eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Excel işleme hatası: {result.ErrorMessage}";
            }

            return RedirectToAction("Index", new { egitimProgramId = model.EgitimProgramId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excel yükleme hatası - ProgramId: {EgitimProgramId}", model.EgitimProgramId);
            TempData["ErrorMessage"] = "Excel dosyası yüklenirken hata oluştu: " + ex.Message;
            return View(model);
        }
    }
    #endregion

    #region Excel'e Aktar
    [HttpGet]
    public async Task<IActionResult> ExportExcel(int egitimProgramId)
    {
        var egitilenler = await _context.Egitilen
            .Include(e => e.Katilimci)
            .Include(e => e.Katilimci.StatuBilgi)
            .Where(e => e.EgtProgId == egitimProgramId)
            .OrderBy(e => e.Katilimci.Adlar)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Katılımcılar");

        // Başlıklar
        worksheet.Cell(1, 1).Value = "TC Kimlik No";
        worksheet.Cell(1, 2).Value = "Ad Soyad";
        worksheet.Cell(1, 3).Value = "Statü";
        worksheet.Cell(1, 4).Value = "Birim 1";
        worksheet.Cell(1, 5).Value = "Birim 2";
        worksheet.Cell(1, 6).Value = "Birim 3";
        worksheet.Cell(1, 7).Value = "Dakika";
        worksheet.Cell(1, 8).Value = "Durum";
        worksheet.Cell(1, 9).Value = "Ekleme Tarihi";

        // Başlık stili
        var headerRange = worksheet.Range(1, 1, 1, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Veriler
        for (int i = 0; i < egitilenler.Count; i++)
        {
            var katilimci = egitilenler[i];
            worksheet.Cell(i + 2, 1).Value = katilimci.Katilimci?.Tc;
            worksheet.Cell(i + 2, 2).Value = katilimci.Katilimci?.Adlar;
            worksheet.Cell(i + 2, 3).Value = katilimci.Katilimci?.StatuBilgi?.Anlam;
            worksheet.Cell(i + 2, 4).Value = katilimci.Katilimci?.Birim1;
            worksheet.Cell(i + 2, 5).Value = katilimci.Katilimci?.Birim2;
            worksheet.Cell(i + 2, 6).Value = katilimci.Katilimci?.Birim3;
            worksheet.Cell(i + 2, 7).Value = katilimci.Dk;
            worksheet.Cell(i + 2, 8).Value = katilimci.Yapildi == 1 ? "Tamamlandı" : "Devam Ediyor";
            worksheet.Cell(i + 2, 9).Value = katilimci.Tarih?.ToString("dd.MM.yyyy HH:mm");
        }

        // Sütun genişliklerini ayarla
        worksheet.Columns().AdjustToContents();

        var fileName = $"Katilimcilar_{egitimProgramId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
    #endregion

    #region Excel Şablon İndir
    [HttpGet]
    public IActionResult DownloadTemplate(int egitimProgramId)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Şablon");

        // Başlıklar
        worksheet.Cell(1, 1).Value = "TC Kimlik No";
        worksheet.Cell(1, 2).Value = "Ad Soyad (Opsiyonel)";
        worksheet.Cell(1, 3).Value = "Statü (Opsiyonel)";

        // Örnek veriler
        worksheet.Cell(2, 1).Value = "12345678901";
        worksheet.Cell(2, 2).Value = "Ahmet Yılmaz";
        worksheet.Cell(2, 3).Value = "Öğretmen";

        worksheet.Cell(3, 1).Value = "98765432109";
        worksheet.Cell(3, 2).Value = "Ayşe Demir";
        worksheet.Cell(3, 3).Value = "Öğrenci";

        // Stil
        var headerRange = worksheet.Range(1, 1, 1, 3);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;

        worksheet.Columns().AdjustToContents();

        var fileName = $"Katilimci_Sablon_{egitimProgramId}.xlsx";
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
    #endregion

    #region Excel İşleme Yardımcı Metodu (ClosedXML)
    private async Task<(bool Success, int AddedCount, string ErrorMessage)> ProcessExcelFile(
        IFormFile excelFile, int egitimProgramId, bool firstRowIsHeader)
    {
        var addedCount = 0;
        var yaratanTc = HttpContext.Session.GetString("PersonelTc");

        using var stream = excelFile.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        var rows = worksheet.RangeUsed().RowsUsed();
        var startRow = firstRowIsHeader ? 2 : 1;

        for (int row = startRow; row <= rows.Count(); row++)
        {
            var tcCell = worksheet.Cell(row, 1).Value.ToString();

            if (string.IsNullOrEmpty(tcCell))
                continue;

            // Personelin var olduğunu kontrol et
            var personel = await _context.Personel
                .FirstOrDefaultAsync(p => p.Tc == tcCell && p.Aktif == 1);

            if (personel == null)
            {
                _logger.LogWarning("Excel yükleme: TC {Tc} bulunamadı", tcCell);
                continue;
            }

            // Aynı kişi aynı eğitim programına zaten eklenmiş mi kontrol et
            if (!await _context.Egitilen.AnyAsync(e => e.PerTc == tcCell && e.EgtProgId == egitimProgramId))
            {
                var yeniEgitilen = new Egitilen
                {
                    PerTc = tcCell,
                    EgtProgId = egitimProgramId,
                    Dk = null,
                    Yapildi = 0,
                    YaratanTc = yaratanTc,
                    Tarih = DateTime.Now
                };

                _context.Egitilen.Add(yeniEgitilen);
                addedCount++;
            }
        }

        await _context.SaveChangesAsync();
        return (true, addedCount, "");
    }
    #endregion



    [HttpGet]
        public async Task<IActionResult> CreateMultiple(int egitimProgramId)
        {
            // Yetki kontrolü - talepçi ve üstü yetkililer
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index", "EgitimProgram");
            }

            var egitimProgram = await _context.EgitimProgram
                .Include(ep => ep.EgitimSablon)
                .FirstOrDefaultAsync(ep => ep.Id == egitimProgramId);

            if (egitimProgram == null)
            {
                TempData["ErrorMessage"] = "Eğitim programı bulunamadı.";
                return RedirectToAction("Index", "EgitimProgram");
            }

            var personeller = await _context.Personel
                .Where(p => p.Aktif == 1)
                .OrderBy(p => p.Adlar)
                .ToListAsync();

            ViewBag.EgitimProgram = egitimProgram;
            ViewBag.Personeller = personeller;

            return View();
        }
        #endregion

        #region Toplu Personel Ekleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMultiple(int egitimProgramId, List<string> selectedPersoneller)
        {
            // Yetki kontrolü - talepçi ve üstü yetkililer
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index", "EgitimProgram");
            }

            if (selectedPersoneller == null || !selectedPersoneller.Any())
            {
                TempData["ErrorMessage"] = "En az bir personel seçmelisiniz.";
                return RedirectToAction("CreateMultiple", new { egitimProgramId });
            }

            try
            {
                var yaratanTc = HttpContext.Session.GetString("PersonelTc");
                var eklenenSayisi = 0;

                foreach (var personelTc in selectedPersoneller)
                {
                    // Aynı kişi aynı eğitim programına zaten eklenmiş mi kontrol et
                    if (!await _context.Egitilen.AnyAsync(e => e.PerTc == personelTc && e.EgtProgId == egitimProgramId))
                    {
                        var yeniEgitilen = new Egitilen
                        {
                            PerTc = personelTc,
                            EgtProgId = egitimProgramId,
                            Dk = null,
                            Yapildi = 0,
                            YaratanTc = yaratanTc,
                            Tarih = DateTime.Now
                        };

                        _context.Egitilen.Add(yeniEgitilen);
                        eklenenSayisi++;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{eklenenSayisi} personel eğitim programına başarıyla eklendi.";
                return RedirectToAction("Index", new { egitimProgramId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toplu Egitilen oluşturma hatası - ProgramId: {EgtProgId}", egitimProgramId);
                TempData["ErrorMessage"] = "Personeller eklenirken hata oluştu: " + ex.Message;
                return RedirectToAction("CreateMultiple", new { egitimProgramId });
            }
        }
        #endregion

        #region Personel Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var egitilen = await _context.Egitilen
                .Include(e => e.EgitimProgram)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (egitilen == null)
            {
                TempData["ErrorMessage"] = "Kayıt bulunamadı.";
                return RedirectToAction("Index", "EgitimProgram");
            }

            // Yetki kontrolü - sadece yaratıcı, sorumlu veya yönetici silebilir
            var currentUserTc = HttpContext.Session.GetString("PersonelTc");
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");

            if (egitilen.YaratanTc != currentUserTc &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu kaydı silme yetkiniz yok.";
                return RedirectToAction("Index", new { egitimProgramId = egitilen.EgtProgId });
            }

            try
            {
                _context.Egitilen.Remove(egitilen);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Personel eğitim programından başarıyla çıkarıldı.";
                return RedirectToAction("Index", new { egitimProgramId = egitilen.EgtProgId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Egitilen silme hatası - ID: {Id}", id);
                TempData["ErrorMessage"] = "Personel çıkarılırken hata oluştu: " + ex.Message;
                return RedirectToAction("Index", new { egitimProgramId = egitilen.EgtProgId });
            }
        }
        #endregion




        #region Düzenleme - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var egitilen = await _context.Egitilen
                .Include(e => e.Katilimci)
                .Include(e => e.EgitimProgram)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (egitilen == null)
            {
                TempData["ErrorMessage"] = "Kayıt bulunamadı.";
                return RedirectToAction("Index", "EgitimProgram");
            }

            // Yetki kontrolü - sadece yaratıcı, sorumlu veya yönetici düzenleyebilir
            var currentUserTc = HttpContext.Session.GetString("PersonelTc");
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");

            if (egitilen.YaratanTc != currentUserTc &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu kaydı düzenleme yetkiniz yok.";
                return RedirectToAction("Index", new { egitimProgramId = egitilen.EgtProgId });
            }

            var model = new EgitilenEditViewModel
            {
                Id = egitilen.Id,
                PerTc = egitilen.PerTc,
                Adlar = egitilen.Katilimci?.Adlar ?? "",
                Dk = egitilen.Dk,
                Yapildi = egitilen.Yapildi == 1,
                BasTar = egitilen.BasTar,
                BitTar = egitilen.BitTar,
                Aciklama = egitilen.Aciklama,
                EgtProgId = egitilen.EgtProgId
            };

            return View(model);
        }
        #endregion

        #region Düzenleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EgitilenEditViewModel model)
        {
            var egitilen = await _context.Egitilen
                .FirstOrDefaultAsync(e => e.Id == model.Id);

            if (egitilen == null)
            {
                TempData["ErrorMessage"] = "Kayıt bulunamadı.";
                return RedirectToAction("Index", "EgitimProgram");
            }

            // Yetki kontrolü - sadece yaratıcı, sorumlu veya yönetici düzenleyebilir
            var currentUserTc = HttpContext.Session.GetString("PersonelTc");
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");

            if (egitilen.YaratanTc != currentUserTc &&
                currentUserTip != "sorumlu" && currentUserTip != "yonetici")
            {
                TempData["ErrorMessage"] = "Bu kaydı düzenleme yetkiniz yok.";
                return RedirectToAction("Index", new { egitimProgramId = egitilen.EgtProgId });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                egitilen.Dk = model.Dk;
                egitilen.Yapildi = model.Yapildi ? 1 : 0;
                egitilen.BasTar = model.BasTar;
                egitilen.BitTar = model.BitTar;
                egitilen.Aciklama = model.Aciklama;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Katılımcı bilgileri başarıyla güncellendi.";
                return RedirectToAction("Index", new { egitimProgramId = model.EgtProgId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Egitilen düzenleme hatası - ID: {Id}", model.Id);
                ModelState.AddModelError("", "Katılımcı güncellenirken hata oluştu: " + ex.Message);
                return View(model);
            }
        }
        #endregion



    }
}