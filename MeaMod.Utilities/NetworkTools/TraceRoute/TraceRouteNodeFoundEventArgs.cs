using System;

namespace MeaMod.Utilities.NetworkTools.TraceRoute
{
    /// <summary>Event data for <see cref="TraceRouteManager.TraceRouteNodeFound"/>.</summary>
    public class TraceRouteNodeFoundEventArgs : EventArgs
    {
        /// <summary>Creates a new instance of this type.</summary>
        public TraceRouteNodeFoundEventArgs()
        {
        }

        /// <summary>Creates a new instance of this type.</summary>
        /// <param name="detail">The hop that was found.</param>
        public TraceRouteNodeFoundEventArgs(TraceRouteHopDetail detail)
          : this()
        {
            this.Detail = detail;
        }

        /// <summary>Gets or sets the hop that was found.</summary>
        public TraceRouteHopDetail Detail { get; set; }
    }
}
