using Microsoft.AspNetCore.Mvc;
using HomeHutBD.Models;
using Newtonsoft.Json;
namespace HomeHutBD.Controllers
{
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class PredictionController : Controller
    {
        private readonly HttpClient _httpClient;
        // private readonly string _flaskApiUrl = "http://127.0.0.1:5000/predict";


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
                var response = await _httpClient.GetAsync("http://127.0.0.1:5000/");
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

            var requestData = new
            {
                address = model.Address,
                type = model.Type,
                size = model.Size,
                beds = model.Beds,
                bath = model.Bath,
                beds_to_baths_ratio = model.Bath / (double)model.Beds
            };

            var jsonRequest = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("http://127.0.0.1:5000/predict", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                if (responseData != null && responseData.predicted_price != null)
                {
                    ViewBag.PredictedPrice = responseData.predicted_price;
                }
                else
                {
                    ViewBag.Error = "Invalid response from Flask API.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error connecting to Flask API: " + ex.Message;
            }

            return View("Index", model);
        }
    }
}
