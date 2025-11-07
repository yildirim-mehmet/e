using System.ComponentModel.DataAnnotations;

namespace EgitimKayit.ViewModels
{
    public class EgitilenEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "TC Kimlik No")]
        public string PerTc { get; set; } = string.Empty;

        [Display(Name = "Ad Soyad")]
        public string Adlar { get; set; } = string.Empty;

        [Display(Name = "Dakika")]
        [Range(0, 1000, ErrorMessage = "Dakika 0-1000 arasında olmalıdır")]
        public int? Dk { get; set; }

        [Display(Name = "Tamamlandı mı?")]
        public bool Yapildi { get; set; }

        [Display(Name = "Başlangıç Tarihi")]
        public DateTime? BasTar { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        public DateTime? BitTar { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Aciklama { get; set; }

        public int EgtProgId { get; set; }
    }
}