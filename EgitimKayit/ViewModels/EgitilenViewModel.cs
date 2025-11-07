using System.ComponentModel.DataAnnotations;
using EgitimKayit.Models;

namespace EgitimKayit.ViewModels
{
    public class EgitilenViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "TC Kimlik No gereklidir")]
        [Display(Name = "TC Kimlik No")]
        public string PerTc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Eğitim Programı gereklidir")]
        public int EgtProgId { get; set; }

        [Display(Name = "Dakika")]
        [Range(0, 1000, ErrorMessage = "Dakika 0-1000 arasında olmalıdır")]
        public int? Dk { get; set; }

        [Display(Name = "Başlangıç Tarihi")]
        public DateTime? BasTar { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        public DateTime? BitTar { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Aciklama { get; set; }

        // Navigation Properties for Display
        public Personel? Katilimci { get; set; }
        public EgitimProgram? EgitimProgram { get; set; }
        public List<Personel>? Personeller { get; set; }
    }
}