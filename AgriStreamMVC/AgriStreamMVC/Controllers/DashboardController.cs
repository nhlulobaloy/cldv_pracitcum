using AgriStreamMVC.Data;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriStreamMVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public DashboardController(AppDbContext context, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IActionResult> Index()
        {
            // 1. Check SQL Database
            var users = await _context.Users.ToListAsync();
            ViewBag.UserCount = users.Count;

            // 2. Call Azure Function (GET Telemetry)
            string getUrl = _config["FunctionUrls:GetTelemetry"] ?? "";
            string telemetryData = "No telemetry data found.";

            if (!string.IsNullOrEmpty(getUrl) && !getUrl.Contains("PASTE"))
            {
                try
                {
                    var response = await _httpClient.GetAsync(getUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        telemetryData = await response.Content.ReadAsStringAsync();
                    }
                }
                catch { }
            }

            ViewBag.TelemetryData = telemetryData;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string connString = _config["AzureStorage:ConnectionString"] ?? "";
                string containerName = _config["AzureStorage:ContainerName"] ?? "";

                var blobServiceClient = new BlobServiceClient(connString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(file.FileName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TriggerIrrigation()
        {
            string postUrl = _config["FunctionUrls:TriggerIrrigation"] ?? "";

            if (!string.IsNullOrEmpty(postUrl) && !postUrl.Contains("PASTE"))
            {
                try
                {
                    await _httpClient.PostAsync(postUrl, null);
                }
                catch { }
            }

            return RedirectToAction("Index");
        }
    }
}