using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Logging
{
    public interface ILogListener
    {
        void Update(string message, LogLevel level);
    }
}
