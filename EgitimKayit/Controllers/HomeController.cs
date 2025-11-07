using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EgitimKayit.Services;
using EgitimKayit.Models;

namespace EgitimKayit.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthService _authService;

        public HomeController(ILogger<HomeController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        #region Ana Sayfa - Login kontrolüne göre yönlendirme
        public IActionResult Index()
        {
            _logger.LogInformation("Home/Index action'ý çaðrýldý - IP: {RemoteIpAddress}", HttpContext.Connection.RemoteIpAddress?.ToString());

            var personelTc = HttpContext.Session.GetString("PersonelTc");
            var personelTip = HttpContext.Session.GetString("PersonelTip");

            if (string.IsNullOrEmpty(personelTc))
            {
                _logger.LogDebug("Kullanýcý login olmamýþ - Login sayfasýna yönlendiriliyor");
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("Kullanýcý login olmuþ - Dashboard'a yönlendiriliyor. TC: {PersonelTc}, Tip: {PersonelTip}",
                personelTc, personelTip);

            return View("Dashboard");
        }
        #endregion

        #region Dashboard - Giriþ yapmýþ kullanýcý için ana sayfa
        public IActionResult Dashboard()
        {
            Console.WriteLine($"=== DASHBOARD ÇAÐRILDI ===");

            var personelTc = HttpContext.Session.GetString("PersonelTc");
            var personelAd = HttpContext.Session.GetString("PersonelAd");
            var personelTip = HttpContext.Session.GetString("PersonelTip");

            Console.WriteLine($"Session Tc: {personelTc}");
            Console.WriteLine($"Session Ad: {personelAd}");
            Console.WriteLine($"Session Tip: {personelTip}");

            if (string.IsNullOrEmpty(personelTc))
            {
                Console.WriteLine($"Session BULUNAMADI - Login sayfasýna yönlendiriliyor");
                _logger.LogWarning("Dashboard'a eriþim denendi ancak kullanýcý login deðil");
                return RedirectToAction("Login", "Account");
            }

            ViewBag.PersonelAd = personelAd;
            ViewBag.PersonelTip = personelTip;

            _logger.LogInformation("Dashboard görüntüleniyor - Kullanýcý: {PersonelAd}, TC: {PersonelTc}",
                personelAd, personelTc);

            return View();
        }
        //public IActionResult Dashboard()
        //{
        //    _logger.LogDebug("Home/Dashboard action'ý çaðrýldý");

        //    var personelTc = HttpContext.Session.GetString("PersonelTc");
        //    if (string.IsNullOrEmpty(personelTc))
        //    {
        //        _logger.LogWarning("Dashboard'a eriþim denendi ancak kullanýcý login deðil");
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var personelAd = HttpContext.Session.GetString("PersonelAd");
        //    var personelTip = HttpContext.Session.GetString("PersonelTip");

        //    ViewBag.PersonelAd = personelAd;
        //    ViewBag.PersonelTip = personelTip;

        //    // DÜZELTME: ViewBag yerine local deðiþkenleri kullan
        //    _logger.LogInformation("Dashboard görüntüleniyor - Kullanýcý: {PersonelAd}, TC: {PersonelTc}",
        //        personelAd, personelTc);

        //    return View();
        //}
        #endregion

        #region Mevcut Action'larý Koruyoruz
        public IActionResult Privacy()
        {
            _logger.LogDebug("Privacy sayfasý görüntüleniyor");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error sayfasý görüntüleniyor - RequestId: {RequestId}", requestId);

            return View(new ErrorViewModel { RequestId = requestId });
        }
        #endregion
    }
}