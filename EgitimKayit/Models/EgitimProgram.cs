using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EgitimKayit.Models
{
    [Table("EgitimProgram")]
    public class EgitimProgram
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("esId")]
        public int EsId { get; set; }

        [Column("perTc")]
        [MaxLength(20)]
        public string? PerTc { get; set; } // ogretmen

        [Column("onayli")]
        public int? Onayli { get; set; } // NULL: Beklemede, 1: Onaylandı, 0: Reddedildi

        [Column("aciklama")]
        [MaxLength(1000)]
        public string? Aciklama { get; set; }

        [Column("basTar")]
        public DateTime? BasTar { get; set; }

        [Column("bitTar")]
        public DateTime? BitTar { get; set; }

        [Column("yaratanTc")]
        [MaxLength(20)]
        public string? YaratanTc { get; set; }

        [Column("tarih")]
        public DateTime? Tarih { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("EsId")]
        public EgitimSablon? EgitimSablon { get; set; }

        [ForeignKey("PerTc")]
        public Personel? Ogretmen { get; set; }

        [ForeignKey("YaratanTc")]
        public Personel? Yaratan { get; set; }

        public ICollection<Egitilen>? Katilimcilar { get; set; }
    }
}


//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace EgitimKayit.Models
//{
//    [Table("EgitimProgram")]
//    public class EgitimProgram
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("esId")]
//        public int EsId { get; set; }

//        [Column("perTc")]
//        [MaxLength(20)]
//        public string? PerTc { get; set; } // ogretmen

//        [Column("onayli")]
//        public int? Onayli { get; set; } // NULL: Beklemede, 1: Onaylandı

//        [Column("aciklama")]
//        [MaxLength(1000)]
//        public string? Aciklama { get; set; }

//        [Column("basTar")]
//        public DateTime? BasTar { get; set; }

//        [Column("bitTar")]
//        public DateTime? BitTar { get; set; }

//        [Column("yaratanTc")]
//        [MaxLength(20)]
//        public string? YaratanTc { get; set; }

//        [Column("tarih")]
//        public DateTime? Tarih { get; set; } = DateTime.Now;

//        // Navigation Properties
//        [ForeignKey("EsId")]
//        public EgitimSablon? EgitimSablon { get; set; }

//        [ForeignKey("PerTc")]
//        public Personel? Ogretmen { get; set; }

//        [ForeignKey("YaratanTc")]
//        public Personel? Yaratan { get; set; }

//        public ICollection<Egitilen>? Katilimcilar { get; set; }
//    }
//}