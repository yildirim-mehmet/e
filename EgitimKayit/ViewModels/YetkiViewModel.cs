using System.ComponentModel.DataAnnotations;
using EgitimKayit.Models;

namespace EgitimKayit.ViewModels
{
    public class YetkiViewModel
    {
        [Required(ErrorMessage = "Personel TC'si gereklidir")]
        [Display(Name = "Personel TC Kimlik No")]
        public string PerTc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dershane seçimi gereklidir")]
        [Display(Name = "Dershane")]
        public int DerId { get; set; }

        // Personel bilgileri için (AJAX'tan gelecek)
        public string? PersonelAd { get; set; }
        public string? PersonelTip { get; set; }
        public string? PersonelStatu { get; set; }

        // Dropdown listesi için
        public List<Dershane>? Dershaneler { get; set; }
    }

    public class YetkiIndexViewModel
    {
        public List<Yetki> Yetkiler { get; set; } = new();
        public List<Dershane> Dershaneler { get; set; } = new();
        public int? SeciliDershaneId { get; set; }
    }
}