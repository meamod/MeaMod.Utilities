using System;
using System.Net.NetworkInformation;

namespace MeaMod.Utilities.NetworkTools.IcmpPing
{
    /// <summary>Event data for <see cref="PingManager.PingResult"/>.</summary>
    public class PingResultEventArgs : EventArgs
    {
        /// <summary>Creates a new instance of this type.</summary>
        /// <param name="reply">The reply from the ping request, or null.</param>
        /// <param name="exception">The exception that was thrown, if any, or null.</param>
        public PingResultEventArgs(PingReply reply, Exception exception)
        {
            this.Reply = reply;
            // A reply with a non-success status (e.g. TimedOut) is not a successful ping.
            this.Success = reply != null && reply.Status == IPStatus.Success;
            this.LastException = exception;
        }

        /// <summary>Gets or sets the reply, or null when the ping threw.</summary>
        public PingReply Reply { get; set; }

        /// <summary>Gets or sets whether the host answered successfully.</summary>
        public bool Success { get; set; }

        /// <summary>Gets or sets the exception thrown by the ping, if any.</summary>
        public Exception LastException { get; set; }

        /// <summary>Formats the result in the style of the ping command line tool.</summary>
        /// <returns>A one line description of the result.</returns>
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
