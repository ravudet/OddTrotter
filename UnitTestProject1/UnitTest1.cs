using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    public class UnitTest1
    {
        private sealed class AwaiterType<T>
        {
            private readonly T value;

            public AwaiterType(T value)
            {
                this.value = value;
            }

            public async ITask<T> GetValueWithDelay()
            {
                await Task.Delay(100).ConfigureAwait(false);
                return this.value;
            }
        }

        public async Task AwaitInterfaceWithDelay()
        {
            var value = "asdf";
            var result = await new AwaiterType<string>(value).GetValueWithDelay().ConfigureAwait(false);

            Assert.AreEqual(value, result);
        }
    }
}
