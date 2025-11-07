using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EgitimKayit.Models
{
    [Table("Yetki")]
    public class Yetki
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("perTc")]
        [MaxLength(20)]
        public string PerTc { get; set; } = string.Empty;

        [Required]
        [Column("derId")]
        public int DerId { get; set; }

        [Column("yaratanTc")]
        [MaxLength(20)]
        public string? YaratanTc { get; set; }

        [Column("tarih")]
        public DateTime? Tarih { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("PerTc")]
        public Personel? Personel { get; set; }

        [ForeignKey("DerId")]
        public Dershane? Dershane { get; set; }

        [ForeignKey("YaratanTc")]
        public Personel? Yaratan { get; set; }
    }
}


//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace EgitimKayit.Models
//{
//    [Table("Yetki")]
//    public class Yetki
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("perTc")]
//        [MaxLength(20)]
//        public string PerTc { get; set; } = string.Empty;

//        [Required]
//        [Column("derId")]
//        public int DerId { get; set; }

//        [Column("yaratanTc")]
//        [MaxLength(20)]
//        public string? YaratanTc { get; set; }

//        [Column("tarih")]
//        public DateTime? Tarih { get; set; } = DateTime.Now;

//        // Navigation Properties
//        [ForeignKey("PerTc")]
//        public Personel? Personel { get; set; }

//        [ForeignKey("DerId")]
//        public Dershane? Dershane { get; set; }

//        [ForeignKey("YaratanTc")]
//        public Personel? Yaratan { get; set; }
//    }
//}