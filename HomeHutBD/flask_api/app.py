# app.py
from flask import Flask, request, jsonify
import joblib
import numpy as np
from flask_cors import CORS
import os

app = Flask(__name__)
CORS(app)  # Enable CORS for all routes



# Get the absolute path to the model file
model_path = os.path.join(os.path.dirname(__file__), 'best_BD_property_price_model.pkl')

# Load the model
model = joblib.load(model_path)

@app.route('/', methods=['GET'])
def health_check():
    return jsonify({'status': 'Flask API is running'}), 200

@app.route('/predict', methods=['POST'])
def predict():
    try:
        data = request.get_json()
        
        # Extract features from request
        beds = float(data['beds'])
        baths = float(data['bath'])
        size = float(data['size(sqft)'])
        address = data['address']
        
        # Calculate beds to bath ratio
        beds_to_baths_ratio = baths / beds if beds > 0 else 0
        
        # Create feature array
        # Note: Adjust this according to how your model was trained
        features = np.array([[beds, baths, beds_to_baths_ratio, size]])
        
        # Make prediction
        prediction = model.predict(features)
        
        return jsonify({
            'status': 'success',
            'predicted_price': float(prediction[0]),
            "message": "Prediction endpoint is working!",
            'error': None
        })
        
    except Exception as e:
        return jsonify({
            'status': 'error',
            'predicted_price': None,
            'error': str(e)
        })

if __name__ == '__main__':
    app.run(debug=False, host='127.0.0.1', port=5000)