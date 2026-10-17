using System.Net;
using System.Net.Sockets;
using System.Text;
using MeaMod.Utilities.NetworkTools.Whois;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class WhoisManagerMoreTests
    {
        private readonly WhoisManager _manager = new();

        [TestMethod]
        public void ExecuteWhoisForDomainValidatesArguments()
        {
            Assert.ThrowsExactly<ArgumentException>(() => _manager.ExecuteWhoisForDomain(" "));
            Assert.ThrowsExactly<ArgumentException>(() => _manager.ExecuteWhoisForDomain(" ", "whois.example"));
            Assert.ThrowsExactly<ArgumentException>(() => _manager.ExecuteWhoisForDomain("example.com", " "));
        }

        [TestMethod]
        [Timeout(60_000)]
        public void UnreachableWhoisServerReturnsAnEmptyString()
        {
            // ".invalid" never resolves (RFC 6761), so both overloads fail to connect and return empty.
            Assert.AreEqual(string.Empty, _manager.ExecuteWhoisForDomain("example.com", "whois.nonexistent.invalid"));
            Assert.AreEqual(string.Empty, _manager.ExecuteWhoisForDomain("example.invalid"));
        }

        [TestMethod]
        [Timeout(60_000)]
        public void ResponseFromAWhoisServerIsReturnedWithNormalisedLineEndings()
        {
            var listener = new TcpListener(IPAddress.Loopback, 43);
            try
            {
                listener.Start();
            }
            catch (SocketException e)
            {
                Assert.Inconclusive("cannot listen on loopback port 43: " + e.SocketErrorCode);
                return;
            }

            string? received = null;
            Task server = Task.Run(async () =>
            {
                using TcpClient client = await listener.AcceptTcpClientAsync();
                using NetworkStream stream = client.GetStream();
                var buffer = new byte[256];
                int count = await stream.ReadAsync(buffer, 0, buffer.Length);
                received = Encoding.ASCII.GetString(buffer, 0, count);
                byte[] response = Encoding.ASCII.GetBytes("Domain Name: EXAMPLE.COM\r\nWhois Server: whois.example.net\r\n");
                await stream.WriteAsync(response, 0, response.Length);
            });

            try
            {
                string output = _manager.ExecuteWhoisForDomain("example.com", "127.0.0.1");
                Assert.IsTrue(server.Wait(TimeSpan.FromSeconds(10)), "the fake server did not finish");

                Assert.AreEqual("example.com\r\n", received);
                Assert.AreEqual("Domain Name: EXAMPLE.COM" + Environment.NewLine + "Whois Server: whois.example.net" + Environment.NewLine, output);
                CollectionAssert.AreEqual(new[] { "whois.example.net" }, _manager.FindWhoisServerInOutput(output).ToList());
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
