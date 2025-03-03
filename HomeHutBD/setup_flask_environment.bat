@echo off

:: Use relative paths based on the location of this batch file
set BASE_DIR=%~dp0
cd /d %BASE_DIR%\flask_api
echo Current directory: %CD%

:: Check if Python is installed
python --version > nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [INFO] Python is not installed or not in PATH.
    echo [INFO] Attempting to download and install Python...
    
    :: Create a temp directory for the installer
    mkdir temp 2>nul
    cd temp
    
    :: Download Python installer
    echo [INFO] Downloading Python installer...
    curl -L -o python_installer.exe https://www.python.org/ftp/python/3.10.11/python-3.10.11-amd64.exe
    
    if %ERRORLEVEL% NEQ 0 (
        echo [ERROR] Failed to download Python installer.
        echo Please install Python 3.8 or newer manually from https://www.python.org/downloads/
        echo Make sure to check "Add Python to PATH" during installation.
        pause
        start https://www.python.org/downloads/
        cd ..
        exit /b 1
    )
    
    :: Run the installer with parameters to add to PATH and install pip
    echo [INFO] Running Python installer...
    echo [INFO] Please follow the installation instructions.
    echo [INFO] *** IMPORTANT: CHECK "Add Python to PATH" OPTION ***
    
    start /wait python_installer.exe /quiet InstallAllUsers=0 PrependPath=1 Include_test=0
    
    :: Clean up
    cd ..
    rmdir /s /q temp
    
    :: Verify Python installation
    python --version > nul 2>&1
    if %ERRORLEVEL% NEQ 0 (
        echo [ERROR] Python installation failed or PATH not updated.
        echo Please restart your computer and try again, or install Python manually.
        pause
        exit /b 1
    )
    
    echo [SUCCESS] Python has been installed!
)

:: Python is installed, show version
python --version
echo.

:: Check if venv exists, if not create it
if not exist venv (
    echo Creating virtual environment...
    python -m venv venv
    if %ERRORLEVEL% NEQ 0 (
        echo [ERROR] Failed to create virtual environment.
        echo Please ensure you have the latest Python version with venv support.
        pause
        exit /b 1
    )
)

:: Activate the virtual environment
echo Activating virtual environment...
call venv\Scripts\activate

:: Install required packages
echo Installing required packages...
pip install flask flask-cors numpy pandas joblib scikit-learn
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to install required packages.
    echo Please check your internet connection and try again.
    pause
    exit /b 1
)

:: Check if model file exists
if not exist best_BD_property_price_model.pkl (
    echo [WARNING] Machine learning model file not found at:
    echo %CD%\best_BD_property_price_model.pkl
    echo The API may not function correctly without this file.
    echo.
    pause
)

:: Start the Flask API on port 5000
echo Starting Flask API on port 5000...
echo Press Ctrl+C to stop the server

python app.py

pause