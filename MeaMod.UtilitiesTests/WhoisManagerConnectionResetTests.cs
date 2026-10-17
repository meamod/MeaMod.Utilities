using System.Net;
using System.Net.Sockets;
using MeaMod.Utilities.NetworkTools.Whois;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class WhoisManagerConnectionResetTests
    {
        [TestMethod]
        [Timeout(60_000)]
        public void ConnectionResetByTheServerReturnsAnEmptyString()
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

            Task server = Task.Run(async () =>
            {
                using TcpClient client = await listener.AcceptTcpClientAsync();
                // A zero linger makes Close() send RST, so the client's read fails with an IOException.
                client.LingerState = new LingerOption(true, 0);
                client.Close();
            });

            try
            {
                string output = new WhoisManager().ExecuteWhoisForDomain("example.com", "127.0.0.1");

                Assert.IsTrue(server.Wait(TimeSpan.FromSeconds(10)), "the fake server did not finish");
                Assert.AreEqual(string.Empty, output);
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
