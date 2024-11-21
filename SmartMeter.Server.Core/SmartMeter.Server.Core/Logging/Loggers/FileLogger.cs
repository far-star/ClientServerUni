using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Logging.Loggers
{
    public class FileLogger : ILogListener
    {
        private Logger _logger;
        private readonly string _filePath;
        private static readonly object _lock = new object(); // Lock for threads
        public FileLogger(string filePath)
        {
            //Attach event viewer in the case that file cannot be found.
            _logger = new Logger();
            _filePath = filePath;
        }
        public void Update(string message, LogLevel level)
        {
            lock (_lock)
            {
                try
                {
                    string levelString = SetLevel(level);
                    using (StreamWriter writer = new StreamWriter(_filePath, true))
                    {
                        writer.WriteLine($"{levelString}:{DateTime.Now}: {message}");
                    }
                }
                catch (DirectoryNotFoundException ex)
                {
                    _logger.Error($"{ex.Message}");
                    _logger.Warn($"Directory not found. Creating log at the following location: {_filePath}");
                    if (!Directory.Exists(_filePath))
                    {
                        Directory.CreateDirectory(_filePath);
                        _logger.Success("Log file created!");
                    }
                }
                catch (IOException ex)
                {
                    _logger.Error($"I/O error writing to log file: {ex.Message}");
                }
            }
        }

        private string SetLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    return "[INFO]";
                case LogLevel.Warn:
                    return "[WARN]";
                case LogLevel.Error:
                    return "[ERROR]";
                case LogLevel.Success:
                    return "[SUCCESS]";
                default:
                    return "[INFO]";
            }
        }
    }
}
