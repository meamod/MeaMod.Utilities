using System;
using System.Net;
using System.Net.Sockets;

namespace MeaMod.Utilities.NetworkTools.PortScan
{
    /// <summary>Class for scanning a specific port.</summary>
    public class PortScanner
    {
        /// <summary>How long a UDP probe waits for a reply or an ICMP "port unreachable" before the port is treated as open or filtered.</summary>
        private const int UdpProbeTimeoutMilliseconds = 1000;

        /// <summary>Creates a new instance of this type.</summary>
        /// <param name="endPointName"></param>
        /// <param name="port"></param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="T:System.ArgumentException"></exception>
        public PortScanner(string endPointName, int port)
        {
            if (string.IsNullOrWhiteSpace(endPointName))
                throw new ArgumentException("Argument \"endPointName\" cannot be null or empty.", nameof(endPointName));
            if (port < 1 || port > (int)ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(port), "Argument \"port\" must be greater than zero and less than 65535.");
            this.EndPointName = endPointName;
            this.Port = port;
        }

        /// <summary>Gets the port being scanned.</summary>
        public int Port { get; private set; }

        /// <summary>Gets the endpoint name (hostname, or IP address).</summary>
        public string EndPointName { get; private set; }

        /// <summary>Events for when the connection attempt is complete.</summary>
        public event EventHandler<PortScanResultEventArgs> PortScanResult;

        /// <summary>Begins the connection attempt via TCP.</summary>
        public void AttemptTcpConnectionToPort()
        {
            try
            {
                // Disposed on every path, including a failed connect, so a full-range scan does not leak sockets.
                using (TcpClient tcpClient = new TcpClient { ExclusiveAddressUse = false })
                {
                    tcpClient.Connect(this.EndPointName, this.Port);
                }
            }
            catch (SocketException)
            {
                return;
            }

            this.PortScanResult?.Invoke(this, new PortScanResultEventArgs(this.EndPointName, this.Port, PortTypes.Tcp));
        }

        /// <summary>
        /// Begins the connection attempt via UDP. UDP is connectionless, so a probe datagram is sent and the port is
        /// reported when a reply arrives or nothing comes back (open or filtered); an ICMP "port unreachable" means closed.
        /// </summary>
        public void AttemptUdpConnectionToPort()
        {
            bool openOrFiltered;
            try
            {
                using (UdpClient udpClient = new UdpClient { ExclusiveAddressUse = false })
                {
                    udpClient.Connect(this.EndPointName, this.Port);
                    udpClient.Client.ReceiveTimeout = UdpProbeTimeoutMilliseconds;
                    udpClient.Send(new byte[1], 1);

                    IPEndPoint remote = null;
                    try
                    {
                        udpClient.Receive(ref remote);
                        openOrFiltered = true;
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        openOrFiltered = true;
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        // ICMP port unreachable: nothing is listening.
                        openOrFiltered = false;
                    }
                }
            }
            catch (SocketException)
            {
                return;
            }

            if (openOrFiltered)
                this.PortScanResult?.Invoke(this, new PortScanResultEventArgs(this.EndPointName, this.Port, PortTypes.Udp));
        }
    }
}
