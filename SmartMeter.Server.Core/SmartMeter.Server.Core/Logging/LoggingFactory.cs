using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Logging
{
    public class LoggingFactory
    {
        private readonly IServiceProvider _provider;

        public LoggingFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public Logger CreateLogger()
        {
            var logger = new Logger();
            var listeners = _provider.GetServices<ILogListener>();

            // Attach all listeners after everything is initialized
            foreach (var listener in listeners)
            {
                logger.Attach(listener);
            }

            return logger;
        }
    }
}
