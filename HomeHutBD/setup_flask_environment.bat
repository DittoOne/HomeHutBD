@echo off
echo Setting up Flask API environment...

cd %~dp0
cd flask_api

REM Check if venv directory exists
if not exist venv (
    echo Creating virtual environment...
    python -m venv venv
    if %ERRORLEVEL% neq 0 (
        echo Error creating virtual environment.
        echo Please make sure Python is installed and available in PATH.
        pause
        exit /b 1
    )
) else (
    echo Virtual environment already exists.
)

REM Activate the virtual environment and install packages
echo Activating virtual environment and installing required packages...
call venv\Scripts\activate

echo Installing required packages...
pip install flask numpy pandas joblib flask-cors

if %ERRORLEVEL% neq 0 (
    echo Error installing packages.
    pause
    exit /b 1
)

echo Checking for model file...
if not exist best_BD_property_price_model.pkl (
    echo WARNING: Model file 'best_BD_property_price_model.pkl' not found in flask_api directory.
    echo Please make sure to copy the model file to this location.
)

echo Setup completed successfully!
echo You can now run the application.
pause