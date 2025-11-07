using EgitimKayit.Data;
using EgitimKayit.Models;
using EgitimKayit.Services;
using EgitimKayit.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EgitimKayit.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

       
        public AccountController(IAuthService authService, ILogger<AccountController> logger, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _authService = authService;
            _logger = logger;
            _context = context;
            _env = env;
        }



        //yeni kullanıcı ekleme
        #region Kullanıcı Kayıt - GET
        [HttpGet]
        public IActionResult CreateUser()
        {
            var model = new CreateUserViewModel();

            // Dropdown listelerini doldur
            model.StatuList = _context.Statu.ToList();
            model.Birim1List = _context.Birim.Where(b => b.BirimSeviye == 1).ToList();

            // Varsayılan tipi ayarla
            var currentUserTip = HttpContext.Session.GetString("PersonelTip");
            if (currentUserTip == "sorumlu" || currentUserTip == "yonetici")
            {
                model.Tip = "ogrenci"; // Default değer
            }
            else
            {
                model.Tip = "ogrenci"; // Diğer kullanıcılar için sadece öğrenci
            }

            return View(model);
        }
        #endregion

        #region Kullanıcı Kayıt - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model, IFormFile? ResimDosya)
        {
            // Dropdown listelerini tekrar doldur
            model.StatuList = _context.Statu.ToList();
            model.Birim1List = _context.Birim.Where(b => b.BirimSeviye == 1).ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // TC kontrolü
                if (await _context.Personel.AnyAsync(p => p.Tc == model.Tc))
                {
                    ModelState.AddModelError(nameof(model.Tc), "Bu TC ile kayıtlı kullanıcı var.");
                    return View(model);
                }

                // Kullanıcı tipi kontrolü
                var currentUserTip = HttpContext.Session.GetString("PersonelTip");
                if (currentUserTip != "sorumlu" && currentUserTip != "yonetici")
                {
                    model.Tip = "ogrenci"; // Sadece sorumlu ve yönetici farklı tip seçebilir
                }

                // Resim işlemi
                string? resimAdi = null;
                if (ResimDosya != null && ResimDosya.Length > 0)
                {
                    var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var resimlerPath = Path.Combine(wwwrootPath, "resimler");

                    if (!Directory.Exists(resimlerPath))
                        Directory.CreateDirectory(resimlerPath);

                    var uzanti = Path.GetExtension(ResimDosya.FileName);
                    resimAdi = $"{model.Tc}_{DateTime.Now:yyyyMMddHHmmss}{uzanti}";
                    var kayitYolu = Path.Combine(resimlerPath, resimAdi);

                    using (var stream = new FileStream(kayitYolu, FileMode.Create))
                    {
                        await ResimDosya.CopyToAsync(stream);
                    }
                }

                // Birim adlarını al
                string? birim1Ad = null;
                string? birim2Ad = null;
                string? birim3Ad = null;

                if (model.Birim1Id.HasValue)
                {
                    var birim1 = await _context.Birim.FindAsync(model.Birim1Id.Value);
                    birim1Ad = birim1?.Ad;
                }

                if (model.Birim2Id.HasValue)
                {
                    var birim2 = await _context.Birim.FindAsync(model.Birim2Id.Value);
                    birim2Ad = birim2?.Ad;
                }

                if (model.Birim3Id.HasValue)
                {
                    var birim3 = await _context.Birim.FindAsync(model.Birim3Id.Value);
                    birim3Ad = birim3?.Ad;
                }

                // Yeni kullanıcı oluştur
                var yeniKullanici = new Personel
                {
                    Tc = model.Tc,
                    Adlar = model.Adlar,
                    Statu = model.StatuId,
                    Kuvvet = model.Kuvvet,
                    Sinif = model.Sinif,
                    Sicil = model.Sicil,
                    Birim1 = birim1Ad,
                    Birim2 = birim2Ad,
                    Birim3 = birim3Ad,
                    Resim = resimAdi,
                    Tip = model.Tip ?? "ogrenci",
                    Sifre = BCrypt.Net.BCrypt.HashPassword(model.Sifre), // ✨ BCRYPT HASH
                    Aktif = 1,
                    Tarih = DateTime.Now,
                    Alan = null,
                    YaratanTc = HttpContext.Session.GetString("PersonelTc") // Oturum açıksa
                };

                _context.Personel.Add(yeniKullanici);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu!";

                // Yönlendirme
                if (!string.IsNullOrEmpty(currentUserTip))
                {
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı oluşturma hatası - TC: {Tc}", model.Tc);
                ModelState.AddModelError("", "Kullanıcı oluşturulurken hata oluştu: " + ex.Message);
                return View(model);
            }
        }
        #endregion

        #region Birim AJAX Metodları
        [HttpGet]
        public JsonResult GetBirim2(int ustId)
        {
            var liste = _context.Birim
                .Where(b => b.UstId == ustId && b.BirimSeviye == 2)
                .Select(b => new { id = b.Id, ad = b.Ad })
                .ToList();
            return Json(liste);
        }

        [HttpGet]
        public JsonResult GetBirim3(int ustId)
        {
            var liste = _context.Birim
                .Where(b => b.UstId == ustId && b.BirimSeviye == 3)
                .Select(b => new { id = b.Id, ad = b.Ad })
                .ToList();
            return Json(liste);
        }
        #endregion






        #region Login Sayfası - GET
        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogDebug("Login sayfası GET isteği - IP: {RemoteIpAddress}", HttpContext.Connection.RemoteIpAddress?.ToString());

            // Eğer zaten login olmuşsa dashboard'a yönlendir
            var personelTc = HttpContext.Session.GetString("PersonelTc");
            if (!string.IsNullOrEmpty(personelTc))
            {
                _logger.LogInformation("Zaten login olmuş kullanıcı dashboard'a yönlendiriliyor - TC: {PersonelTc}", personelTc);
                return RedirectToAction("Dashboard", "Home");
            }

            return View();
        }
        #endregion

        //#region Login İşlemi - POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            Console.WriteLine($"=== LOGIN BAŞLADI ===");
            Console.WriteLine($"TC: {model.Tc}");
            Console.WriteLine($"Şifre uzunluk: {model.Sifre?.Length}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine($"Model validation hatası");
                return View(model);
            }

            try
            {
                Console.WriteLine($"AuthService.LoginAsync çağrılıyor...");
                var personel = await _authService.LoginAsync(model);

                if (personel == null)
                {
                    Console.WriteLine($"Personel null döndü - TC veya şifre hatalı");
                    ModelState.AddModelError("", "TC Kimlik No veya şifre hatalı!");
                    return View(model);
                }

                Console.WriteLine($"Login başarılı - Personel: {personel.Adlar}");

                #region Session'a kullanıcı bilgilerini kaydet - BU KISIM EKSİKTİ!
                HttpContext.Session.SetString("PersonelTc", personel.Tc);
                HttpContext.Session.SetString("PersonelAd", personel.Adlar ?? "");
                HttpContext.Session.SetString("PersonelTip", personel.Tip ?? "");
                HttpContext.Session.SetInt32("PersonelId", personel.Id);

                Console.WriteLine($"Session kaydedildi - TC: {personel.Tc}, Ad: {personel.Adlar}, Tip: {personel.Tip}");
                #endregion

                _logger.LogInformation("Başarılı login - Kullanıcı: {PersonelAd}, TC: {PersonelTc}, Tip: {PersonelTip}",
                    personel.Adlar, personel.Tc, personel.Tip);

                return RedirectToAction("Dashboard", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HATA: {ex.Message}");
                _logger.LogError(ex, "Login işlemi sırasında hata - TC: {Tc}", model.Tc);
                ModelState.AddModelError("", "Giriş işlemi sırasında bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login(LoginViewModel model)
        //{
        //    _logger.LogInformation("Login POST isteği - TC: {Tc}", model.Tc);

        //    if (!ModelState.IsValid)
        //    {
        //        _logger.LogWarning("Login model validation hatası - TC: {Tc}", model.Tc);
        //        return View(model);
        //    }

        //    try
        //    {
        //        var personel = await _authService.LoginAsync(model);

        //        if (personel == null)
        //        {
        //            _logger.LogWarning("Başarısız login denemesi - TC: {Tc}", model.Tc);
        //            ModelState.AddModelError("", "TC Kimlik No veya şifre hatalı!");
        //            return View(model);
        //        }

        //        #region Session'a kullanıcı bilgilerini kaydet
        //        HttpContext.Session.SetString("PersonelTc", personel.Tc);
        //        HttpContext.Session.SetString("PersonelAd", personel.Adlar ?? "");
        //        HttpContext.Session.SetString("PersonelTip", personel.Tip ?? "");
        //        HttpContext.Session.SetInt32("PersonelId", personel.Id);
        //        #endregion

        //        _logger.LogInformation("Başarılı login - Kullanıcı: {PersonelAd}, TC: {PersonelTc}, Tip: {PersonelTip}",
        //            personel.Adlar, personel.Tc, personel.Tip);

        //        return RedirectToAction("Dashboard", "Home");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Login işlemi sırasında hata - TC: {Tc}", model.Tc);
        //        ModelState.AddModelError("", "Giriş işlemi sırasında bir hata oluştu: " + ex.Message);
        //        return View(model);
        //    }
        //}
        //#endregion

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login(LoginViewModel model)
        //{
        //    // ✨ BASİT DEBUG
        //    Console.WriteLine($"=== LOGIN BAŞLADI ===");
        //    Console.WriteLine($"TC: {model.Tc}");
        //    Console.WriteLine($"Şifre uzunluk: {model.Sifre?.Length}");

        //    if (!ModelState.IsValid)
        //    {
        //        Console.WriteLine($"Model validation hatası");
        //        return View(model);
        //    }

        //    try
        //    {
        //        Console.WriteLine($"AuthService.LoginAsync çağrılıyor...");
        //        var personel = await _authService.LoginAsync(model);

        //        if (personel == null)
        //        {
        //            Console.WriteLine($"Personel null döndü - TC veya şifre hatalı");
        //            ModelState.AddModelError("", "TC Kimlik No veya şifre hatalı!");
        //            return View(model);
        //        }

        //        Console.WriteLine($"Login başarılı - Personel: {personel.Adlar}");

        //        // Session işlemleri...
        //        return RedirectToAction("Dashboard", "Home");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"HATA: {ex.Message}");
        //        ModelState.AddModelError("", "Giriş işlemi sırasında bir hata oluştu: " + ex.Message);
        //        return View(model);
        //    }
        //}

        #region Logout İşlemi
        public IActionResult Logout()
        {
            var personelTc = HttpContext.Session.GetString("PersonelTc");
            var personelAd = HttpContext.Session.GetString("PersonelAd");

            HttpContext.Session.Clear();

            _logger.LogInformation("Kullanıcı logout oldu - Kullanıcı: {PersonelAd}, TC: {PersonelTc}", personelAd, personelTc);

            TempData["SuccessMessage"] = "Başarıyla çıkış yapıldı.";
            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region Şifre Değiştirme - GET
        [HttpGet]
        public IActionResult ChangePassword()
        {
            _logger.LogDebug("ChangePassword GET isteği");

            var personelTc = HttpContext.Session.GetString("PersonelTc");
            if (string.IsNullOrEmpty(personelTc))
            {
                _logger.LogWarning("ChangePassword'a erişim denendi ancak kullanıcı login değil");
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        #endregion

        #region Şifre Değiştirme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var personelTc = HttpContext.Session.GetString("PersonelTc");
            _logger.LogInformation("ChangePassword POST isteği - TC: {PersonelTc}", personelTc);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ChangePassword model validation hatası - TC: {PersonelTc}", personelTc);
                return View(model);
            }

            if (string.IsNullOrEmpty(personelTc))
            {
                _logger.LogWarning("ChangePassword POST için kullanıcı login değil");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _authService.ChangePasswordAsync(personelTc, model.CurrentPassword, model.NewPassword);

                if (result)
                {
                    _logger.LogInformation("Şifre başarıyla değiştirildi - TC: {PersonelTc}", personelTc);
                    TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    _logger.LogWarning("Şifre değiştirme başarısız - Mevcut şifre hatalı - TC: {PersonelTc}", personelTc);
                    ModelState.AddModelError("", "Mevcut şifre hatalı!");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre değiştirme işleminde hata - TC: {PersonelTc}", personelTc);
                ModelState.AddModelError("", "Şifre değiştirme işlemi sırasında bir hata oluştu.");
                return View(model);
            }
        }
        #endregion
    }
}


//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Http;
//using EgitimKayit.Services;
//using EgitimKayit.ViewModels; // ← BU USING EKLENDİ
//using EgitimKayit.Models;
//using Microsoft.Extensions.Logging; // ← Logger için

//namespace EgitimKayit.Controllers
//{
//    public class AccountController : Controller
//    {
//        private readonly IAuthService _authService;
//        private readonly ILogger<AccountController> _logger;

//        public AccountController(IAuthService authService, ILogger<AccountController> logger)
//        {
//            _authService = authService;
//            _logger = logger;
//        }



//        #region Login Sayfası - GET
//        [HttpGet]
//        public IActionResult Login()
//        {
//            _logger.LogDebug("Login sayfası GET isteği - IP: {RemoteIpAddress}", HttpContext.Connection.RemoteIpAddress?.ToString());

//            // Eğer zaten login olmuşsa dashboard'a yönlendir
//            var personelTc = HttpContext.Session.GetString("PersonelTc");
//            if (!string.IsNullOrEmpty(personelTc))
//            {
//                _logger.LogInformation("Zaten login olmuş kullanıcı dashboard'a yönlendiriliyor - TC: {PersonelTc}", personelTc);
//                return RedirectToAction("Dashboard", "Home");
//            }

//            return View();
//        }
//        #endregion


//        // Logout action'ında TempData mesajı 
//        public IActionResult Logout()
//        {
//            var personelTc = HttpContext.Session.GetString("PersonelTc");
//            var personelAd = HttpContext.Session.GetString("PersonelAd");

//            HttpContext.Session.Clear();

//            _logger.LogInformation("Kullanıcı logout oldu - Kullanıcı: {PersonelAd}, TC: {PersonelTc}", personelAd, personelTc);

//            TempData["SuccessMessage"] = "Başarıyla çıkış yapıldı.";
//            return RedirectToAction("Login", "Account");
//        }


//        // ... diğer metodlar aynı kalacak ...

//        #region Şifre Değiştirme - POST
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
//        {
//            var personelTc = HttpContext.Session.GetString("PersonelTc");
//            _logger.LogInformation("ChangePassword POST isteği - TC: {PersonelTc}", personelTc);

//            if (!ModelState.IsValid)
//            {
//                _logger.LogWarning("ChangePassword model validation hatası - TC: {PersonelTc}", personelTc);
//                return View(model);
//            }

//            if (string.IsNullOrEmpty(personelTc))
//            {
//                _logger.LogWarning("ChangePassword POST için kullanıcı login değil");
//                return RedirectToAction("Login", "Account");
//            }

//            try
//            {
//                var result = await _authService.ChangePasswordAsync(personelTc, model.CurrentPassword, model.NewPassword);

//                if (result)
//                {
//                    _logger.LogInformation("Şifre başarıyla değiştirildi - TC: {PersonelTc}", personelTc);
//                    TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
//                    return RedirectToAction("Dashboard", "Home");
//                }
//                else
//                {
//                    _logger.LogWarning("Şifre değiştirme başarısız - Mevcut şifre hatalı - TC: {PersonelTc}", personelTc);
//                    ModelState.AddModelError("", "Mevcut şifre hatalı!");
//                    return View(model);
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Şifre değiştirme işleminde hata - TC: {PersonelTc}", personelTc);
//                ModelState.AddModelError("", "Şifre değiştirme işlemi sırasında bir hata oluştu.");
//                return View(model);
//            }
//        }
//        #endregion
//    }
//}