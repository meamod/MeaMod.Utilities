using System;

namespace MeaMod.Utilities.NetworkTools.TraceRoute
{
    public class TraceRouteHopDetail
    {
        public TraceRouteHopDetail()
        {
        }

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

        public int HopNumber { get; set; }

        public string IPAddress { get; set; }

        public string HostName { get; set; }

        /// <summary>The round trip time to this hop, or <see cref="TimeSpan.MaxValue"/> when it did not answer.</summary>
        public TimeSpan ResponseTime { get; set; }

        public static string FormattedTextHeader => "Hop IP Address       Response Host" + Environment.NewLine + "--- ---------------- -------- ---------------------------------------" + Environment.NewLine;

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
