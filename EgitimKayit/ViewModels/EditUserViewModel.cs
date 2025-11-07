using System.ComponentModel.DataAnnotations;
using EgitimKayit.Models;

namespace EgitimKayit.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        public string Tc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad Soyad gereklidir")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir")]
        [Display(Name = "Ad Soyad")]
        public string Adlar { get; set; } = string.Empty;

        [Display(Name = "Statü")]
        public int? StatuId { get; set; }

        [Display(Name = "Kuvvet")]
        public string? Kuvvet { get; set; }

        [Display(Name = "Sınıf")]
        public string? Sinif { get; set; }

        [Display(Name = "Sicil No")]
        public string? Sicil { get; set; }

        [Display(Name = "Birim 1")]
        public string? Birim1 { get; set; }

        [Display(Name = "Birim 2")]
        public string? Birim2 { get; set; }

        [Display(Name = "Birim 3")]
        public string? Birim3 { get; set; }

        [Required(ErrorMessage = "Kullanıcı tipi gereklidir")]
        [Display(Name = "Kullanıcı Tipi")]
        public string? Tip { get; set; }

        // Dropdown listeleri için
        public List<Statu>? StatuList { get; set; }
        public List<Birim>? Birim1List { get; set; }
    }


}