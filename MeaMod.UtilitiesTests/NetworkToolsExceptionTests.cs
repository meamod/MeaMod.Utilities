using MeaMod.Utilities.NetworkTools;
using MeaMod.Utilities.NetworkTools.IcmpPing;
using MeaMod.Utilities.NetworkTools.PortScan;
using MeaMod.Utilities.NetworkTools.TraceRoute;
using MeaMod.Utilities.NetworkTools.Whois;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class NetworkToolsExceptionTests
    {
        private static void AssertConstructors<T>(Func<T> plain, Func<string, T> withMessage, Func<string, Exception, T> withInner)
            where T : NetworkToolsException
        {
            var inner = new InvalidOperationException("inner");

            Assert.IsNotNull(plain());
            Assert.AreEqual("message", withMessage("message").Message);

            T withInnerException = withInner("outer", inner);
            Assert.AreEqual("outer", withInnerException.Message);
            Assert.AreSame(inner, withInnerException.InnerException);
        }

        [TestMethod]
        public void NetworkToolsException()
        {
            AssertConstructors(() => new NetworkToolsException(), m => new NetworkToolsException(m), (m, i) => new NetworkToolsException(m, i));
        }

        [TestMethod]
        public void IcmpPingException()
        {
            AssertConstructors(() => new IcmpPingException(), m => new IcmpPingException(m), (m, i) => new IcmpPingException(m, i));
        }

        [TestMethod]
        public void PortScanException()
        {
            AssertConstructors(() => new PortScanException(), m => new PortScanException(m), (m, i) => new PortScanException(m, i));
        }

        [TestMethod]
        public void TraceRouteExceptionException()
        {
            AssertConstructors(() => new TraceRouteExceptionException(), m => new TraceRouteExceptionException(m), (m, i) => new TraceRouteExceptionException(m, i));
        }

        [TestMethod]
        public void WhoisException()
        {
            AssertConstructors(() => new WhoisException(), m => new WhoisException(m), (m, i) => new WhoisException(m, i));
        }
    }
}
