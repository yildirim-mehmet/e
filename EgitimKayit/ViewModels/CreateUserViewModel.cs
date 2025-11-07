using System.ComponentModel.DataAnnotations;
using EgitimKayit.Models;

namespace EgitimKayit.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "TC Kimlik No gereklidir")]
        [StringLength(20, ErrorMessage = "TC en fazla 20 karakter olabilir")]
        [Display(Name = "TC Kimlik No")]
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
        public int? Birim1Id { get; set; }

        [Display(Name = "Birim 2")]
        public int? Birim2Id { get; set; }

        [Display(Name = "Birim 3")]
        public int? Birim3Id { get; set; }

        [Display(Name = "Kullanıcı Tipi")]
        public string? Tip { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
        [DataType(DataType.Password)]
        [Compare("Sifre", ErrorMessage = "Şifreler uyuşmuyor")]
        [Display(Name = "Şifre Tekrar")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Resim")]
        public IFormFile? ResimDosya { get; set; }

        // Dropdown listeleri için
        public List<Statu>? StatuList { get; set; }
        public List<Birim>? Birim1List { get; set; }
        public List<Birim>? Birim2List { get; set; }
        public List<Birim>? Birim3List { get; set; }
    }
}