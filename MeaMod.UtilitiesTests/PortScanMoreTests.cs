using System.Net;
using System.Net.Sockets;
using MeaMod.Utilities.NetworkTools.PortScan;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class PortScanMoreTests
    {
        [TestMethod]
        public void ManagerConstructorRejectsAnEmptyEndpoint()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new PortScanManager(" "));
        }

        [TestMethod]
        public void ScannerConstructorValidatesArguments()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new PortScanner(" ", 80));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PortScanner("127.0.0.1", 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PortScanner("127.0.0.1", 65536));

            var scanner = new PortScanner("127.0.0.1", 80);
            Assert.AreEqual("127.0.0.1", scanner.EndPointName);
            Assert.AreEqual(80, scanner.Port);
        }

        [TestMethod]
        public void StartRejectsPortsOutsideTheValidRange()
        {
            var manager = new PortScanManager("127.0.0.1");

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => manager.Start(0, 10, PortTypes.Tcp));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => manager.Start(1, 65536, PortTypes.Tcp));
        }

        [TestMethod]
        public void UnknownPortTypeCreatesNoTasks()
        {
            var manager = new PortScanManager("127.0.0.1");

            manager.Start(1, 3, PortTypes.Unknown);

            Assert.AreEqual(0, manager.Tasks.Count());
            Assert.AreEqual("127.0.0.1", manager.EndPoint);
        }

        [TestMethod]
        public void UdpScanThroughTheManagerReportsAListeningPort()
        {
            using var listener = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            int port = ((IPEndPoint)listener.Client.LocalEndPoint!).Port;
            var manager = new PortScanManager("127.0.0.1");
            var found = new List<PortScanResultEventArgs>();
            manager.PortScanResult += (_, e) => { lock (found) found.Add(e); };

            manager.Start(port, port, PortTypes.Udp);
            Task.WaitAll(manager.Tasks.ToArray(), TimeSpan.FromSeconds(10));

            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(port, found[0].Port);
            Assert.AreEqual(PortTypes.Udp, found[0].PortType);
            Assert.AreEqual("127.0.0.1", found[0].EndPoint);
        }

        [TestMethod]
        public void UdpScanReportsAPortThatAnswers()
        {
            using var listener = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            int port = ((IPEndPoint)listener.Client.LocalEndPoint!).Port;
            var echo = Task.Run(() =>
            {
                IPEndPoint? remote = null;
                byte[] data = listener.Receive(ref remote);
                listener.Send(data, data.Length, remote);
            });
            var scanner = new PortScanner("127.0.0.1", port);
            bool reported = false;
            scanner.PortScanResult += (_, _) => reported = true;

            scanner.AttemptUdpConnectionToPort();

            Assert.IsTrue(reported, "a UDP port that answered was not reported");
            Assert.IsTrue(echo.Wait(TimeSpan.FromSeconds(5)), "the echo listener never received the probe");
        }

        [TestMethod]
        public void UnresolvableHostIsNotReported()
        {
            var tcp = new PortScanner("host.invalid", 80);
            var udp = new PortScanner("host.invalid", 80);
            bool reported = false;
            tcp.PortScanResult += (_, _) => reported = true;
            udp.PortScanResult += (_, _) => reported = true;

            tcp.AttemptTcpConnectionToPort();
            udp.AttemptUdpConnectionToPort();

            Assert.IsFalse(reported);
        }

        [TestMethod]
        [Timeout(240_000)]
        public void FullRangeScanCanBeStopped()
        {
            // Start() with no arguments scans every TCP port on the loopback address; Stop() cancels the queued tasks.
            var manager = new PortScanManager("127.0.0.1");

            manager.Start();
            manager.Stop();

            Task[] tasks = manager.Tasks.ToArray();
            Assert.AreEqual(ushort.MaxValue, tasks.Length);
            Assert.IsTrue(Task.WhenAll(tasks).ContinueWith(_ => { }, TaskScheduler.Default).Wait(TimeSpan.FromSeconds(200)), "scan tasks did not finish");
            Assert.IsTrue(tasks.All(t => t.IsCompleted));
        }

        [TestMethod]
        public void ResultEventArgsDefaultsAndToString()
        {
            var empty = new PortScanResultEventArgs();
            Assert.AreEqual(0, empty.Port);
            Assert.IsNull(empty.EndPoint);
            Assert.AreEqual(PortTypes.Unknown, empty.PortType);

            var args = new PortScanResultEventArgs("example.test", 80, PortTypes.Tcp);
            Assert.AreEqual("Tcp port 80 found on host example.test.", args.ToString());
        }

        [TestMethod]
        [Timeout(240_000)]
        public void StopFromAResultHandlerAbandonsTheRemainingPorts()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                int port = ((IPEndPoint)listener.LocalEndpoint).Port;
                var manager = new PortScanManager("127.0.0.1");
                bool found = false;
                manager.PortScanResult += (_, e) =>
                {
                    if (e.Port != port)
                        return;
                    found = true;
                    // Cancelling while Start() is still queueing ports makes it abandon the rest of the range.
                    manager.Stop();
                };

                manager.Start(port, ushort.MaxValue, PortTypes.Tcp);
                Task[] tasks = manager.Tasks.ToArray();
                Assert.IsTrue(Task.WhenAll(tasks).ContinueWith(_ => { }, TaskScheduler.Default).Wait(TimeSpan.FromSeconds(200)), "scan tasks did not finish");

                Assert.IsTrue(found, "the listening port was not reported");
                Assert.IsTrue(manager.CurrentCancellationTokenSource.IsCancellationRequested);
                Assert.IsTrue(tasks.Length <= ushort.MaxValue - port + 1);
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
