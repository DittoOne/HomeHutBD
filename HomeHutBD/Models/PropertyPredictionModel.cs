using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HomeHutBD.Models
{
    public class PropertyPredictionModel
    {
        [Required]
        public string address { get; set; }

        [Required]
        public string type { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of beds must be at least 1.")]
        public int beds { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of baths must be at least 1.")]
        public int bath { get; set; }

        [Required]
        [Range(100, int.MaxValue, ErrorMessage = "Size must be at least 100 sqft.")]
        public int size { get; set; }
    }

    public class PredictionResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("predicted_price")]
        public double? PredictedPrice { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}