# app.py
from math import ceil
from flask import Flask, request, jsonify
import joblib
import numpy as np
import os
from flask_cors import CORS
import pandas as pd

app = Flask(__name__)
CORS(app)  # Allow ASP.NET to send requests
  




model_path = os.path.join(os.path.dirname(__file__), 'best_BD_property_price_model.pkl')


model = joblib.load(model_path)

expected_features = ["address", "type", "size(sqft)", "beds", "bath", "beds_to_baths_ratio"]


@app.route('/health_check', methods=['GET'])
def health_check():
    return jsonify({'status': 'Flask API is running'}), 200

@app.route('/predict', methods=['POST'])
def predict():
    try:
        if not request.is_json:
            return jsonify({"error": "415 Unsupported Media Type: Request must be in JSON format."}), 415
        
        data = request.get_json()

        
        if "baths" in data:
            data["bath"] = data.pop("baths")

        if "size" in data:
            data["size(sqft)"] = data.pop("size")

        
        if "beds_to_baths_ratio" not in data:
            data["beds_to_baths_ratio"] = data["bath"] / data["beds"]

        
        required_features = ["address", "type", "size(sqft)", "beds", "bath", "beds_to_baths_ratio"]
        missing_features = [f for f in required_features if f not in data]

        if missing_features:
            return jsonify({
                "error": f"Missing required features: {', '.join(missing_features)}",
                "predicted_price": None,
                "status": "error"
            }), 400

        
        input_data = pd.DataFrame({
            "address": [data["address"]],
            "type": [data["type"]],
            "size(sqft)": [data["size(sqft)"]],
            "beds": [data["beds"]],
            "bath": [data["bath"]],
            "beds_to_baths_ratio": [data["beds_to_baths_ratio"]]
        })

       
        print("Input Data for Model Prediction:")
        print(input_data)

       
        predicted_price = model.predict(input_data)[0]

        return jsonify({"predicted_price": round(predicted_price, 2), "status": "success"})

    except Exception as e:
        return jsonify({"error": str(e), "predicted_price": None, "status": "error"}), 500


if __name__ == '__main__':
    app.run(debug=False, host='127.0.0.1', port=5000)