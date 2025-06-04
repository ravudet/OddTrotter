using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace UnitTestProject2
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            await new UnitTestProject1.UnitTest1().AwaitInterfaceWithDelay().ConfigureAwait(false);
        }
    }
}
