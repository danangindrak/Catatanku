using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebAuthNet5.Models;
using System.Linq;
using System.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;


namespace WebAuthNet5.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _cfg;
        private readonly IHttpClientFactory _clientFactory;
        private string _client_id;
        private string _redirect_uri;
        private string _end_session;
        private string _domain;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _cfg = configuration;
            _clientFactory = clientFactory;


            _client_id = _cfg.GetSection("keycloak").GetSection("client_id").Value;
            _redirect_uri = _cfg.GetSection("keycloak").GetSection("redirect_uri").Value;
            _end_session = _cfg.GetSection("keycloak").GetSection("end_point").GetSection("end_session").Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        [HttpGet("sign_off")]
        public async Task<IActionResult> signOff()
        {
            _domain = $"{Request.Scheme}://{Request.Host.Value}";
            await ControllerContext.HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            await ControllerContext.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Response.Cookies.Delete(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Response.Cookies.Delete(OpenIdConnectDefaults.AuthenticationScheme);
            HttpContext.Response.Cookies.Delete(_cfg.GetSection("keycloak").GetSection("cookies_name").Value);

            return View("Index");

        }




    }
}
