using System.Diagnostics;
using System.IO;

namespace HomeHutBD.Services
{
    
      using System.Diagnostics;
      using Microsoft.Extensions.Hosting;
      using Microsoft.Extensions.Logging;

    public class FlaskServiceManager : IHostedService
        {
            private Process _flaskProcess;
            private readonly ILogger<FlaskServiceManager> _logger;
            private readonly string _pythonPath;
            private readonly string _flaskScriptPath;
            private readonly string _workingDirectory;

            public FlaskServiceManager(ILogger<FlaskServiceManager> logger)
            {
                    _logger = logger;

                string baseDirectory = Directory.GetCurrentDirectory();
                _logger.LogInformation($"Base Directory: {baseDirectory}");

                _workingDirectory = Path.Combine(baseDirectory, "flask_api");
                _flaskScriptPath = Path.Combine(_workingDirectory, "app.py");
                _pythonPath = Path.Combine(_workingDirectory, "venv", "Scripts", "python.exe");

                
                _logger.LogInformation($"Python Path: {_pythonPath}");
                _logger.LogInformation($"Flask Script Path: {_flaskScriptPath}");
                _logger.LogInformation($"Working Directory: {_workingDirectory}");

               
                if (!Directory.Exists(_workingDirectory))
                {
                    _logger.LogInformation($"Creating flask_api directory at: {_workingDirectory}");
                    Directory.CreateDirectory(_workingDirectory);
                }

               
                if (!File.Exists(_pythonPath))
                {
                    _logger.LogWarning($"Python executable not found at: {_pythonPath}");
                    _logger.LogInformation("Please create a virtual environment in the flask_api directory");
                }

                if (!File.Exists(_flaskScriptPath))
                {
                    _logger.LogWarning($"Flask script not found at: {_flaskScriptPath}");
                    _logger.LogInformation("Please ensure app.py is in the flask_api directory");
                }
            }

            public  Task StartAsync(CancellationToken cancellationToken)
            {
                    try
                    {
                        _logger.LogInformation("Starting Flask API service...");

                        

                        ProcessStartInfo startInfo = new ProcessStartInfo
                                {
                                    FileName = _pythonPath,
                                    Arguments = $"\"{_flaskScriptPath}\"",
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    CreateNoWindow = true,
                                    WorkingDirectory = _workingDirectory
                                };


                        _flaskProcess = new Process { StartInfo = startInfo };

                        _flaskProcess.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data != null)
                                _logger.LogInformation($"Flask: {e.Data}");
                        };

                        _flaskProcess.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data != null)
                                _logger.LogError($"Flask Error: {e.Data}");
                        };

                        _flaskProcess.Start();
                        _flaskProcess.BeginOutputReadLine();
                        _flaskProcess.BeginErrorReadLine();
                    
                        _logger.LogInformation("Flask API service started successfully");
                        return Task.CompletedTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to start Flask API service");
                        throw;
                    }
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                try
                {
                    if (_flaskProcess != null && !_flaskProcess.HasExited)
                    {
                        _logger.LogInformation("Stopping Flask API service...");
                        _flaskProcess.Kill(true);
                        _flaskProcess.WaitForExit();
                        _flaskProcess.Dispose();
                        _logger.LogInformation("Flask API service stopped successfully");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping Flask API service");
                }

                return Task.CompletedTask;
            }
        }

}
