using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using System.Text;

namespace PentestHub.Launcher;

class Program
{
    private static HubConnection? _connection;
    private static string _apiUrl = "http://localhost:5000";
    private static string _toolsPath = "../tools-containerized";
    private static Process? _currentProcess;
    private static int _currentScanId = 0;
    private static StringBuilder _outputBuffer = new();

    static async Task Main(string[] args)
    {
        Console.WriteLine("PentestHub Launcher Starting...");

        // Parse command line arguments
        if (args.Length > 0)
        {
            _apiUrl = args[0];
        }
        if (args.Length > 1)
            _toolsPath = args[1];

        Console.WriteLine($"API URL: {_apiUrl}");
        Console.WriteLine($"Tools Path: {_toolsPath}");

        // Check if tools directory exists
        if (!Directory.Exists(_toolsPath))
        {
            Console.WriteLine($"Warning: Tools directory not found: {_toolsPath}");
            Console.WriteLine("Creating directory...");
            Directory.CreateDirectory(_toolsPath);
        }

        // Connect to SignalR hub
        await ConnectToHub();

        // Keep running
        Console.WriteLine("Launcher is running. Press Ctrl+C to exit.");
        Console.CancelKeyPress += async (sender, e) =>
        {
            e.Cancel = true;
            await Cleanup();
            Environment.Exit(0);
        };

        // Keep the application running
        await Task.Delay(-1);
    }

    static async Task ConnectToHub()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{_apiUrl}/hubs/launcher")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<object>("RunTool", async (command) =>
        {
            try
            {
                var cmd = System.Text.Json.JsonSerializer.Deserialize<RunToolCommand>(
                    command.ToString() ?? "{}"
                );

                if (cmd != null)
                {
                    await RunTool(cmd);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling RunTool command: {ex.Message}");
            }
        });

        _connection.Reconnecting += (error) =>
        {
            Console.WriteLine($"Reconnecting to hub... {error?.Message}");
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            Console.WriteLine($"Reconnected to hub. Connection ID: {connectionId}");
            return Task.CompletedTask;
        };

        _connection.Closed += async (error) =>
        {
            Console.WriteLine($"Connection closed. {error?.Message}");
            await Task.Delay(5000);
            await ConnectToHub();
        };

        try
        {
            await _connection.StartAsync();
            Console.WriteLine("Connected to PentestHub API");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect: {ex.Message}");
            Console.WriteLine("Retrying in 5 seconds...");
            await Task.Delay(5000);
            await ConnectToHub();
        }
    }

    static async Task RunTool(RunToolCommand command)
    {
        if (_currentProcess != null && !_currentProcess.HasExited)
        {
            Console.WriteLine("A tool is already running. Please wait for it to complete.");
            return;
        }

        _currentScanId = command.ScanId;
        _outputBuffer.Clear();

        var toolPath = Path.Combine(_toolsPath, command.ToolName);
        
        // Try different extensions
        var extensions = new[] { ".exe", ".bat", ".cmd", "" };
        string? executablePath = null;

        foreach (var ext in extensions)
        {
            var testPath = toolPath + ext;
            if (File.Exists(testPath))
            {
                executablePath = testPath;
                break;
            }
        }

        if (executablePath == null)
        {
            var errorMsg = $"Tool not found: {command.ToolName}";
            Console.WriteLine(errorMsg);
            await SendOutput(_currentScanId, $"ERROR: {errorMsg}\n");
            await NotifyScanComplete(_currentScanId, 0, _outputBuffer.ToString());
            return;
        }

        Console.WriteLine($"Running: {command.ToolName} on {command.Target}");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = $"{command.Target} {command.Arguments}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = _toolsPath
        };

        try
        {
            _currentProcess = Process.Start(processStartInfo);
            if (_currentProcess == null)
            {
                throw new Exception("Failed to start process");
            }

            // Read output asynchronously
            _currentProcess.OutputDataReceived += async (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    var output = e.Data + "\n";
                    _outputBuffer.Append(output);
                    Console.Write(output);
                    await SendOutput(_currentScanId, output);
                }
            };

            _currentProcess.ErrorDataReceived += async (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    var output = "ERROR: " + e.Data + "\n";
                    _outputBuffer.Append(output);
                    Console.Write(output);
                    await SendOutput(_currentScanId, output);
                }
            };

            _currentProcess.BeginOutputReadLine();
            _currentProcess.BeginErrorReadLine();

            await _currentProcess.WaitForExitAsync();

            var exitCode = _currentProcess.ExitCode;
            Console.WriteLine($"Process completed with exit code: {exitCode}");

            // Get user ID from environment or default to 0 (should be passed from backend)
            var userId = 0; // In production, this should be passed from the command

            await NotifyScanComplete(_currentScanId, userId, _outputBuffer.ToString());
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error running tool: {ex.Message}\n";
            _outputBuffer.Append(errorMsg);
            Console.WriteLine(errorMsg);
            await SendOutput(_currentScanId, errorMsg);
            await NotifyScanComplete(_currentScanId, 0, _outputBuffer.ToString());
        }
        finally
        {
            _currentProcess?.Dispose();
            _currentProcess = null;
        }
    }

    static async Task SendOutput(int scanId, string output)
    {
        if (_connection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _connection.InvokeAsync("SendOutput", scanId, output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending output: {ex.Message}");
            }
        }
    }

    static async Task NotifyScanComplete(int scanId, int userId, string fullOutput)
    {
        if (_connection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _connection.InvokeAsync("ScanComplete", scanId, userId, fullOutput);
                Console.WriteLine($"Scan {scanId} completed notification sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying scan complete: {ex.Message}");
            }
        }
    }

    static async Task Cleanup()
    {
        if (_currentProcess != null && !_currentProcess.HasExited)
        {
            try
            {
                _currentProcess.Kill();
                await _currentProcess.WaitForExitAsync();
            }
            catch { }
        }

        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
        }

        Console.WriteLine("Launcher stopped.");
    }
}

public class RunToolCommand
{
    public int ScanId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

