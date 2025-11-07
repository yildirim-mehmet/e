using EgitimKayit.Models;

namespace EgitimKayit.ViewModels
{
    public class EgitimProgramIndexViewModel
    {
        public List<EgitimProgram> EgitimProgramlari { get; set; } = new();
        public List<EgitimSablon> EgitimSablonlari { get; set; } = new();
        public int? SeciliEgitimSablonId { get; set; }
        public int? SeciliOnayDurumu { get; set; } // NULL: Tümü, 0: Red, 1: Onay, 2: Beklemede
    }
}
//using EgitimKayit.Models;

//namespace EgitimKayit.ViewModels
//{
//    public class EgitimProgramIndexViewModel
//    {
//        public List<EgitimProgram> EgitimProgramlari { get; set; } = new();
//        public List<EgitimSablon> EgitimSablonlari { get; set; } = new();
//        public int? SeciliEgitimSablonId { get; set; }
//        public int? OnayDurumu { get; set; } // null: Tümü, 0: Beklemede, 1: Onaylandı
//    }
//}