using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Orneholm.AIJukebox.Web.Models;
using SpotifyAPI.Web.Auth;

namespace Orneholm.AIJukebox.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly CredentialsAuth _credentialsAuth;

        public HomeController(TelemetryClient telemetryClient, CredentialsAuth credentialsAuth)
        {
            _telemetryClient = telemetryClient;
            _credentialsAuth = credentialsAuth;
        }

        //[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<HomeIndexViewModel>> Index()
        {
            var spotifyToken = await _credentialsAuth.GetToken();
            var viewModel = new HomeIndexViewModel
            {
                SpotifyBearerToken = spotifyToken.AccessToken
            };

            return View(viewModel);
        }
    }
}
