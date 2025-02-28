using Microsoft.AspNetCore.Mvc;
using HomeHutBD.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HomeHutBD.Controllers
{
    public class PredictionController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _flaskApiBaseUrl = "http://127.0.0.1:5000";
        private readonly ILogger<PredictionController> _logger;

        public PredictionController(ILogger<PredictionController> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Predict(PropertyPredictionModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Calculate beds to baths ratio
            double bedsToBathsRatio = (double)model.beds / model.bath;

            var requestDict = new Dictionary<string, object>
            {
                { "address", model.address },
                { "type", model.type },
                { "size(sqft)", model.size },
                { "beds", model.beds },
                { "bath", model.bath },
                { "beds_to_baths_ratio", bedsToBathsRatio }
            };

            var jsonRequest = JsonConvert.SerializeObject(requestDict);
            _logger.LogInformation($"Request payload: {jsonRequest}");

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_flaskApiBaseUrl}/predict", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response from Flask API: {jsonResponse}");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"Error from prediction service: {response.StatusCode}";
                    return View("Index", model);
                }

                var predictionResponse = JsonConvert.DeserializeObject<PredictionResponse>(jsonResponse);

                if (predictionResponse != null && predictionResponse.Status == "success" && predictionResponse.PredictedPrice.HasValue)
                {
                    _logger.LogInformation($"Setting ViewBag.PredictedPrice to: {predictionResponse.PredictedPrice}");
                    ViewBag.PredictedPrice = predictionResponse.PredictedPrice.Value;
                }
                else
                {
                    _logger.LogError($"Invalid prediction response: {jsonResponse}");
                    ViewBag.Error = "Invalid response from prediction service.";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP Request Error: {ex.Message}");
                ViewBag.Error = "Unable to connect to prediction service. Please try again later.";
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON Parsing Error: {ex.Message}");
                ViewBag.Error = "Error processing service response. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected Error: {ex.Message}");
                ViewBag.Error = "An unexpected error occurred. Please try again later.";
            }

            return View("Index", model);
        }
    }
}