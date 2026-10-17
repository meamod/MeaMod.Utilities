using System.Net;
using System.Net.Sockets;
using MeaMod.Utilities.NetworkTools.Whois;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class WhoisManagerConnectionResetTests
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        [Timeout(60_000, CooperativeCancellation = true)]
        public void ConnectionResetByTheServerReturnsAnEmptyString()
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

            Task server = Task.Run(async () =>
            {
                using TcpClient client = await listener.AcceptTcpClientAsync(TestContext.CancellationToken);
                // A zero linger makes Close() send RST, so the client's read fails with an IOException.
                client.LingerState = new LingerOption(true, 0);
                client.Close();
            }, TestContext.CancellationToken);

            try
            {
                string output = new WhoisManager().ExecuteWhoisForDomain("example.com", "127.0.0.1");

                Assert.IsTrue(server.Wait(10_000, TestContext.CancellationToken), "the fake server did not finish");
                Assert.AreEqual(string.Empty, output);
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
