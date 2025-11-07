using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EgitimKayit.Models
{
    [Table("EgitimSablon")]
    public class EgitimSablon
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("ad")]
        [MaxLength(200)]
        public string Ad { get; set; } = string.Empty;

        [Required]
        [Column("derId")]
        public int DerId { get; set; }

        [Required]
        [Column("etId")]
        public int EtId { get; set; }

        [Column("aciklama")]
        [MaxLength(1000)]
        public string? Aciklama { get; set; }

        [Column("yaratanTc")]
        [MaxLength(20)]
        public string? YaratanTc { get; set; }

        [Column("tarih")]
        public DateTime? Tarih { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("DerId")]
        public Dershane? Dershane { get; set; }

        [ForeignKey("EtId")]
        public EgitimTip? EgitimTip { get; set; }

        [ForeignKey("YaratanTc")]
        public Personel? Yaratan { get; set; }

        public ICollection<EgitimProgram>? EgitimProgramlari { get; set; }
    }
}


// versiyon 2 sonrası 

//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace EgitimKayit.Models
//{
//    [Table("EgitimSablon")]
//    public class EgitimSablon
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("ad")]
//        [MaxLength(200)]
//        public string Ad { get; set; } = string.Empty;

//        [Required]
//        [Column("derId")]
//        public int DerId { get; set; }

//        [Required]
//        [Column("etId")]
//        public int EtId { get; set; }

//        [Column("aciklama")]
//        [MaxLength(1000)]
//        public string? Aciklama { get; set; }

//        [Column("yaratanTc")]
//        [MaxLength(20)]
//        public string? YaratanTc { get; set; }

//        [Column("tarih")]
//        public DateTime? Tarih { get; set; } = DateTime.Now;

//        // Navigation Properties
//        [ForeignKey("DerId")]
//        public Dershane? Dershane { get; set; }

//        [ForeignKey("EtId")]
//        public EgitimTip? EgitimTip { get; set; }

//        [ForeignKey("YaratanTc")]
//        public Personel? Yaratan { get; set; }

//        public ICollection<EgitimProgram>? EgitimProgramlari { get; set; }
//    }
//}