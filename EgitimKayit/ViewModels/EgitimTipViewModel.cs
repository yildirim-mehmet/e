using System.ComponentModel.DataAnnotations;
using EgitimKayit.Models;

namespace EgitimKayit.ViewModels
{
    public class EgitimTipViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Dershane seçimi gereklidir")]
        [Display(Name = "Dershane")]
        public int DerId { get; set; }

        [Required(ErrorMessage = "Eğitim tipi adı gereklidir")]
        [StringLength(100, ErrorMessage = "Eğitim tipi adı en fazla 100 karakter olabilir")]
        [Display(Name = "Eğitim Tipi Adı")]
        public string Ad { get; set; } = string.Empty;

        // Dropdown listesi için
        public List<Dershane>? Dershaneler { get; set; }
    }

    public class EgitimTipIndexViewModel
    {
        public List<EgitimTip> EgitimTipleri { get; set; } = new();
        public List<Dershane> Dershaneler { get; set; } = new();
        public int? SeciliDershaneId { get; set; }
    }
}