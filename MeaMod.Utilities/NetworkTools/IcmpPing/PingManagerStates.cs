namespace MeaMod.Utilities.NetworkTools.IcmpPing
{
    /// <summary>The states a <see cref="PingManager"/> moves through.</summary>
    public enum PingManagerStates
    {
        /// <summary>The state has not been set.</summary>
        Unknown,
        /// <summary>A ping is in flight.</summary>
        Pinging,
        /// <summary>Waiting for the next ping interval.</summary>
        Idle,
        /// <summary>The ping loop has stopped.</summary>
        Stopped,
    }
}
