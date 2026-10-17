using MeaMod.Utilities.NetworkTools.Whois;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class WhoisManagerTests
    {
        private readonly WhoisManager _manager = new();
        private static readonly string[] expected = ["whois.example.net"];
        private static readonly string[] expectedArray = ["whois.one.example", "whois.two.example"];

        [TestMethod]
        public void FindWhoisServerInOutputWithoutTrailingNewline()
        {
            // The old code searched for the next newline, got -1 and passed a negative length to Substring.
            var servers = _manager.FindWhoisServerInOutput("Domain: example.com\nWhois Server: whois.example.net").ToList();

            Assert.AreSequenceEqual(expected, servers);
        }

        [TestMethod]
        public void FindWhoisServerInOutputOnTheFirstLine()
        {
            // The old code treated an index of 0 as "not found".
            var servers = _manager.FindWhoisServerInOutput("Whois Server: whois.example.net\n").ToList();

            Assert.AreSequenceEqual(expected, servers);
        }

        [TestMethod]
        public void FindWhoisServerInOutputHandlesCrLfAndMultipleMatches()
        {
            string output = "Whois Server: whois.one.example\r\nOther: x\r\n   Registrar WHOIS Server: whois.two.example\r\n";

            var servers = _manager.FindWhoisServerInOutput(output).ToList();

            Assert.AreSequenceEqual(expectedArray, servers);
        }

        [TestMethod]
        public void FindWhoisServerInOutputReturnsEmptyWhenAbsent()
        {
            Assert.IsEmpty(_manager.FindWhoisServerInOutput("Domain: example.com\n"));
            Assert.IsEmpty(_manager.FindWhoisServerInOutput("Whois Server:\n"));
        }

        [TestMethod]
        public void FindWhoisServerInOutputRejectsEmptyInput()
        {
            Assert.ThrowsExactly<ArgumentException>(() => _manager.FindWhoisServerInOutput(" "));
        }
    }
}
