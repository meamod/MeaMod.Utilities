using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MeaMod.Utilities.NetworkTools.TraceRoute
{
    /// <summary>Class for managing a network trace route.</summary>
    public class TraceRouteManager : ITraceRouteManager, INetworkManager
    {
        /// <summary>The highest TTL probed before giving up.</summary>
        private const int MaxHops = 30;

        /// <summary>Per-probe timeout in milliseconds.</summary>
        private const int ProbeTimeoutMilliseconds = 5000;

        /// <summary>Creates a new instance of this type.</summary>
        public TraceRouteManager() => this.CurrentCancellationTokenSource = new CancellationTokenSource();

        /// <summary>Event for when a trace route node is found.</summary>
        public event EventHandler<TraceRouteNodeFoundEventArgs> TraceRouteNodeFound;

        /// <summary>Event for when the trace route is complete.</summary>
        public event EventHandler<TraceRouteCompleteEventArgs> TraceRouteComplete;

        /// <summary>Gets the current cancellation token source.</summary>
        public CancellationTokenSource CurrentCancellationTokenSource { get; private set; }

        /// <summary>Executes the trace route.</summary>
        /// <param name="host">The destination host to which you wish to find the route.</param>
        /// <returns>All of the hops between the current computers and the
        /// <paramref name="host" /> computer, including the destination itself.</returns>
        /// <exception cref="T:System.ArgumentException"></exception>
        public Task<IEnumerable<TraceRouteHopDetail>> ExecuteTraceRoute(
          string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Argument \"host\" cannot be null or empty.", nameof(host));

            // A cancelled token source can never be reset, so a trace started after Cancel() needs a fresh one.
            if (this.CurrentCancellationTokenSource.IsCancellationRequested)
                this.CurrentCancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = this.CurrentCancellationTokenSource.Token;

            return Task<IEnumerable<TraceRouteHopDetail>>.Factory.StartNew((Func<IEnumerable<TraceRouteHopDetail>>)(() =>
           {
               List<TraceRouteHopDetail> traceRouteHops = new List<TraceRouteHopDetail>();
               using (Ping ping = new Ping())
               {
                   PingOptions options = new PingOptions(1, true);
                   byte[] buffer = new byte[32];

                   while (!token.IsCancellationRequested)
                   {
                       PingReply pingReply = ping.Send(host, ProbeTimeoutMilliseconds, buffer, options);

                       // Timed-out replies do not have a null Address on current .NET (it is the target or 0.0.0.0),
                       // so the reply status decides what kind of hop this is.
                       bool hasAddress = pingReply.Address != null && !IPAddress.Any.Equals(pingReply.Address) && !IPAddress.IPv6Any.Equals(pingReply.Address);
                       bool finalHop;
                       TraceRouteHopDetail detail;

                       switch (pingReply.Status)
                       {
                           case IPStatus.Success:
                               // The destination answered: this is the last hop.
                               finalHop = true;
                               detail = hasAddress ? this.CreateHopDetail(options.Ttl, pingReply.Address) : CreateTimedOutHopDetail(options.Ttl);
                               break;

                           case IPStatus.TtlExpired:
                               finalHop = false;
                               detail = hasAddress ? this.CreateHopDetail(options.Ttl, pingReply.Address) : CreateTimedOutHopDetail(options.Ttl);
                               break;

                           case IPStatus.TimedOut:
                               finalHop = false;
                               detail = CreateTimedOutHopDetail(options.Ttl);
                               break;

                           default:
                               // e.g. DestinationHostUnreachable: whoever reported it is the last reachable hop.
                               finalHop = true;
                               detail = hasAddress
                                   ? this.CreateHopDetail(options.Ttl, pingReply.Address)
                                   : new TraceRouteHopDetail(options.Ttl, "*", "***" + pingReply.Status + "***", TimeSpan.MaxValue);
                               break;
                       }

                       traceRouteHops.Add(detail);
                       this.TraceRouteNodeFound?.Invoke(this, new TraceRouteNodeFoundEventArgs(detail));

                       if (finalHop || options.Ttl >= MaxHops)
                           break;

                       ++options.Ttl;
                   }
               }

               this.TraceRouteComplete?.Invoke((object)this, new TraceRouteCompleteEventArgs(traceRouteHops));
               return (IEnumerable<TraceRouteHopDetail>)traceRouteHops;
           }), token);
        }

        /// <summary>
        /// Gets the response time to the specified <paramref name="host" />.
        /// </summary>
        /// <param name="host">The host to ping.</param>
        /// <returns>The round trip time, or <see cref="TimeSpan.MaxValue"/> when the host did not answer.</returns>
        /// <exception cref="T:System.ArgumentException"></exception>
        public TimeSpan GetResponseTime(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Argument \"ipAddress\" cannot be null or empty.", nameof(host));
            using (Ping ping = new Ping())
            {
                try
                {
                    PingReply reply = ping.Send(host, ProbeTimeoutMilliseconds);
                    // A timed-out reply has RoundtripTime 0, which must not be reported as a 0ms response.
                    if (reply.Status == IPStatus.Success)
                        return TimeSpan.FromMilliseconds(reply.RoundtripTime);
                }
                catch (PingException)
                {
                }
            }
            return TimeSpan.MaxValue;
        }

        /// <summary>Cancels the current execution.</summary>
        public void Cancel() => this.CurrentCancellationTokenSource.Cancel();

        private TraceRouteHopDetail CreateHopDetail(int hopNumber, IPAddress address)
        {
            string hostName = address.ToString();
            try
            {
                hostName = Dns.GetHostEntry(address).HostName;
            }
            catch (SocketException)
            {
            }

            TimeSpan responseTime = this.GetResponseTime(address.ToString());
            return new TraceRouteHopDetail(hopNumber, address.ToString(), hostName, responseTime);
        }

        private static TraceRouteHopDetail CreateTimedOutHopDetail(int hopNumber)
        {
            return new TraceRouteHopDetail(hopNumber, "*", "***Request Timed Out***", TimeSpan.MaxValue);
        }
    }
}
