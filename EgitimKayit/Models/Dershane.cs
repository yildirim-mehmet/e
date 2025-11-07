using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EgitimKayit.Models
{
    [Table("Dershane")]
    public class Dershane
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("ad")]
        [MaxLength(100)]
        public string Ad { get; set; } = string.Empty;

        [Column("aciklama")]
        [MaxLength(500)]
        public string? Aciklama { get; set; }

        [Column("durum")]
        public int Durum { get; set; } = 1;

        [Column("yaratanTc")]
        [MaxLength(20)]
        public string? YaratanTc { get; set; }

        [Column("tarih")]
        public DateTime? Tarih { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("YaratanTc")]
        public Personel? Yaratan { get; set; }

        public ICollection<EgitimTip>? EgitimTipleri { get; set; }
        public ICollection<EgitimSablon>? EgitimSablonlari { get; set; }
        public ICollection<Yetki>? Yetkiler { get; set; }
    }
}