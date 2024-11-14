using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Logging.Loggers
{
    public class ConsoleLogger : ILogListener
    {
        public void Update(string message, LogLevel level)
        {
            // Save the original colors
            ConsoleColor originalForegroundColor = Console.ForegroundColor;
            ConsoleColor originalBackgroundColor = Console.BackgroundColor;

            // Set background color based on the log level for the level name only
            switch (level)
            {
                case LogLevel.Info:
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.White; // Ensure text is visible
                    Console.Write("[INFO] ");
                    break;
                case LogLevel.Warn:
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black; // Ensure text is visible
                    Console.Write("[WARN] ");
                    break;
                case LogLevel.Error:
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White; // Ensure text is visible
                    Console.Write("[ERROR] ");
                    break;
                case LogLevel.Success:
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.ForegroundColor = ConsoleColor.White; // Ensure text is visible
                    Console.Write("[SUCCESS] ");
                    break;
            }

            // Reset colors to original after printing the level
            Console.ForegroundColor = originalForegroundColor;
            Console.BackgroundColor = originalBackgroundColor;

            // Print the message in default color
            Console.WriteLine(message);
        }
    }
}
