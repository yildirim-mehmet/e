using System.ComponentModel.DataAnnotations;

namespace EgitimKayit.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "TC Kimlik No gereklidir")]
        [Display(Name = "TC Kimlik No")]
        public string Tc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}