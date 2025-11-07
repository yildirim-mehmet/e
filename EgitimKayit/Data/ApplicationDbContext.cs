using Microsoft.EntityFrameworkCore;
using EgitimKayit.Models;

namespace EgitimKayit.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region DbSet'ler - Veritabanı tablolarını temsil eden property'ler
        public DbSet<Statu> Statu { get; set; }
        public DbSet<Birim> Birim { get; set; }
        public DbSet<Personel> Personel { get; set; }
        public DbSet<Dershane> Dershane { get; set; }
        public DbSet<EgitimTip> EgitimTip { get; set; }
        public DbSet<EgitimSablon> EgitimSablon { get; set; }
        public DbSet<EgitimProgram> EgitimProgram { get; set; }
        public DbSet<Egitilen> Egitilen { get; set; }
        public DbSet<Yetki> Yetki { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region EgitimProgram Tablosu Konfigürasyonu
            modelBuilder.Entity<EgitimProgram>(entity =>
            {
                entity.Property(ep => ep.PerTc).HasMaxLength(20);
                entity.Property(ep => ep.YaratanTc).HasMaxLength(20);

                entity.HasOne(ep => ep.EgitimSablon)
                      .WithMany(es => es.EgitimProgramlari)
                      .HasForeignKey(ep => ep.EsId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ep => ep.Ogretmen)
                      .WithMany()
                      .HasForeignKey(ep => ep.PerTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(ep => ep.Yaratan)
                      .WithMany()
                      .HasForeignKey(ep => ep.YaratanTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion

            #region Egitilen Tablosu Konfigürasyonu
            modelBuilder.Entity<Egitilen>(entity =>
            {
                entity.Property(e => e.PerTc).HasMaxLength(20).IsRequired();
                entity.Property(e => e.OgrtTc).HasMaxLength(20);
                entity.Property(e => e.YaratanTc).HasMaxLength(20);

                entity.HasOne(e => e.Katilimci)
                      .WithMany(p => p.KatildigiEgitimler)
                      .HasForeignKey(e => e.PerTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EgitimProgram)
                      .WithMany(ep => ep.Katilimcilar)
                      .HasForeignKey(e => e.EgtProgId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Ogretmen)
                      .WithMany()
                      .HasForeignKey(e => e.OgrtTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Yaratan)
                      .WithMany()
                      .HasForeignKey(e => e.YaratanTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion

            //#region Tüm Modellere PersonelId Ignore Ekle - ÇÖZÜM BURADA
            //modelBuilder.Entity<Personel>().Ignore("PersonelId");
            //modelBuilder.Entity<Dershane>().Ignore("PersonelId");
            //modelBuilder.Entity<EgitimTip>().Ignore("PersonelId");
            //modelBuilder.Entity<EgitimSablon>().Ignore("PersonelId");
            //modelBuilder.Entity<EgitimProgram>().Ignore("PersonelId");
            //modelBuilder.Entity<Egitilen>().Ignore("PersonelId");
            //modelBuilder.Entity<Yetki>().Ignore("PersonelId");
            //modelBuilder.Entity<Statu>().Ignore("PersonelId");
            //modelBuilder.Entity<Birim>().Ignore("PersonelId");
            //#endregion

            #region Personel Tablosu Konfigürasyonu
            modelBuilder.Entity<Personel>(entity =>
            {
                entity.HasIndex(p => p.Tc).IsUnique();
                entity.Property(p => p.Tc).IsRequired().HasMaxLength(20);
                entity.Property(p => p.Sifre).HasMaxLength(60);
                entity.Property(p => p.Adlar).HasMaxLength(100);
                entity.Property(p => p.Tip).HasMaxLength(50);

                // Foreign Key ilişkileri
                entity.HasOne(p => p.StatuBilgi)
                      .WithMany()
                      .HasForeignKey(p => p.Statu)
                      .HasPrincipalKey(s => s.StatuDeger)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Yaratan)
                      .WithMany()
                      .HasForeignKey(p => p.YaratanTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion

            #region Dershane Tablosu Konfigürasyonu
            modelBuilder.Entity<Dershane>(entity =>
            {
                entity.Property(d => d.Ad).IsRequired().HasMaxLength(100);

                entity.HasOne(d => d.Yaratan)
                      .WithMany()
                      .HasForeignKey(d => d.YaratanTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion

            #region EgitimTip Tablosu Konfigürasyonu
            modelBuilder.Entity<EgitimTip>(entity =>
            {
                entity.Property(et => et.Ad).IsRequired().HasMaxLength(100);

                entity.HasOne(et => et.Dershane)
                      .WithMany(d => d.EgitimTipleri)
                      .HasForeignKey(et => et.DerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(et => et.Yaratan)
                      .WithMany()
                      .HasForeignKey(et => et.YaratanTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion

            #region EgitimSablon Tablosu Konfigürasyonu
            modelBuilder.Entity<EgitimSablon>(entity =>
            {
                entity.Property(es => es.Ad).IsRequired().HasMaxLength(200);

                entity.HasOne(es => es.Dershane)
                      .WithMany(d => d.EgitimSablonlari)
                      .HasForeignKey(es => es.DerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(es => es.EgitimTip)
                      .WithMany(et => et.EgitimSablonlari)
                      .HasForeignKey(es => es.EtId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(es => es.Yaratan)
                      .WithMany()
                      .HasForeignKey(es => es.YaratanTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion

            #region EgitimProgram Tablosu Konfigürasyonu
            modelBuilder.Entity<EgitimProgram>(entity =>
            {
                entity.Property(ep => ep.PerTc).HasMaxLength(20);
                entity.Property(ep => ep.YaratanTc).HasMaxLength(20);

                entity.HasOne(ep => ep.EgitimSablon)
                      .WithMany(es => es.EgitimProgramlari)
                      .HasForeignKey(ep => ep.EsId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ep => ep.Ogretmen)
                      .WithMany(p => p.EgitimProgramlari)     // <<< EKLENECEK
                      .HasForeignKey(ep => ep.PerTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(ep => ep.Yaratan)
                      .WithMany()
                      .HasForeignKey(ep => ep.YaratanTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion

            #region Egitilen Tablosu Konfigürasyonu
            modelBuilder.Entity<Egitilen>(entity =>
            {
                entity.Property(e => e.PerTc).HasMaxLength(20).IsRequired();
                entity.Property(e => e.OgrtTc).HasMaxLength(20);
                entity.Property(e => e.YaratanTc).HasMaxLength(20);

                entity.HasOne(e => e.Katilimci)
                      .WithMany(p => p.KatildigiEgitimler)
                      .HasForeignKey(e => e.PerTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EgitimProgram)
                      .WithMany(ep => ep.Katilimcilar)
                      .HasForeignKey(e => e.EgtProgId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Ogretmen)
                      .WithMany()
                      .HasForeignKey(e => e.OgrtTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Yaratan)
                      .WithMany()
                      .HasForeignKey(e => e.YaratanTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion

            #region Yetki Tablosu Konfigürasyonu
            modelBuilder.Entity<Yetki>(entity =>
            {
                entity.Property(y => y.PerTc).HasMaxLength(20).IsRequired();
                entity.Property(y => y.YaratanTc).HasMaxLength(20);

                entity.HasIndex(y => new { y.PerTc, y.DerId }).IsUnique();

                entity.HasOne(y => y.Personel)
                      .WithMany(p => p.Yetkileri)
                      .HasForeignKey(y => y.PerTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(y => y.Dershane)
                      .WithMany(d => d.Yetkiler)
                      .HasForeignKey(y => y.DerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(y => y.Yaratan)
                      .WithMany()
                      .HasForeignKey(y => y.YaratanTc)
                      .HasPrincipalKey(p => p.Tc)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion
        }
    }
}


//using Microsoft.EntityFrameworkCore;
//using EgitimKayit.Models;

//namespace EgitimKayit.Data
//{
//    public class ApplicationDbContext : DbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//            : base(options)
//        {
//        }

//        #region DbSet'ler
//        public DbSet<Statu> Statu { get; set; }
//        public DbSet<Birim> Birim { get; set; }
//        public DbSet<Personel> Personel { get; set; }
//        public DbSet<Dershane> Dershane { get; set; }
//        public DbSet<EgitimTip> EgitimTip { get; set; }
//        public DbSet<EgitimSablon> EgitimSablon { get; set; }
//        public DbSet<EgitimProgram> EgitimProgram { get; set; }
//        public DbSet<Egitilen> Egitilen { get; set; }
//        public DbSet<Yetki> Yetki { get; set; }
//        #endregion

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            #region Personel Tablosu Konfigürasyonu - TEK BLOK
//            modelBuilder.Entity<Personel>(entity =>
//            {
//                // Primary Key
//                entity.HasKey(p => p.Id);

//                // Unique Index
//                entity.HasIndex(p => p.Tc).IsUnique();

//                // Property Konfigürasyonları
//                entity.Property(p => p.Tc).IsRequired().HasMaxLength(20);
//                entity.Property(p => p.Sifre).HasMaxLength(60);
//                entity.Property(p => p.Adlar).HasMaxLength(100);
//                entity.Property(p => p.Tip).HasMaxLength(50);

//                // Foreign Key ilişkileri - Statu
//                entity.HasOne(p => p.StatuBilgi)
//                      .WithMany()
//                      .HasForeignKey(p => p.Statu)
//                      .HasPrincipalKey(s => s.StatuDeger)
//                      .OnDelete(DeleteBehavior.SetNull);

//                // Foreign Key ilişkileri - Yaratan
//                entity.HasOne(p => p.Yaratan)
//                      .WithMany()
//                      .HasForeignKey(p => p.YaratanTc)
//                      .HasPrincipalKey(p => p.Tc)
//                      .OnDelete(DeleteBehavior.SetNull);
//            });
//            #endregion

//            #region Dershane Tablosu Konfigürasyonu
//            modelBuilder.Entity<Dershane>(entity =>
//            {
//                entity.Property(d => d.Ad).IsRequired().HasMaxLength(100);

//                entity.HasOne(d => d.Yaratan)
//                      .WithMany()
//                      .HasForeignKey(d => d.YaratanTc)
//                      .HasPrincipalKey(p => p.Tc)
//                      .OnDelete(DeleteBehavior.SetNull);
//            });
//            #endregion

//            #region EgitimTip Tablosu Konfigürasyonu
//            modelBuilder.Entity<EgitimTip>(entity =>
//            {
//                entity.Property(et => et.Ad).IsRequired().HasMaxLength(100);

//                entity.HasOne(et => et.Dershane)
//                      .WithMany(d => d.EgitimTipleri)
//                      .HasForeignKey(et => et.DerId)
//                      .OnDelete(DeleteBehavior.Restrict);

//                entity.HasOne(et => et.Yaratan)
//                      .WithMany()
//                      .HasForeignKey(et => et.YaratanTc)
//                      .HasPrincipalKey(p => p.Tc)
//                      .OnDelete(DeleteBehavior.SetNull);
//            });
//            #endregion

//            #region EgitimSablon Tablosu Konfigürasyonu
//            modelBuilder.Entity<EgitimSablon>(entity =>
//            {
//                entity.Property(es => es.Ad).IsRequired().HasMaxLength(200);

//                entity.HasOne(es => es.Dershane)
//                      .WithMany(d => d.EgitimSablonlari)
//                      .HasForeignKey(es => es.DerId)
//                      .OnDelete(DeleteBehavior.Restrict);

//                entity.HasOne(es => es.EgitimTip)
//                      .WithMany(et => et.EgitimSablonlari)
//                      .HasForeignKey(es => es.EtId)
//                      .OnDelete(DeleteBehavior.Restrict);

//                entity.HasOne(es => es.Yaratan)
//                      .WithMany()
//                      .HasForeignKey(es => es.YaratanTc)
//                      .HasPrincipalKey(p => p.Tc)
//                      .OnDelete(DeleteBehavior.SetNull);
//            });
//            #endregion

//            #region EgitimProgram Tablosu Konfigürasyonu
//            modelBuilder.Entity<EgitimProgram>(entity =>
//            {
//                entity.Property(ep => ep.PerTc).HasMaxLength(20);
//                entity.Property(ep => ep.YaratanTc).HasMaxLength(20);

//                entity.HasOne(ep => ep.EgitimSablon)
//                      .WithMany(es => es.EgitimProgramlari)
//                      .HasForeignKey(ep => ep.EsId)
//                      .OnDelete(DeleteBehavior.Restrict);

//                entity.HasOne(ep => ep.Ogretmen)
//                      .WithMany()
//                      .HasForeignKey(ep => ep.PerTc)
//                      .HasPrincipalKey(p => p.Tc)
//                      .OnDelete(DeleteBehavior.SetNull);

//                entity.HasOne(ep => ep.Yaratan)
//                      .WithMany()
//                      .HasForeignKey(ep => ep.YaratanTc)
//                      .HasPrincipalKey(p => p.Tc)
//                      .OnDelete(DeleteBehavior.SetNull);
//            });
//            #endregion

//            #region Egitilen Tablosu Konfigürasyonu
//            modelBuilder.Entity<Egitilen>(entity =>
//            {
//                entity.Property(e => e.PerTc).HasMaxLength(20).IsRequired();
//                entity.Property(e => e.OgrtTc).HasMaxLength(20);
//                entity.Property(e => e.YaratanTc).HasMaxLength(20);

//                entity.HasOne(e => e.Katilimci)
//                      .WithMany(p => p.KatildigiEgitimler)
//                      .HasForeignKey(e => e.PerTc)
//                      .HasPrincipalKey(p => p.Tc)
//                      .OnDelete(DeleteBehavior.Restrict);

//                entity.HasOne(e => e.EgitimProgram)
//                      .WithMany(ep => ep.Katilimcilar)
//                      .HasForeignKey(e => e.EgtProgId)
//                      .OnDelete(DeleteBehavior.Restrict);

//                entity.HasOne(e => e.Ogretmen)
//                      .WithMany()
//                      .HasForeignKey(e => e.OgrtTc)
//                      .HasPrincipalKey(p => p.Tc)
//                      .OnDelete(DeleteBehavior.SetNull);

//                entity.HasOne(e => e.Yaratan)
//                      .WithMany()
//                      .HasForeignKey(e => e.YaratanTc)
//                      .HasPrincipalKey(p => p.Tc)
//                      .OnDelete(DeleteBehavior.SetNull);
//            });
//            #endregion

//            #region Yetki Tablosu Konfigürasyonu
//            modelBuilder.Entity<Yetki>(entity =>
//            {
//                entity.Property(y => y.PerTc).HasMaxLength(20).IsRequired();
//                entity.Property(y => y.YaratanTc).HasMaxLength(20);

//                // Composite unique index
//                entity.HasIndex(y => new { y.PerTc, y.DerId }).IsUnique();

//                entity.HasOne(y => y.Personel)
//                      .WithMany(p => p.Yetkileri)
//                      .HasForeignKey(y => y.PerTc)
//                      .HasPrincipalKey(p => p.Tc)
//                      .OnDelete(DeleteBehavior.Cascade);

//                entity.HasOne(y => y.Dershane)
//                      .WithMany(d => d.Yetkiler)
//                      .HasForeignKey(y => y.DerId)
//                      .OnDelete(DeleteBehavior.Cascade);

//                entity.HasOne(y => y.Yaratan)
//                      .WithMany()
//                      .HasForeignKey(y => y.YaratanTc)
//                      .HasPrincipalKey(p => p.Tc)
//                      .OnDelete(DeleteBehavior.SetNull);
//            });
//            #endregion

//            #region Statu Tablosu Konfigürasyonu - EKSİK OLAN
//            modelBuilder.Entity<Statu>(entity =>
//            {
//                entity.HasKey(s => s.Id);
//                entity.HasIndex(s => s.StatuDeger).IsUnique();
//                entity.Property(s => s.Anlam).HasMaxLength(50);
//            });
//            #endregion

//            #region Birim Tablosu Konfigürasyonu - EKSİK OLAN
//            modelBuilder.Entity<Birim>(entity =>
//            {
//                entity.HasKey(b => b.Id);
//                entity.Property(b => b.Ad).HasMaxLength(100);
//            });
//            #endregion
//        }
//    }
//}



//// 📚 Tablolar
//public DbSet<Personel> Personeller { get; set; }
//public DbSet<Statu> Statuler { get; set; }
//public DbSet<Birim> Birimler { get; set; }


//public DbSet<Kitap> Kitaplar { get; set; }
//public DbSet<Kategori> Kategoriler { get; set; }
//public DbSet<Odunc> Oduncler { get; set; }
//public DbSet<Stok> Stoklar { get; set; }

//protected override void OnModelCreating(ModelBuilder modelBuilder)
//{
//    base.OnModelCreating(modelBuilder);

//    // 📌 Personel → Statu ilişkisi
//    modelBuilder.Entity<Personel>()
//        .HasOne(p => p.StatuBilgi)
//        .WithMany(s => s.Personeller) // 👈 Statu modelinde ICollection<Personel> Personeller olmalı
//        .HasForeignKey(p => p.Statu)
//        .HasPrincipalKey(s => s.StatuDeger)
//        .OnDelete(DeleteBehavior.Restrict);

//    // 📌 Birim → kendi kendine bağlı üst-alt ilişki
//    modelBuilder.Entity<Birim>()
//        .HasOne(b => b.UstBirim)
//        .WithMany(b => b.AltBirimler)
//        .HasForeignKey(b => b.UstId)
//        .OnDelete(DeleteBehavior.Restrict);

//    // 📌 Personel tablosu için ek özellikler (örnek: varsayılan değerler)
//    modelBuilder.Entity<Personel>()
//        .Property(p => p.Aktif)
//        .HasDefaultValue(1);

//    modelBuilder.Entity<Personel>()
//        .Property(p => p.Tarih)
//        .HasDefaultValueSql("GETDATE()");

//    // 📕 Kitap-Kategori ilişkisi
//    //modelBuilder.Entity<Kitap>()
//    //    .HasOne(k => k.Kategori)
//    //    .WithMany() // Kategori tarafında ICollection<Kitap> yoksa bu şekilde kalmalı
//    //    .HasForeignKey(k => k.KategoriId)
//    //    .OnDelete(DeleteBehavior.SetNull);

//    // 📗 Ödünç-Kitap ilişkisi
//    modelBuilder.Entity<Kitap>()
//        .HasOne(k => k.Kategori)
//        .WithMany(c => c.Kitaplar)
//        .HasForeignKey(k => k.kategoriId)
//        .HasPrincipalKey(c => c.Id)   // opsiyonel ama netlik için koydum
//        .OnDelete(DeleteBehavior.SetNull);

//    modelBuilder.Entity<Odunc>()
//        .HasOne(o => o.Kitap)
//        .WithMany()
//        .HasForeignKey(o => o.KitapId)
//        .OnDelete(DeleteBehavior.Cascade);

//    modelBuilder.Entity<Odunc>()
//        .HasOne(o => o.Personel)
//        .WithMany()
//        .HasForeignKey(o => o.PersonelId)
//        .OnDelete(DeleteBehavior.SetNull);

//    // 📌 Tabloların isimlerini veritabanına uygun hale getir
//    modelBuilder.Entity<Personel>().ToTable("Personel");
//    modelBuilder.Entity<Birim>().ToTable("Birim");
//    modelBuilder.Entity<Statu>().ToTable("Statu");
//    //modelBuilder.Entity<Kitap>().ToTable("Kitap");

//    modelBuilder.Entity<Kitap>()
//.ToTable("Kitap", tb => tb.HasTrigger("trg_GenerateBarcode")); // Doğru kullanım!

//    modelBuilder.Entity<Kategori>().ToTable("Kategori");
//    modelBuilder.Entity<Odunc>().ToTable("Odunc");
//    modelBuilder.Entity<Stok>().ToTable("Stok");
//}