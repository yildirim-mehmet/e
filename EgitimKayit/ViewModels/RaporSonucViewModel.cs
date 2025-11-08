using EgitimKayit.Models;

namespace EgitimKayit.ViewModels
{
    public class RaporSonucViewModel
    {
        public List<RaporKayit> Kayitlar { get; set; } = new();
        public RaporFilterViewModel Filtre { get; set; } = new();
        public RaporOzet Ozet { get; set; } = new();

        public class RaporKayit
        {
            public string? Tc { get; set; }
            public string? AdSoyad { get; set; }
            public string? Statu { get; set; }
            public string? Birim1 { get; set; }
            public string? Birim2 { get; set; }
            public string? Birim3 { get; set; }
            public string? PersonelTip { get; set; }
            public string? EgitimAdi { get; set; }
            public string? Dershane { get; set; }
            public string? EgitimTip { get; set; }
            public int? Dakika { get; set; }
            public string? Durum { get; set; }
            public DateTime? EgitimBaslangic { get; set; }
            public DateTime? EgitimBitis { get; set; }
            public DateTime? KatilimBaslangic { get; set; }
            public DateTime? KatilimBitis { get; set; }
            public string? Ogretmen { get; set; }
        }

        public class RaporOzet
        {
            public int ToplamKayit { get; set; }
            public int ToplamKatilimci { get; set; }
            public int Tamamlanan { get; set; }
            public int DevamEden { get; set; }
            public int ToplamDakika { get; set; }
            public int OrtalamaDakika { get; set; }
            public int FarkliEgitimSayisi { get; set; }
            public int FarkliKatilimciSayisi { get; set; }
        }
    }
}