using System.Net;
using System.Net.Sockets;
using MeaMod.Utilities.NetworkTools.PortScan;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class PortScanTests
    {
        [TestMethod]
        public void TcpScanFindsAListeningPortAndCanRestartAfterStop()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                int port = ((IPEndPoint)listener.LocalEndpoint).Port;
                var manager = new PortScanManager("127.0.0.1");
                var found = new List<int>();
                manager.PortScanResult += (_, e) => { lock (found) found.Add(e.Port); };

                manager.Start(port, port, PortTypes.Tcp);
                Task.WaitAll([.. manager.Tasks], TimeSpan.FromSeconds(10));
                Assert.AreSequenceEqual([port], found);

                // The old manager never replaced its cancelled token source, so a scan after Stop() did nothing.
                manager.Stop();
                manager.Start(port, port, PortTypes.Tcp);
                Task.WaitAll([.. manager.Tasks], TimeSpan.FromSeconds(10));
                Assert.AreSequenceEqual([port, port], found);
            }
            finally
            {
                listener.Stop();
            }
        }

        [TestMethod]
        public void TcpScanDoesNotReportAClosedPort()
        {
            int port = FreePort(ProtocolType.Tcp);
            var scanner = new PortScanner("127.0.0.1", port);
            bool reported = false;
            scanner.PortScanResult += (_, _) => reported = true;

            scanner.AttemptTcpConnectionToPort();

            Assert.IsFalse(reported);
        }

        [TestMethod]
        public void UdpScanDoesNotReportAClosedPort()
        {
            // UdpClient.Connect() never fails, so the old scanner reported every UDP port as open.
            int port = FreePort(ProtocolType.Udp);
            var scanner = new PortScanner("127.0.0.1", port);
            bool reported = false;
            scanner.PortScanResult += (_, _) => reported = true;

            scanner.AttemptUdpConnectionToPort();

            Assert.IsFalse(reported, "a closed UDP port was reported as open");
        }

        [TestMethod]
        public void UdpScanReportsAListeningPort()
        {
            using var listener = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            int port = ((IPEndPoint)listener.Client.LocalEndPoint!).Port;
            var scanner = new PortScanner("127.0.0.1", port);
            bool reported = false;
            scanner.PortScanResult += (_, _) => reported = true;

            scanner.AttemptUdpConnectionToPort();

            Assert.IsTrue(reported, "a listening UDP port was not reported");
        }

        [TestMethod]
        public void StartRejectsAnInvertedRange()
        {
            var manager = new PortScanManager("127.0.0.1");

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => manager.Start(100, 99, PortTypes.Tcp));
        }

        private static int FreePort(ProtocolType protocol)
        {
            using var socket = new Socket(AddressFamily.InterNetwork, protocol == ProtocolType.Tcp ? SocketType.Stream : SocketType.Dgram, protocol);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            return ((IPEndPoint)socket.LocalEndPoint!).Port;
        }
    }
}
