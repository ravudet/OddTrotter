using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OddTrotter.CalendarV1.Tokenization
{
    public interface IDispositionManager : IAsyncDisposable
    {
        T Register<T>(Func<T> factory) where T : IDisposable;
    }
}
