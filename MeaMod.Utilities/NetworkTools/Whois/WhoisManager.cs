using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace MeaMod.Utilities.NetworkTools.Whois
{
    /// <summary>Class for executing Whois requests.</summary>
    public class WhoisManager : IWhoisManager, INetworkManager
    {
        private const string DefaultWhoisLookupFormat = "{0}.whois-servers.net";

        private const int WhoisPort = 43;

        private const string WhoisServerMarker = "Whois Server:";

        /// <summary>Executes whois for a domain.</summary>
        /// <param name="domain">The domain to lookup.</param>
        /// <exception cref="T:System.ArgumentException"></exception>
        /// <returns>A text string with the whois information.</returns>
        public string ExecuteWhoisForDomain(string domain)
        {
            string[] strArray = !string.IsNullOrWhiteSpace(domain) ? domain.Split('.') : throw new ArgumentException("Argument \"domain\" cannot be null or empty.", nameof(domain));
            string whoisServer = string.Format(DefaultWhoisLookupFormat, (object)strArray[strArray.Length - 1]);
            return this.ExecuteWhoisForDomain(domain, whoisServer);
        }

        /// <summary>Executes whois for a domain.</summary>
        /// <param name="domain">The domain to lookup.</param>
        /// <param name="whoisServer">The whois server to use.</param>
        /// <exception cref="T:System.ArgumentException"></exception>
        /// <returns>A text string with the whois information, or an empty string if the server could not be reached.</returns>
        public string ExecuteWhoisForDomain(string domain, string whoisServer)
        {
            if (string.IsNullOrWhiteSpace(domain))
                throw new ArgumentException("Argument \"domain\" cannot be null or empty.", nameof(domain));
            if (string.IsNullOrWhiteSpace(whoisServer))
                throw new ArgumentException("Argument \"whoisServer\" cannot be null or empty.", nameof(whoisServer));

            StringBuilder stringBuilder = new();
            try
            {
                // TcpClient.Connect(host, port) tries every resolved address (IPv4 and IPv6) rather than only the
                // first one, which may be an IPv6 address that an IPv4-only socket cannot connect to.
                using TcpClient client = new();
                client.Connect(whoisServer, WhoisPort);
                using NetworkStream stream = client.GetStream();
                byte[] bytes = Encoding.ASCII.GetBytes(domain + "\r\n");
                stream.Write(bytes, 0, bytes.Length);

                byte[] buffer = new byte[1024];
                int count;
                while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                    stringBuilder.Append(Encoding.ASCII.GetString(buffer, 0, count));
            }
            catch (SocketException)
            {
            }
            catch (IOException)
            {
            }

            // Normalise line endings without turning a server's CRLF into CR CR LF.
            return stringBuilder.ToString().Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// Attempts to find the authority whois server, in another whois server output.
        /// </summary>
        /// <param name="whoisOutput">The whois output to search.</param>
        /// <exception cref="T:System.ArgumentException"></exception>
        /// <returns>A list of all of the found whois servers, or an empty list.</returns>
        public IEnumerable<string> FindWhoisServerInOutput(string whoisOutput)
        {
            if (string.IsNullOrWhiteSpace(whoisOutput))
                throw new ArgumentException("Argument \"whoisOutput\" cannot be null or empty.", nameof(whoisOutput));

            List<string> servers = [];

            // Line based: this copes with a match on the first line and with a final line that has no newline.
            foreach (string rawLine in whoisOutput.Split('\n'))
            {
                string line = rawLine.Trim();
                int index = line.IndexOf(WhoisServerMarker, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                    continue;

                string server = line.Substring(index + WhoisServerMarker.Length).Trim();
                if (server.Length > 0)
                    servers.Add(server);
            }

            return (IEnumerable<string>)servers;
        }
    }
}
