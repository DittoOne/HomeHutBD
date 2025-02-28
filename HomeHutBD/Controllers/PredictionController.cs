using Microsoft.AspNetCore.Mvc;
using HomeHutBD.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;

namespace HomeHutBD.Controllers
{
    public class PredictionController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _flaskApiBaseUrl = "http://127.0.0.1:5000";
        private readonly ILogger<PredictionController> _logger;
        private readonly int _maxRetries = 3;

        public PredictionController(ILogger<PredictionController> logger)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Check if Flask API is available on page load
            try
            {
                var response = await _httpClient.GetAsync($"{_flaskApiBaseUrl}/health_check");
                if (response.IsSuccessStatusCode)
                {
                    ViewBag.ApiStatus = "API is running";
                }
                else
                {
                    ViewBag.ApiStatus = "API is not responding correctly";
                }
            }
            catch
            {
                ViewBag.ApiStatus = "API is not available";
            }

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

            int retryCount = 0;
            while (retryCount < _maxRetries)
            {
                try
                {
                    var response = await _httpClient.PostAsync($"{_flaskApiBaseUrl}/predict", content);
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Response from Flask API: {jsonResponse}");

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            retryCount++;
                            await Task.Delay(1000); // Wait 1 second before retrying
                            continue;
                        }

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
                        ViewBag.Error = predictionResponse?.Error ?? "Invalid response from prediction service.";
                    }

                    break; // Exit the loop if request was successful
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError($"HTTP Request Error (Attempt {retryCount + 1}/{_maxRetries}): {ex.Message}");

                    if (retryCount < _maxRetries - 1)
                    {
                        retryCount++;
                        await Task.Delay(1000); // Wait 1 second before retrying
                    }
                    else
                    {
                        ViewBag.Error = "Unable to connect to prediction service. Please try again later.";
                        break;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"JSON Parsing Error: {ex.Message}");
                    ViewBag.Error = "Error processing service response. Please try again later.";
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected Error: {ex.Message}");
                    ViewBag.Error = "An unexpected error occurred. Please try again later.";
                    break;
                }
            }

            return View("Index", model);
        }
    }
}