using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EgitimKayit.Models
{
    [Table("EgitimTip")]
    public class EgitimTip
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("derId")]
        public int DerId { get; set; }

        [Required]
        [Column("ad")]
        [MaxLength(100)]
        public string Ad { get; set; } = string.Empty;

        [Column("yaratanTc")]
        [MaxLength(20)]
        public string? YaratanTc { get; set; }

        [Column("tarih")]
        public DateTime? Tarih { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("DerId")]
        public Dershane? Dershane { get; set; }

        [ForeignKey("YaratanTc")]
        public Personel? Yaratan { get; set; }

        public ICollection<EgitimSablon>? EgitimSablonlari { get; set; }
    }
}