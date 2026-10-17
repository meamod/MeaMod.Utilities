using System;

namespace MeaMod.Utilities.NetworkTools.IcmpPing
{
    /// <summary>Event data for <see cref="PingManager.PingManagerStateChanged"/>.</summary>
    public class PingManagerStateChangedEventArgs : EventArgs
    {
        /// <summary>Creates a new instance of this type.</summary>
        /// <param name="newState">The state the manager has moved to.</param>
        public PingManagerStateChangedEventArgs(PingManagerStates newState) => this.NewState = newState;

        /// <summary>Gets or sets the state the manager has moved to.</summary>
        public PingManagerStates NewState { get; set; }
    }
}
