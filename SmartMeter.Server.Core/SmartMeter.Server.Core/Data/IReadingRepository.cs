using SmartMeter.Server.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Data
{
    public interface IReadingRepository
    {
        Reading GetReadingById(int id);
        void AddReading(Reading reading);
    }
}
