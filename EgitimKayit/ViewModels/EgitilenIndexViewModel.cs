using EgitimKayit.Models;

namespace EgitimKayit.ViewModels
{
    public class EgitilenIndexViewModel
    {
        public int EgitimProgramId { get; set; }
        public EgitimProgram? EgitimProgram { get; set; }
        public List<Egitilen> Egitilenler { get; set; } = new();
        public List<Personel> Personeller { get; set; } = new();
    }
}