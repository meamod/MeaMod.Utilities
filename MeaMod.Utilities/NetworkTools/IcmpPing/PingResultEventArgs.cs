using System;
using System.Net.NetworkInformation;

namespace MeaMod.Utilities.NetworkTools.IcmpPing
{
    public class PingResultEventArgs : EventArgs
    {
        public PingResultEventArgs(PingReply reply, Exception exception)
        {
            this.Reply = reply;
            // A reply with a non-success status (e.g. TimedOut) is not a successful ping.
            this.Success = reply != null && reply.Status == IPStatus.Success;
            this.LastException = exception;
        }

        public PingReply Reply { get; set; }

        public bool Success { get; set; }

        public Exception LastException { get; set; }

        public override string ToString()
        {
            if (this.Success)
            {
                return string.Format("Reply from {0}: bytes={1} time={2}ms TTL={3}",
                    this.Reply.Address,
                    this.Reply.Buffer?.Length ?? 0,
                    this.Reply.RoundtripTime,
                    this.Reply.Options?.Ttl ?? 0);
            }

            if (this.Reply != null)
                return string.Format("Request to {0} failed: {1}", this.Reply.Address, this.Reply.Status);

            // SocketException has no inner exception; PingException usually does.
            return this.LastException?.InnerException?.Message
                   ?? this.LastException?.Message
                   ?? "Ping failed for an unknown reason.";
        }
    }
}
