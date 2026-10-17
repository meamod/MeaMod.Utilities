using MeaMod.Utilities.NetworkTools.TraceRoute;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class TraceRouteTests
    {
        [TestMethod]
        public void HopDetailToStringUsesTotalMilliseconds()
        {
            // The old code printed the Milliseconds component, so 1.5 s showed as "500ms".
            var detail = new TraceRouteHopDetail(1, "10.0.0.1", "router", TimeSpan.FromSeconds(1.5));

            StringAssert.Contains(detail.ToString(), 1500d.ToString("N0") + "ms");
            StringAssert.Contains(detail.ToString(), "router");
        }

        [TestMethod]
        public void HopDetailToStringShowsAStarForNoResponse()
        {
            var detail = new TraceRouteHopDetail(2, "*", "***Request Timed Out***", TimeSpan.MaxValue);

            string text = detail.ToString();
            StringAssert.Contains(text, " * ");
            Assert.IsFalse(text.Contains("ms"), text);
        }

        [TestMethod]
        public async Task LoopbackTraceRouteReturnsTheDestinationAsTheOnlyHop()
        {
            // The old code dropped the destination whenever the host was given as an IP address.
            var manager = new TraceRouteManager();
            var found = new List<TraceRouteHopDetail>();
            manager.TraceRouteNodeFound += (_, e) => found.Add(e.Detail);

            var hops = (await manager.ExecuteTraceRoute("127.0.0.1")).ToList();

            Assert.AreEqual(1, hops.Count, string.Join(Environment.NewLine, hops));
            Assert.AreEqual("127.0.0.1", hops[0].IPAddress);
            Assert.AreEqual(1, hops[0].HopNumber);
            Assert.AreNotEqual(TimeSpan.MaxValue, hops[0].ResponseTime);
            Assert.AreEqual(1, found.Count);
        }

        [TestMethod]
        public async Task TraceRouteCanRunAgainAfterCancel()
        {
            var manager = new TraceRouteManager();
            manager.Cancel();

            var hops = (await manager.ExecuteTraceRoute("127.0.0.1")).ToList();

            Assert.AreEqual(1, hops.Count);
        }

        [TestMethod]
        public void GetResponseTimeToLoopbackIsNotMaxValue()
        {
            Assert.AreNotEqual(TimeSpan.MaxValue, new TraceRouteManager().GetResponseTime("127.0.0.1"));
        }
    }
}
