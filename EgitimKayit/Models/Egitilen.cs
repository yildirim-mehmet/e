
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EgitimKayit.Models
{
    [Table("Egitilen")]
    public class Egitilen
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("dk")]
        public int? Dk { get; set; } // Dakika

        [Required]
        [Column("perTc")]
        [MaxLength(20)]
        public string PerTc { get; set; } = string.Empty; // Katılımcı

        [Required]
        [Column("egtProgId")]
        public int EgtProgId { get; set; }

        [Column("yapildi")]
        public int? Yapildi { get; set; } = 0; // 0: Yapılmadı, 1: Yapıldı

        [Column("basTar")]
        public DateTime? BasTar { get; set; }

        [Column("bitTar")]
        public DateTime? BitTar { get; set; }

        [Column("ogrtTc")]
        [MaxLength(20)]
        public string? OgrtTc { get; set; } // Derse giren hoca

        [Column("aciklama")]
        [MaxLength(500)]
        public string? Aciklama { get; set; }

        [Column("yaratanTc")]
        [MaxLength(20)]
        public string? YaratanTc { get; set; }

        [Column("tarih")]
        public DateTime? Tarih { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("PerTc")]
        public Personel? Katilimci { get; set; }

        [ForeignKey("EgtProgId")]
        public EgitimProgram? EgitimProgram { get; set; }

        [ForeignKey("OgrtTc")]
        public Personel? Ogretmen { get; set; }

        [ForeignKey("YaratanTc")]
        public Personel? Yaratan { get; set; }
    }
}


//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace EgitimKayit.Models
//{
//    [Table("Egitilen")]
//    public class Egitilen
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Column("dk")]
//        public int? Dk { get; set; } // Dakika

//        [Required]
//        [Column("perTc")]
//        [MaxLength(20)]
//        public string PerTc { get; set; } = string.Empty; // Katılımcı

//        [Required]
//        [Column("egtProgId")]
//        public int EgtProgId { get; set; }

//        [Column("yapildi")]
//        public int? Yapildi { get; set; } = 0; // 0: Yapılmadı, 1: Yapıldı

//        [Column("basTar")]
//        public DateTime? BasTar { get; set; }

//        [Column("bitTar")]
//        public DateTime? BitTar { get; set; }

//        [Column("ogrtTc")]
//        [MaxLength(20)]
//        public string? OgrtTc { get; set; } // Derse giren hoca

//        [Column("aciklama")]
//        [MaxLength(500)]
//        public string? Aciklama { get; set; }

//        [Column("yaratanTc")]
//        [MaxLength(20)]
//        public string? YaratanTc { get; set; }

//        [Column("tarih")]
//        public DateTime? Tarih { get; set; } = DateTime.Now;

//        // Navigation Properties - FOREIGN KEY'leri netleştiriyoruz
//        [ForeignKey("PerTc")]
//        public Personel? Katilimci { get; set; }

//        [ForeignKey("EgtProgId")]
//        public EgitimProgram? EgitimProgram { get; set; }

//        [ForeignKey("OgrtTc")]  // ← BU EKLENDİ
//        public Personel? Ogretmen { get; set; }

//        [ForeignKey("YaratanTc")]
//        public Personel? Yaratan { get; set; }
//    }
//}

////using System.ComponentModel.DataAnnotations;
////using System.ComponentModel.DataAnnotations.Schema;

////namespace EgitimKayit.Models
////{
////    [Table("Egitilen")]
////    public class Egitilen
////    {
////        [Key]
////        [Column("id")]
////        public int Id { get; set; }

////        [Column("dk")]
////        public int? Dk { get; set; } // Dakika

////        [Required]
////        [Column("perTc")]
////        [MaxLength(20)]
////        public string PerTc { get; set; } = string.Empty; // Katılımcı

////        [Required]
////        [Column("egtProgId")]
////        public int EgtProgId { get; set; }

////        [Column("yapildi")]
////        public int? Yapildi { get; set; } = 0; // 0: Yapılmadı, 1: Yapıldı

////        [Column("basTar")]
////        public DateTime? BasTar { get; set; }

////        [Column("bitTar")]
////        public DateTime? BitTar { get; set; }

////        [Column("ogrtTc")]
////        [MaxLength(20)]
////        public string? OgrtTc { get; set; } // Derse giren hoca

////        [Column("aciklama")]
////        [MaxLength(500)]
////        public string? Aciklama { get; set; }

////        [Column("yaratanTc")]
////        [MaxLength(20)]
////        public string? YaratanTc { get; set; }

////        [Column("tarih")]
////        public DateTime? Tarih { get; set; } = DateTime.Now;

////        // Navigation Properties
////        [ForeignKey("PerTc")]
////        public Personel? Katilimci { get; set; }

////        [ForeignKey("EgtProgId")]
////        public EgitimProgram? EgitimProgram { get; set; }

////        [ForeignKey("OgrtTc")]
////        public Personel? Ogretmen { get; set; }

////        [ForeignKey("YaratanTc")]
////        public Personel? Yaratan { get; set; }
////    }
////}