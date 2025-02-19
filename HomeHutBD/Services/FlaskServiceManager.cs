using System.Diagnostics;

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

            public FlaskServiceManager(ILogger<FlaskServiceManager> logger)
            {
                _logger = logger;
                
                _pythonPath = @"C:\Users\User\AppData\Local\Programs\Python\Python312\python.exe";  
                _flaskScriptPath = @"J:\HomeHutBD\HomeHutBD\flask_api\app.py";
            
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
                        WorkingDirectory = @"J:\HomeHutBD\HomeHutBD\flask_api" 
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
