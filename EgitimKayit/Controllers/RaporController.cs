using System.Data;
using ClosedXML.Excel;
using EgitimKayit.Data;
using EgitimKayit.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EgitimKayit.Controllers
{
    public class RaporController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RaporController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dropdown'ları ve filtre formunu hazırlar
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new RaporFilterViewModel
            {
                Dershaneler = await _context.Dershane.OrderBy(d => d.Ad).ToListAsync(),
                EgitimTipleri = await _context.EgitimTip.OrderBy(et => et.Ad).ToListAsync(),
                EgitimSablonlari = await _context.EgitimSablon.OrderBy(es => es.Ad).ToListAsync(),
                // Birimler ve Tipler için Distinct değerleri çekiyoruz
                Birim1List = await _context.Personel.Where(p => p.Birim1 != null).Select(p => p.Birim1).Distinct().ToListAsync(),
                Birim2List = await _context.Personel.Where(p => p.Birim2 != null).Select(p => p.Birim2).Distinct().ToListAsync(),
                Birim3List = await _context.Personel.Where(p => p.Birim3 != null).Select(p => p.Birim3).Distinct().ToListAsync(),
                PersonelTipleri = await _context.Personel.Where(p => p.Tip != null).Select(p => p.Tip).Distinct().ToListAsync(),

                // Varsayılan tarih seçimi: Program tarihlerini baz al
                EgitimProgramTarihKullan = true,
                EgitilenTarihKullan = false
            };

            return View(model);
        }

        // Filtreleme sonrası Excel çıktısını oluşturur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RaporCikar(RaporFilterViewModel filter)
        {
            var (kayitlar, ozet) = await RaporVerisiCek(filter);

            if (kayitlar == null || !kayitlar.Any())
            {
                TempData["ErrorMessage"] = "Seçilen filtre kriterlerine uygun kayıt bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            return ExcelOlustur(kayitlar, ozet);
        }

        // Tüm rapor mantığını ve filtrelemeyi içeren ana metod
        private async Task<(List<RaporSonucViewModel.RaporKayit> Kayitlar, RaporSonucViewModel.RaporOzet Ozet)> RaporVerisiCek(RaporFilterViewModel filter)
        {
            // Ana Sorgu: Egitilen tablosundan başlayıp tüm ilişkileri yüklüyoruz
            var query = _context.Egitilen
                .Include(e => e.Katilimci)
                    .ThenInclude(p => p.StatuBilgi) // Statu Bilgisi
                .Include(e => e.EgitimProgram)
                    .ThenInclude(ep => ep!.Ogretmen) // Öğretmen bilgisi
                .Include(e => e.EgitimProgram)
                    .ThenInclude(ep => ep!.EgitimSablon)
                        .ThenInclude(es => es.Dershane) // Dershane
                .Include(e => e.EgitimProgram)
                    .ThenInclude(ep => ep!.EgitimSablon)
                        .ThenInclude(es => es.EgitimTip) // Eğitim Tipi
                .AsQueryable();


            // --- FİLTRELEMELER ---

            // 1. Birim Filtreleri
            if (!string.IsNullOrEmpty(filter.Birim1))
                query = query.Where(e => e.Katilimci!.Birim1 == filter.Birim1);
            if (!string.IsNullOrEmpty(filter.Birim2))
                query = query.Where(e => e.Katilimci!.Birim2 == filter.Birim2);
            if (!string.IsNullOrEmpty(filter.Birim3))
                query = query.Where(e => e.Katilimci!.Birim3 == filter.Birim3);

            // 2. Personel Tipi
            if (!string.IsNullOrEmpty(filter.PersonelTip))
                query = query.Where(e => e.Katilimci!.Tip == filter.PersonelTip);

            // 3. Durum Filtresi (0: Devam Ediyor/Yapılmadı, 1: Tamamlandı/Yapıldı)
            if (filter.Durum.HasValue)
            {
                query = query.Where(e => e.Yapildi == filter.Durum.Value);
            }

            // 4. Eğitim/Şablon Filtreleri
            if (filter.DershaneId.HasValue)
                query = query.Where(e => e.EgitimProgram!.EgitimSablon!.DerId == filter.DershaneId);
            if (filter.EgitimTipId.HasValue)
                query = query.Where(e => e.EgitimProgram!.EgitimSablon!.EtId == filter.EgitimTipId);
            if (filter.EgitimSablonId.HasValue)
                query = query.Where(e => e.EgitimProgram!.EsId == filter.EgitimSablonId);


            // 5. Tarih Filtreleri (En karmaşık kısım)
            if (filter.BaslangicTarihi.HasValue && filter.BitisTarihi.HasValue)
            {
                var basTar = filter.BaslangicTarihi.Value.Date;
                var bitTar = filter.BitisTarihi.Value.Date.AddDays(1).AddSeconds(-1); // Gün sonunu dahil et

                // EĞİTİM PROGRAMI TARİHLERİNİ KULLAN (EgitimProgram.BasTar/BitTar)
                if (filter.EgitimProgramTarihKullan && !filter.EgitilenTarihKullan)
                {
                    query = query.Where(e =>
                        e.EgitimProgram!.BasTar.HasValue && e.EgitimProgram.BasTar.Value <= bitTar &&
                        e.EgitimProgram.BitTar.HasValue && e.EgitimProgram.BitTar.Value >= basTar
                    );
                }
                // EĞİTİLEN TARİHLERİNİ KULLAN (Egitilen.Tarih - Katılım Başlangıç/Bitiş)
                else if (filter.EgitilenTarihKullan && !filter.EgitimProgramTarihKullan)
                {
                    query = query.Where(e =>
                       e.Tarih.HasValue && e.Tarih.Value >= basTar && e.Tarih.Value <= bitTar
                   );
                }
                // Her ikisi de seçiliyse (veya hiçbiri seçili değilse) varsayılan olarak tüm kayıtları getiririz (tarih filtresini es geçeriz)
                // Bu durumda, sadece tarihler seçilmişse, tüm kayıtları getiririz.
            }
            // Not: Hiçbir tarih seçeneği işaretlenmemişse veya tarih seçilmemişse, tüm kayıtlar çekilir.

            // --- VERİ ÇEKME VE MAPPING ---
            var sonucListesi = await query
                .Select(e => new RaporSonucViewModel.RaporKayit
                {
                    Tc = e.PerTc,
                    AdSoyad = e.Katilimci!.Adlar,
                    Statu = e.Katilimci.StatuBilgi!.Anlam,
                    Birim1 = e.Katilimci.Birim1,
                    Birim2 = e.Katilimci.Birim2,
                    Birim3 = e.Katilimci.Birim3,
                    PersonelTip = e.Katilimci.Tip,

                    EgitimAdi = e.EgitimProgram!.EgitimSablon!.Ad,
                    Dershane = e.EgitimProgram.EgitimSablon.Dershane!.Ad,
                    EgitimTip = e.EgitimProgram.EgitimSablon.EgitimTip!.Ad,

                    Dakika = e.Dk,
                    Durum = e.Yapildi == 1 ? "Tamamlandı" : "Devam Ediyor",

                    EgitimBaslangic = e.EgitimProgram.BasTar,
                    EgitimBitis = e.EgitimProgram.BitTar,
                    KatilimBaslangic = e.Tarih,
                    KatilimBitis = e.Tarih, // Egitilen'de tek tarih olduğu için
                    Ogretmen = e.EgitimProgram.Ogretmen!.Adlar 
                })
                .ToListAsync();

            // --- ÖZET HESAPLAMA ---
            var ozet = new RaporSonucViewModel.RaporOzet
            {
                ToplamKayit = sonucListesi.Count,
                ToplamKatilimci = sonucListesi.Select(r => r.Tc).Distinct().Count(),
                Tamamlanan = sonucListesi.Count(r => r.Durum == "Tamamlandı"),
                DevamEden = sonucListesi.Count(r => r.Durum == "Devam Ediyor"),
                ToplamDakika = sonucListesi.Sum(r => r.Dakika ?? 0),
                FarkliEgitimSayisi = sonucListesi.Select(r => r.EgitimAdi).Distinct().Count(),
                FarkliKatilimciSayisi = sonucListesi.Select(r => r.Tc).Distinct().Count()
            };
            ozet.OrtalamaDakika = ozet.ToplamKayit > 0 ? ozet.ToplamDakika / ozet.ToplamKayit : 0;


            return (sonucListesi, ozet);
        }

        // ClosedXML ile Excel dosyasını oluşturur ve döndürür
        private FileContentResult ExcelOlustur(List<RaporSonucViewModel.RaporKayit> kayitlar, RaporSonucViewModel.RaporOzet ozet)
        {
            using (var workbook = new XLWorkbook())
            {
                var dt = DataTableOlustur(kayitlar);
                var worksheet = workbook.Worksheets.Add(dt, "Eğitim Raporu");

                // Header stilini ayarla
                var headerRange = worksheet.Range(1, 1, 1, dt.Columns.Count);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                // Özet Bilgileri ayrı bir sayfaya veya üst kısma ekleyebilirsiniz.
                var ozetWs = workbook.Worksheets.Add("Özet");
                ozetWs.Cell(1, 1).Value = "Toplam Kayıt Sayısı";
                ozetWs.Cell(1, 2).Value = ozet.ToplamKayit;
                ozetWs.Cell(2, 1).Value = "Toplam Katılımcı Sayısı";
                ozetWs.Cell(2, 2).Value = ozet.ToplamKatilimci;
                // ... Diğer özet alanlarını da ekleyebilirsiniz ...

                // Excel dosyasını MemoryStream'e yaz
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Egitim_Raporu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                    );
                }
            }
        }

        // Rapor Kayıtlarını ClosedXML için DataTable'a dönüştürür
        private DataTable DataTableOlustur(List<RaporSonucViewModel.RaporKayit> kayitlar)
        {
            DataTable dt = new DataTable("Eğitim Raporu");

            // Kolonları oluştur
            dt.Columns.Add("TC", typeof(string));
            dt.Columns.Add("Ad Soyad", typeof(string));
            dt.Columns.Add("Statu", typeof(string));
            dt.Columns.Add("Birim 1", typeof(string));
            dt.Columns.Add("Birim 2", typeof(string));
            dt.Columns.Add("Birim 3", typeof(string));
            dt.Columns.Add("Personel Tipi", typeof(string));
            dt.Columns.Add("Eğitim Adı", typeof(string));
            dt.Columns.Add("Dershane", typeof(string));
            dt.Columns.Add("Eğitim Tipi", typeof(string));
            dt.Columns.Add("Süre (Dakika)", typeof(int));
            dt.Columns.Add("Durum", typeof(string));
            dt.Columns.Add("Eğitim Başlangıç", typeof(DateTime));
            dt.Columns.Add("Eğitim Bitiş", typeof(DateTime));
            dt.Columns.Add("Katılım Tarihi", typeof(DateTime));
            dt.Columns.Add("Öğretmen", typeof(string));


            // Verileri ekle
            foreach (var kayit in kayitlar)
            {
                dt.Rows.Add(
                    kayit.Tc,
                    kayit.AdSoyad,
                    kayit.Statu,
                    kayit.Birim1,
                    kayit.Birim2,
                    kayit.Birim3,
                    kayit.PersonelTip,
                    kayit.EgitimAdi,
                    kayit.Dershane,
                    kayit.EgitimTip,
                    kayit.Dakika,
                    kayit.Durum,
                    kayit.EgitimBaslangic,
                    kayit.EgitimBitis,
                    kayit.KatilimBaslangic,
                    kayit.Ogretmen
                );
            }

            return dt;
        }
    }
}