using System.Net;
using System.Net.Sockets;
using System.Text;
using MeaMod.Utilities.NetworkTools.Whois;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class WhoisManagerMoreTests
    {
        private readonly WhoisManager _manager = new();
        private static readonly string[] expected = ["whois.example.net"];

        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public void ExecuteWhoisForDomainValidatesArguments()
        {
            Assert.ThrowsExactly<ArgumentException>(() => _manager.ExecuteWhoisForDomain(" "));
            Assert.ThrowsExactly<ArgumentException>(() => _manager.ExecuteWhoisForDomain(" ", "whois.example"));
            Assert.ThrowsExactly<ArgumentException>(() => _manager.ExecuteWhoisForDomain("example.com", " "));
        }

        [TestMethod]
        [Timeout(60_000, CooperativeCancellation = true)]
        public void UnreachableWhoisServerReturnsAnEmptyString()
        {
            // ".invalid" never resolves (RFC 6761), so both overloads fail to connect and return empty.
            Assert.AreEqual(string.Empty, _manager.ExecuteWhoisForDomain("example.com", "whois.nonexistent.invalid"));
            Assert.AreEqual(string.Empty, _manager.ExecuteWhoisForDomain("example.invalid"));
        }

        [TestMethod]
        [Timeout(60_000, CooperativeCancellation = true)]
        public void ResponseFromAWhoisServerIsReturnedWithNormalisedLineEndings()
        {
            var listener = new TcpListener(IPAddress.Loopback, 43);
            SocketError? listenFailure = null;
            try
            {
                listener.Start();
            }
            catch (SocketException e)
            {
                listenFailure = e.SocketErrorCode;
            }

            if (listenFailure != null)
                Assert.Inconclusive("cannot listen on loopback port 43: " + listenFailure);

            string? received = null;
            Task server = Task.Run(async () =>
            {
                using TcpClient client = await listener.AcceptTcpClientAsync(TestContext.CancellationToken);
                using NetworkStream stream = client.GetStream();
                var buffer = new byte[256];
                int count = await stream.ReadAsync(buffer, TestContext.CancellationToken);
                received = Encoding.ASCII.GetString(buffer, 0, count);
                byte[] response = Encoding.ASCII.GetBytes("Domain Name: EXAMPLE.COM\r\nWhois Server: whois.example.net\r\n");
                await stream.WriteAsync(response, TestContext.CancellationToken);
            }, TestContext.CancellationToken);

            try
            {
                string output = _manager.ExecuteWhoisForDomain("example.com", "127.0.0.1");
                Assert.IsTrue(server.Wait(10_000, TestContext.CancellationToken), "the fake server did not finish");

                Assert.AreEqual("example.com\r\n", received);
                Assert.AreEqual("Domain Name: EXAMPLE.COM" + Environment.NewLine + "Whois Server: whois.example.net" + Environment.NewLine, output);
                Assert.AreSequenceEqual(expected, _manager.FindWhoisServerInOutput(output).ToList());
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
