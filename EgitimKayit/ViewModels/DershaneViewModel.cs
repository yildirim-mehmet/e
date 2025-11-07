using System.ComponentModel.DataAnnotations;

namespace EgitimKayit.ViewModels
{
    public class DershaneViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Dershane adı gereklidir")]
        [StringLength(100, ErrorMessage = "Dershane adı en fazla 100 karakter olabilir")]
        [Display(Name = "Dershane Adı")]
        public string Ad { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }
    }
}