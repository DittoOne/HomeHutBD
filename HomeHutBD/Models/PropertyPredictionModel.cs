namespace HomeHutBD.Models
{
    public class PropertyPredictionModel
    {
        public int Beds { get; set; }
        public int Baths { get; set; }
        public double Size { get; set; }
        public string Address { get; set; }
    }

    public class PredictionResponse
    {
        public string Status { get; set; }
        public double? PredictedPrice { get; set; }
        public string Error { get; set; }
    }

}
