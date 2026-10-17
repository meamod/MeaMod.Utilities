using System;

namespace MeaMod.Utilities.NetworkTools.TraceRoute
{
    /// <summary>One hop of a trace route.</summary>
    public class TraceRouteHopDetail
    {
        /// <summary>Creates a new instance of this type.</summary>
        public TraceRouteHopDetail()
        {
        }

        /// <summary>Creates a new instance of this type.</summary>
        /// <param name="hopNumber">The 1-based hop number (the TTL that reached it).</param>
        /// <param name="ipAddress">The address that answered, or "*" when nothing did.</param>
        /// <param name="hostName">The reverse-resolved host name, or a status text.</param>
        /// <param name="responseTime">The round trip time, or <see cref="TimeSpan.MaxValue"/> when it did not answer.</param>
        public TraceRouteHopDetail(
          int hopNumber,
          string ipAddress,
          string hostName,
          TimeSpan responseTime)
          : this()
        {
            this.HopNumber = hopNumber;
            this.IPAddress = ipAddress;
            this.HostName = hostName;
            this.ResponseTime = responseTime;
        }

        /// <summary>Gets or sets the 1-based hop number.</summary>
        public int HopNumber { get; set; }

        /// <summary>Gets or sets the address that answered, or "*" when nothing did.</summary>
        public string IPAddress { get; set; }

        /// <summary>Gets or sets the reverse-resolved host name, or a status text.</summary>
        public string HostName { get; set; }

        /// <summary>The round trip time to this hop, or <see cref="TimeSpan.MaxValue"/> when it did not answer.</summary>
        public TimeSpan ResponseTime { get; set; }

        /// <summary>Gets a column header that lines up with <see cref="ToString"/>.</summary>
        public static string FormattedTextHeader => "Hop IP Address       Response Host" + Environment.NewLine + "--- ---------------- -------- ---------------------------------------" + Environment.NewLine;

        /// <summary>Formats the hop as one fixed-width line.</summary>
        /// <returns>The hop number, address, response time and host name.</returns>
        public override string ToString()
        {
            // TotalMilliseconds, not the Milliseconds component: 1.5 s must print as 1,500ms rather than 500ms.
            string response = this.ResponseTime == TimeSpan.MaxValue
                ? "*"
                : string.Format("{0:N0}ms", this.ResponseTime.TotalMilliseconds);
            return string.Format("{0,3} {1,-16} {2,8} {3}", (object)this.HopNumber, (object)this.IPAddress, (object)response, (object)this.HostName);
        }
    }
}
