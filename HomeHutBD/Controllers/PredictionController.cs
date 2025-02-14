using Microsoft.AspNetCore.Mvc;

namespace HomeHutBD.Controllers
{
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using HomeHutBD.Models;
    using Newtonsoft.Json;

    public class PredictionController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _flaskApiUrl = "http://localhost:5000/predict";

        public PredictionController()
        {
            _httpClient = new HttpClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        private async Task<bool> IsFlaskRunning()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:5000/");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IActionResult> Predict(PropertyPredictionModel model)
        {
            if (!await IsFlaskRunning())
            {
                ViewBag.Error = "Flask API is not running.";
                return View("Index", model);
            }
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_flaskApiUrl, content);
                var result = await response.Content.ReadAsStringAsync();

                var predictionResponse = JsonConvert.DeserializeObject<PredictionResponse>(result);

                ViewBag.PredictedPrice = predictionResponse.PredictedPrice;
                ViewBag.Error = predictionResponse.Error;

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Index", model);
            }
        }
    }
}
