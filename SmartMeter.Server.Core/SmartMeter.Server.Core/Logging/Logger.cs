using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Logging
{
    public class Logger
    {
        private List<ILogListener> listeners = new List<ILogListener>();

        public void Attach(ILogListener listener)
        {
            listeners.Add(listener);
        }

        public void Detach(ILogListener listener)
        {
            listeners.Remove(listener);
        }

        public void Info(string message) => Log(message, LogLevel.Info);
        public void Warn(string message) => Log(message, LogLevel.Warn);
        public void Error(string message) => Log(message, LogLevel.Error);
        public void Success(string message) => Log(message, LogLevel.Success);

        public void Log(string message, LogLevel level)
        {
            NotifyListeners(message, level);
        }

        private void NotifyListeners(string message, LogLevel level)
        {
            foreach (var listener in listeners)
            {
                listener.Update(message, level);
            }
        }
    }
}
