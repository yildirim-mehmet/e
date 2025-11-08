//using System;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using ClosedXML.Excel;
//using EgitimKayit.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using YourNamespace.Models; // kendi namespace'ni ekle

//public class EgitilenController : Controller
//{
//    private readonly ApplicationDbContext _context;
//    private readonly ILogger<EgitilenController> _logger;

//    public EgitilenController(ApplicationDbContext context, ILogger<EgitilenController> logger)
//    {
//        _context = context;
//        _logger = logger;
//    }

//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> ExcelUpload(IFormFile ExcelFile, int EgitimProgramId, bool FirstRowIsHeader = true)
//    {
//        // yetki kontrolü (senin mevcut pattern'e uyacak şekilde)
//        var currentUserTip = HttpContext.Session.GetString("PersonelTip");
//        if (currentUserTip != "talepci" && currentUserTip != "ogretmen" &&
//            currentUserTip != "sorumlu" && currentUserTip != "yonetici")
//        {
//            TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
//            return RedirectToAction("Index", "EgitimProgram");
//        }

//        if (ExcelFile == null || ExcelFile.Length == 0)
//        {
//            TempData["ErrorMessage"] = "Lütfen bir Excel dosyası seçin.";
//            return RedirectToAction("Index", new { egitimProgramId = EgitimProgramId });
//        }

//        try
//        {
//            var result = await ProcessExcelFile(ExcelFile, EgitimProgramId, FirstRowIsHeader);

//            if (result.Success)
//            {
//                TempData["SuccessMessage"] = $"Excel işlendi. {result.AddedEgitilenCount} eğitim kaydı, {result.AddedPersonelCount} yeni personel eklendi.";
//            }
//            else
//            {
//                TempData["ErrorMessage"] = "Excel işlenemedi: " + result.ErrorMessage;
//            }

//            return RedirectToAction("Index", new { egitimProgramId = EgitimProgramId });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Excel yükleme hatası - ProgramId: {EgitimProgramId}", EgitimProgramId);
//            TempData["ErrorMessage"] = "Excel dosyası yüklenirken hata oluştu: " + ex.Message;
//            return RedirectToAction("Index", new { egitimProgramId = EgitimProgramId });
//        }
//    }

//    private class ProcessResult
//    {
//        public bool Success { get; set; } = true;
//        public int AddedPersonelCount { get; set; } = 0;
//        public int AddedEgitilenCount { get; set; } = 0;
//        public string ErrorMessage { get; set; } = "";
//    }

//    private async Task<ProcessResult> ProcessExcelFile(IFormFile file, int egitimProgramId, bool firstRowIsHeader)
//    {
//        var result = new ProcessResult();

//        // Eğitim program bilgilerini al (BasTar, BitTar, Aciklama)
//        var egitimProgram = await _context.EgitimProgram
//            .FirstOrDefaultAsync(ep => ep.Id == egitimProgramId); // eğer farklı isimliyse düzelt

//        // Transaction ile sar
//        using var transaction = await _context.Database.BeginTransactionAsync();
//        try
//        {
//            using var stream = file.OpenReadStream();
//            using var workbook = new XLWorkbook(stream);
//            var worksheet = workbook.Worksheets.First(); // ilk sheet
//            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
//            var startRow = firstRowIsHeader ? 2 : 1;

//            for (int row = startRow; row <= lastRow; row++)
//            {
//                // Excel sütun sırası (senin onayladığın):
//                // 1: TC, 2: Ad Soyad, 3: Statü(Anlam), 4: Birim1, 5: Birim2, 6: Birim3, 7: Dakika
//                var tc = worksheet.Cell(row, 1).GetString()?.Trim();
//                if (string.IsNullOrWhiteSpace(tc)) continue; // boş satır atla

//                // normalize TC (istersen ekstra temizleme yap)
//                tc = tc.Replace(" ", "");

//                var adSoyad = worksheet.Cell(row, 2).GetString()?.Trim();
//                var statüAnlam = worksheet.Cell(row, 3).GetString()?.Trim();
//                var birim1 = worksheet.Cell(row, 4).GetString()?.Trim();
//                var birim2 = worksheet.Cell(row, 5).GetString()?.Trim();
//                var birim3 = worksheet.Cell(row, 6).GetString()?.Trim();
//                var dakikaRaw = worksheet.Cell(row, 7).GetString()?.Trim();

//                int dk = 0;
//                if (!string.IsNullOrWhiteSpace(dakikaRaw))
//                {
//                    // sayısal parse denemesi
//                    if (!int.TryParse(dakikaRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out dk))
//                    {
//                        // hücre metin formatındaysa, virgül/nokta olanları da handle et
//                        var cleaned = dakikaRaw.Replace(",", ".").Split('.')[0];
//                        int.TryParse(cleaned, out dk);
//                    }
//                }

//                // --- Statü eşleştirme: Anlam => StatuDeger; bulunamazsa 999999 ---
//                int statudeğer = 999999;
//                if (!string.IsNullOrWhiteSpace(statüAnlam))
//                {
//                    var statu = await _context.Set<Statu>()
//                        .FirstOrDefaultAsync(s => s.Anlam == statüAnlam);
//                    if (statu != null && statu.StatuDeger.HasValue)
//                    {
//                        statudeğer = statu.StatuDeger.Value;
//                    }
//                    else
//                    {
//                        // eşleşmezse kural gereği 999999 kullan (yeni Statu oluşturma yok)
//                        statudeğer = 999999;
//                    }
//                }

//                // --- Personel kontrol / oluşturma ---
//                var personel = await _context.Personel
//                    .FirstOrDefaultAsync(p => p.Tc == tc && p.Aktif == 1);

//                if (personel == null)
//                {
//                    // Yeni personel yarat (Ad güncelleme yapılmayacak burada, sadece yaratıyorum)
//                    personel = new Personel
//                    {
//                        Tc = tc,
//                        Adlar = string.IsNullOrWhiteSpace(adSoyad) ? null : adSoyad,
//                        Statu = statudeğer,
//                        Birim1 = string.IsNullOrWhiteSpace(birim1) ? null : birim1,
//                        Birim2 = string.IsNullOrWhiteSpace(birim2) ? null : birim2,
//                        Birim3 = string.IsNullOrWhiteSpace(birim3) ? null : birim3,
//                        Tarih = DateTime.Now,
//                        YaratanTc = HttpContext.Session.GetString("PersonelTc"),
//                        Aktif = 1
//                    };

//                    _context.Personel.Add(personel);
//                    await _context.SaveChangesAsync(); // personel'i kaydet
//                    result.AddedPersonelCount++;
//                }
//                else
//                {
//                    // Var olan personel için **isim veya birim güncelleme yapılmayacak** (senin isteğin)
//                    // Ancak eğer Statu boşsa veya farklıysa değiştirmek istersen burada ekleyebilirsin.
//                }

//                // --- Egitilen kaydı oluştur ---
//                var egitilen = new Egitilen
//                {
//                    PerTc = tc, // PersonelId yok; TC üzerinden ilişki
//                    EgtProgId = egitimProgramId,
//                    Dk = dk,
//                    BasTar = egitimProgram?.BasTar ?? DateTime.Now,
//                    BitTar = egitimProgram?.BitTar ?? DateTime.Now,
//                    Aciklama = egitimProgram?.Aciklama,
//                    Yapildi = 0,
//                    YaratanTc = HttpContext.Session.GetString("PersonelTc"),
//                    Tarih = DateTime.Now
//                };

//                _context.Egitilen.Add(egitilen);
//                result.AddedEgitilenCount++;

//                // ---- performans için: istersen belirli aralıklarla SaveChanges yapabilirsin.
//                // burada her satır sonrası SaveChanges yapmıyoruz (personel için yaptık),
//                // egitilenleri döngü sonunda kaydetmeyi tercih edebiliriz.
//            }

//            // döngü bittikten sonra tüm eklenen Egitilen kayıtlarını kaydet
//            await _context.SaveChangesAsync();
//            await transaction.CommitAsync();

//            result.Success = true;
//            return result;
//        }
//        catch (Exception ex)
//        {
//            await transaction.RollbackAsync();
//            _logger.LogError(ex, "Excel işleme sırasında hata oluştu.");
//            result.Success = false;
//            result.ErrorMessage = ex.Message;
//            return result;
//        }
//    }
//}
