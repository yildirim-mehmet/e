using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EgitimKayit.Models
{
    [Table("Statu")]
    public class Statu
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("statuDeger")]
        public int? StatuDeger { get; set; }

        [Column("anlam")]
        [MaxLength(50)]
        public string? Anlam { get; set; }
    }
}