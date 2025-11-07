using System.ComponentModel.DataAnnotations;
using EgitimKayit.Models;

namespace EgitimKayit.ViewModels
{
    public class EgitimSablonViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Eğitim şablon adı gereklidir")]
        [StringLength(200, ErrorMessage = "Eğitim şablon adı en fazla 200 karakter olabilir")]
        [Display(Name = "Eğitim Şablon Adı")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dershane seçimi gereklidir")]
        [Display(Name = "Dershane")]
        public int DerId { get; set; }

        [Required(ErrorMessage = "Eğitim tipi seçimi gereklidir")]
        [Display(Name = "Eğitim Tipi")]
        public int EtId { get; set; }

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        // Dropdown listeleri için
        public List<Dershane>? Dershaneler { get; set; }
        public List<EgitimTip>? EgitimTipleri { get; set; }
    }
}


//using System.ComponentModel.DataAnnotations;
//using EgitimKayit.Models;

//namespace EgitimKayit.ViewModels
//{
//    public class EgitimSablonViewModel
//    {
//        public int Id { get; set; }

//        [Required(ErrorMessage = "Dershane seçimi gereklidir")]
//        [Display(Name = "Dershane")]
//        public int DerId { get; set; }

//        [Required(ErrorMessage = "Eğitim tipi seçimi gereklidir")]
//        [Display(Name = "Eğitim Tipi")]
//        public int EtId { get; set; }

//        [Required(ErrorMessage = "Şablon adı gereklidir")]
//        [StringLength(200, ErrorMessage = "Şablon adı en fazla 200 karakter olabilir")]
//        [Display(Name = "Şablon Adı")]
//        public string Ad { get; set; } = string.Empty;

//        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
//        [Display(Name = "Açıklama")]
//        public string? Aciklama { get; set; }

//        // Dropdown listeleri için
//        public List<Dershane>? Dershaneler { get; set; }
//        public List<EgitimTip>? EgitimTipleri { get; set; }
//    }

//    public class EgitimSablonIndexViewModel
//    {
//        public List<EgitimSablon> EgitimSablonlari { get; set; } = new();
//        public List<Dershane> Dershaneler { get; set; } = new();
//        public List<EgitimTip> EgitimTipleri { get; set; } = new();
//        public int? SeciliDershaneId { get; set; }
//        public int? SeciliEgitimTipId { get; set; }
//    }
//}