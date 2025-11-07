using System.ComponentModel.DataAnnotations;
using EgitimKayit.Models;

namespace EgitimKayit.ViewModels
{
    public class EgitimProgramViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Eğitim şablonu seçimi gereklidir")]
        [Display(Name = "Eğitim Şablonu")]
        public int EsId { get; set; }

        [Display(Name = "Öğretmen")]
        public string? PerTc { get; set; }

        [Display(Name = "Onay Durumu")]
        public int? Onayli { get; set; }

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi gereklidir")]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime? BasTar { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi gereklidir")]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime? BitTar { get; set; }

        // Dropdown listeleri için
        public List<EgitimSablon>? EgitimSablonlari { get; set; }
        public List<Personel>? Ogretmenler { get; set; }
    }
}