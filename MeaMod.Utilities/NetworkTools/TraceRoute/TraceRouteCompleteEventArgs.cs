using System;
using System.Collections.Generic;

namespace MeaMod.Utilities.NetworkTools.TraceRoute
{
    /// <summary>Event data for <see cref="TraceRouteManager.TraceRouteComplete"/>.</summary>
    public class TraceRouteCompleteEventArgs : EventArgs
    {
        /// <summary>Creates a new instance of this type.</summary>
        public TraceRouteCompleteEventArgs()
        {
        }

        /// <summary>Creates a new instance of this type.</summary>
        /// <param name="traceRouteHops">Every hop recorded by the trace, in order.</param>
        public TraceRouteCompleteEventArgs(List<TraceRouteHopDetail> traceRouteHops)
          : this()
        {
            this.TraceRouteHops = traceRouteHops;
        }

        /// <summary>Gets or sets every hop recorded by the trace, in order.</summary>
        public List<TraceRouteHopDetail> TraceRouteHops { get; set; }
    }
}
