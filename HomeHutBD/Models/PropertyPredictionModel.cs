using System.ComponentModel.DataAnnotations;

namespace HomeHutBD.Models
{
    public class PropertyPredictionModel
    {
        [Required]
        public string Address { get; set; }

        [Required]
        public string Type { get; set; }  // ✅ Ensure this exists

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of beds must be at least 1.")]
        public int Beds { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of baths must be at least 1.")]
        public int Bath { get; set; }  // ✅ Ensure this exists

        [Required]
        [Range(100, int.MaxValue, ErrorMessage = "Size must be at least 100 sqft.")]
        public int Size { get; set; }
    }

    public class PredictionResponse
    {
        public string Status { get; set; }
        public double? PredictedPrice { get; set; }
        public string Error { get; set; }
    }

}
