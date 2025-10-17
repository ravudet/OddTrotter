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

        Task<T> RegisterAsync<T>(Func<Task<T>> factory) where T : IDisposable;

        void Unregister<T>(T disposable) where T : IDisposable; //// TODO make sure to document (and implement) that this needs to dispose the item as well
    }
}
