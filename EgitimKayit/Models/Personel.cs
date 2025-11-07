using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EgitimKayit.Models
{
    [Table("Personel")]
    public class Personel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("tc")]
        [MaxLength(20)]
        public string Tc { get; set; } = string.Empty;

        [Column("adlar")]
        [MaxLength(100)]
        public string? Adlar { get; set; }

        [Column("statu")]
        public int? Statu { get; set; }

        [ForeignKey("Statu")]
        public Statu? StatuBilgi { get; set; }

        [Column("kuvvet")]
        [MaxLength(50)]
        public string? Kuvvet { get; set; }

        [Column("sinif")]
        [MaxLength(50)]
        public string? Sinif { get; set; }

        [Column("sicil")]
        [MaxLength(100)]
        public string? Sicil { get; set; }

        [Column("birim1")]
        [MaxLength(150)]
        public string? Birim1 { get; set; }

        [Column("birim2")]
        [MaxLength(1000)]
        public string? Birim2 { get; set; }

        [Column("birim3")]
        [MaxLength(500)]
        public string? Birim3 { get; set; }

        [Column("resim")]
        [MaxLength(100)]
        public string? Resim { get; set; }

        [Column("tip")]
        [MaxLength(50)]
        public string? Tip { get; set; }

        [Column("sifre")]
        [MaxLength(60)]
        public string? Sifre { get; set; }

        [Column("aktif")]
        public int Aktif { get; set; } = 1;

        [Column("tarih")]
        public DateTime? Tarih { get; set; } = DateTime.Now;

        [Column("alan")]
        [MaxLength(100)]
        public string? Alan { get; set; }

        [Column("yaratanTc")]
        [MaxLength(20)]
        public string? YaratanTc { get; set; } // yaratanId → yaratanTc

        //// Navigation Properties
        //[ForeignKey("YaratanTc")]
        //public Personel? Yaratan { get; set; }

        ////Login için gerekli olmayan navigation property'leri KALDIRIYORUZ
        //public ICollection<EgitimProgram>? EgitimProgramlari { get; set; }
        //public ICollection<Egitilen>? KatildigiEgitimler { get; set; }
        //public ICollection<Yetki>? Yetkileri { get; set; }

        // Navigation Properties - DÜZELTİLDİ
        [ForeignKey("YaratanTc")]
        public Personel? Yaratan { get; set; }

        public ICollection<EgitimProgram>? EgitimProgramlari { get; set; }
        public ICollection<Egitilen>? KatildigiEgitimler { get; set; }
        public ICollection<Yetki>? Yetkileri { get; set; }
    }
}