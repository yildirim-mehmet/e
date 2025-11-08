using System.ComponentModel.DataAnnotations;

namespace EgitimKayit.ViewModels
{
    public class RaporFilterViewModel
    {
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime? BaslangicTarihi { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        public DateTime? BitisTarihi { get; set; }

        [Display(Name = "Eğitim Programı Tarihlerini Kullan")]
        public bool EgitimProgramTarihKullan { get; set; } = true;

        [Display(Name = "Eğitilen Tarihlerini Kullan")]
        public bool EgitilenTarihKullan { get; set; }

        [Display(Name = "Dershane")]
        public int? DershaneId { get; set; }

        [Display(Name = "Eğitim Tipi")]
        public int? EgitimTipId { get; set; }

        [Display(Name = "Eğitim Şablonu")]
        public int? EgitimSablonId { get; set; }

        [Display(Name = "Birim 1")]
        public string? Birim1 { get; set; }

        [Display(Name = "Birim 2")]
        public string? Birim2 { get; set; }

        [Display(Name = "Birim 3")]
        public string? Birim3 { get; set; }

        [Display(Name = "Personel Tipi")]
        public string? PersonelTip { get; set; }

        [Display(Name = "Durum")]
        public int? Durum { get; set; } // 0: Devam Ediyor, 1: Tamamlandı

        // Dropdown listeleri için
        public List<Models.Dershane>? Dershaneler { get; set; }
        public List<Models.EgitimTip>? EgitimTipleri { get; set; }
        public List<Models.EgitimSablon>? EgitimSablonlari { get; set; }
        public List<string>? Birim1List { get; set; }
        public List<string>? Birim2List { get; set; }
        public List<string>? Birim3List { get; set; }
        public List<string>? PersonelTipleri { get; set; }
    }
}