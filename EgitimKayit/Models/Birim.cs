using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EgitimKayit.Models
{
    [Table("Birim")]
    public class Birim
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("ad")]
        [MaxLength(100)]
        public string? Ad { get; set; }

        [Column("ustId")]
        public int UstId { get; set; }

        [Column("birimSeviye")]
        public int? BirimSeviye { get; set; }
    }
}