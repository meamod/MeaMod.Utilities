using MeaMod.Utilities.NetworkTools.Whois;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class WhoisManagerTests
    {
        private readonly WhoisManager _manager = new();

        [TestMethod]
        public void FindWhoisServerInOutputWithoutTrailingNewline()
        {
            // The old code searched for the next newline, got -1 and passed a negative length to Substring.
            var servers = _manager.FindWhoisServerInOutput("Domain: example.com\nWhois Server: whois.example.net").ToList();

            CollectionAssert.AreEqual(new[] { "whois.example.net" }, servers);
        }

        [TestMethod]
        public void FindWhoisServerInOutputOnTheFirstLine()
        {
            // The old code treated an index of 0 as "not found".
            var servers = _manager.FindWhoisServerInOutput("Whois Server: whois.example.net\n").ToList();

            CollectionAssert.AreEqual(new[] { "whois.example.net" }, servers);
        }

        [TestMethod]
        public void FindWhoisServerInOutputHandlesCrLfAndMultipleMatches()
        {
            string output = "Whois Server: whois.one.example\r\nOther: x\r\n   Registrar WHOIS Server: whois.two.example\r\n";

            var servers = _manager.FindWhoisServerInOutput(output).ToList();

            CollectionAssert.AreEqual(new[] { "whois.one.example", "whois.two.example" }, servers);
        }

        [TestMethod]
        public void FindWhoisServerInOutputReturnsEmptyWhenAbsent()
        {
            Assert.AreEqual(0, _manager.FindWhoisServerInOutput("Domain: example.com\n").Count());
            Assert.AreEqual(0, _manager.FindWhoisServerInOutput("Whois Server:\n").Count());
        }

        [TestMethod]
        public void FindWhoisServerInOutputRejectsEmptyInput()
        {
            Assert.ThrowsExactly<ArgumentException>(() => _manager.FindWhoisServerInOutput(" "));
        }
    }
}
