using System.Net.NetworkInformation;
using MeaMod.Utilities.NetworkTools.TraceRoute;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class TraceRouteMoreTests
    {
        [TestMethod]
        public void ExecuteTraceRouteRejectsAnEmptyHost()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new TraceRouteManager().ExecuteTraceRoute(" "));
        }

        [TestMethod]
        public void GetResponseTimeRejectsAnEmptyHost()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new TraceRouteManager().GetResponseTime(" "));
        }

        [TestMethod]
        public void GetResponseTimeForAnUnresolvableHostIsMaxValue()
        {
            Assert.AreEqual(TimeSpan.MaxValue, new TraceRouteManager().GetResponseTime("host.invalid"));
        }

        [TestMethod]
        [Timeout(30_000)]
        public void GetResponseTimeForAnUnreachableAddressIsMaxValue()
        {
            // The old code returned 0ms for a timed-out reply.
            Assert.AreEqual(TimeSpan.MaxValue, new TraceRouteManager().GetResponseTime("192.0.2.1"));
        }

        [TestMethod]
        public async Task TraceRouteCompleteEventCarriesTheHops()
        {
            var manager = new TraceRouteManager();
            TraceRouteCompleteEventArgs? completed = null;
            manager.TraceRouteComplete += (_, e) => completed = e;

            var hops = (await manager.ExecuteTraceRoute("127.0.0.1")).ToList();

            Assert.IsNotNull(completed);
            Assert.IsNotNull(completed.TraceRouteHops);
            CollectionAssert.AreEqual(hops, completed.TraceRouteHops);
        }

        [TestMethod]
        [Timeout(90_000)]
        public async Task CancelDuringATraceStopsAfterTheCurrentHop()
        {
            var manager = new TraceRouteManager();
            manager.TraceRouteNodeFound += (_, _) => manager.Cancel();

            var hops = (await manager.ExecuteTraceRoute("192.0.2.1")).ToList();

            Assert.AreEqual(1, hops.Count, string.Join(Environment.NewLine, hops));
            Assert.AreEqual(1, hops[0].HopNumber);
        }

        [TestMethod]
        [Timeout(300_000)]
        public async Task MultiHopTraceRecordsIntermediateHops()
        {
            using (var ping = new Ping())
            {
                PingReply probe;
                try
                {
                    probe = ping.Send("1.1.1.1", 2000);
                }
                catch (PingException)
                {
                    Assert.Inconclusive("no network available");
                    return;
                }

                if (probe.Status != IPStatus.Success)
                    Assert.Inconclusive("1.1.1.1 is not reachable: " + probe.Status);
            }

            var manager = new TraceRouteManager();
            var hops = (await manager.ExecuteTraceRoute("1.1.1.1")).ToList();

            Assert.IsTrue(hops.Count >= 2, "expected at least one intermediate hop");
            Assert.AreEqual("1.1.1.1", hops[hops.Count - 1].IPAddress);
            for (int i = 0; i < hops.Count; i++)
                Assert.AreEqual(i + 1, hops[i].HopNumber);
            Assert.IsTrue(hops.Any(h => h.ResponseTime != TimeSpan.MaxValue));
        }

        [TestMethod]
        public void HopDetailDefaultsSettersAndHeader()
        {
            var detail = new TraceRouteHopDetail
            {
                HopNumber = 3,
                IPAddress = "10.0.0.3",
                HostName = "three",
                ResponseTime = TimeSpan.FromMilliseconds(12),
            };

            Assert.AreEqual(3, detail.HopNumber);
            Assert.AreEqual("10.0.0.3", detail.IPAddress);
            Assert.AreEqual("three", detail.HostName);
            Assert.AreEqual(TimeSpan.FromMilliseconds(12), detail.ResponseTime);
            StringAssert.Contains(TraceRouteHopDetail.FormattedTextHeader, "Hop IP Address");
            StringAssert.Contains(detail.ToString(), "12ms");
        }

        [TestMethod]
        public void EventArgsConstructors()
        {
            Assert.IsNull(new TraceRouteNodeFoundEventArgs().Detail);
            Assert.IsNull(new TraceRouteCompleteEventArgs().TraceRouteHops);

            var detail = new TraceRouteHopDetail(1, "10.0.0.1", "one", TimeSpan.Zero);
            Assert.AreSame(detail, new TraceRouteNodeFoundEventArgs(detail).Detail);

            var hops = new List<TraceRouteHopDetail> { detail };
            Assert.AreSame(hops, new TraceRouteCompleteEventArgs(hops).TraceRouteHops);
        }

        [TestMethod]
        [Timeout(120_000)]
        public async Task TraceToAnUnroutableAddressTerminatesWithinMaxHops()
        {
            // Every probe beyond the local gateway either times out or is reported unreachable, so this exercises
            // the silent-hop and unreachable branches and the 30 hop limit. A short probe timeout keeps it quick.
            var manager = new TraceRouteManager { ProbeTimeoutMilliseconds = 300 };
            Assert.AreEqual(300, manager.ProbeTimeoutMilliseconds);

            var hops = (await manager.ExecuteTraceRoute("192.0.2.1")).ToList();

            Assert.IsTrue(hops.Count >= 1 && hops.Count <= 30, hops.Count.ToString());
            for (int i = 0; i < hops.Count; i++)
                Assert.AreEqual(i + 1, hops[i].HopNumber);
            foreach (TraceRouteHopDetail silent in hops.Where(h => h.IPAddress == "*"))
                Assert.AreEqual(TimeSpan.MaxValue, silent.ResponseTime);
            Assert.AreNotEqual("192.0.2.1", hops[hops.Count - 1].IPAddress, "the destination must not appear as answered");
        }
    }
}
