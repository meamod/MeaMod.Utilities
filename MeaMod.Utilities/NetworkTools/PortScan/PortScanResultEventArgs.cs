using System;

namespace MeaMod.Utilities.NetworkTools.PortScan
{
    /// <summary>Event data for a port that was found open.</summary>
    public class PortScanResultEventArgs : EventArgs
    {
        /// <summary>Creates a new instance of this type.</summary>
        public PortScanResultEventArgs()
        {
        }

        /// <summary>Creates a new instance of this type.</summary>
        /// <param name="endPoint">The hostname or IP address that was scanned.</param>
        /// <param name="port">The port that was found open.</param>
        /// <param name="portType">The protocol the port was scanned with.</param>
        public PortScanResultEventArgs(string endPoint, int port, PortTypes portType)
          : this()
        {
            this.EndPoint = endPoint;
            this.Port = port;
            this.PortType = portType;
        }

        /// <summary>Gets or sets the port that was found open.</summary>
        public int Port { get; set; }

        /// <summary>Gets or sets the hostname or IP address that was scanned.</summary>
        public string EndPoint { get; set; }

        /// <summary>Gets or sets the protocol the port was scanned with.</summary>
        public PortTypes PortType { get; set; }

        /// <summary>Describes the result, e.g. "Tcp port 80 found on host example.test."</summary>
        /// <returns>A one line description of the result.</returns>
        public override string ToString() => string.Format("{0} port {1} found on host {2}.", (object)this.PortType, (object)this.Port, (object)this.EndPoint);
    }
}
