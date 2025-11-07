using EgitimKayit.Models;

namespace EgitimKayit.ViewModels
{
    public class EgitimSablonIndexViewModel
    {
        public List<EgitimSablon> EgitimSablonlari { get; set; } = new();
        public List<Dershane> Dershaneler { get; set; } = new();
        public List<EgitimTip> EgitimTipleri { get; set; } = new();
        public int? SeciliDershaneId { get; set; }
        public int? SeciliEgitimTipId { get; set; }
    }
}