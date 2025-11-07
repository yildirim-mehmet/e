using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EgitimKayit.ViewModels
{
    public class ExcelUploadViewModel
    {
        [Required(ErrorMessage = "Excel dosyası gereklidir")]
        [Display(Name = "Excel Dosyası")]
        public IFormFile ExcelFile { get; set; }

        public int EgitimProgramId { get; set; }

        [Display(Name = "İlk satır başlık mı?")]
        public bool FirstRowIsHeader { get; set; } = true;
    }
}