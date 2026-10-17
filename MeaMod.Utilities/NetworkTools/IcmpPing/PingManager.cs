using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MeaMod.Utilities.NetworkTools.IcmpPing
{
    /// <summary>Class who can perform an ICMP ping to a network host.</summary>
    public class PingManager : IPingManager, INetworkManager
    {
        private Task pingTask;
        private CancellationTokenSource pingCancellationToken = new();

        /// <summary>Creates a new instance of this type.</summary>
        /// <param name="hostName">The hostname to ping.</param>
        /// <param name="timeBetweenPings">The time span in between ping attempts.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="T:System.ArgumentException"></exception>
        public PingManager(string hostName, TimeSpan timeBetweenPings)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Argument \"HostName\" cannot be null or empty.", nameof(hostName));
            if (timeBetweenPings.TotalSeconds < 1.0)
                throw new ArgumentOutOfRangeException(nameof(timeBetweenPings), "Argument \"timeBetweenPings\" cannot be less than 1 second.");
            this.HostName = hostName;
            this.TimeBetweenPings = timeBetweenPings;
        }

        /// <summary>Gets the current hostname who will be pinged.</summary>
        public string HostName { get; protected set; }

        /// <summary>Gets the time span in between ping attempts.</summary>
        public TimeSpan TimeBetweenPings { get; protected set; }

        /// <summary>
        /// Event for when the state of this ping manager changes. (e.g. starting, stopping, etc)
        /// </summary>
        public event EventHandler<PingManagerStateChangedEventArgs> PingManagerStateChanged;

        /// <summary>
        /// Event for when the ping process determines a result, including no response.
        /// </summary>
        public event EventHandler<PingResultEventArgs> PingResult;

        /// <summary>
        /// Starts the pinging process. Check the <see cref="E:MeaMod.Utilities.NetworkTools.IcmpPing.PingManager.PingResult" /> event for the async results of the ping.
        /// </summary>
        public virtual void Start()
        {
            // IsCompleted covers every "still alive" state (WaitingToRun, Running, ...), not just Running,
            // so two quick Start() calls cannot spawn two ping loops.
            if (this.pingTask != null && !this.pingTask.IsCompleted)
                return;
            this.pingCancellationToken = new CancellationTokenSource();
            // The loop itself observes the token; it is not passed to StartNew so that ExecutePing always runs
            // and always raises the Stopped state when it exits.
            this.pingTask = Task.Factory.StartNew(new Action(this.ExecutePing), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>Triggers the stop of the pinging process.</summary>
        public virtual void Stop()
        {
            Task task = this.pingTask;
            if (task != null && !task.IsCompleted)
            {
                // The ping loop raises the Stopped state once it has actually exited.
                this.pingCancellationToken?.Cancel();
            }
            else
            {
                this.OnStateChangedEvent(PingManagerStates.Stopped);
            }
        }

        /// <summary>Actually execute the ping.</summary>
        protected virtual void ExecutePing()
        {
            CancellationToken token = this.pingCancellationToken.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    this.OnStateChangedEvent(PingManagerStates.Pinging);
                    using (Ping ping = new())
                    {
                        try
                        {
                            this.OnPingResult(ping.Send(this.HostName, 5000), (Exception)null);
                        }
                        catch (PingException ex)
                        {
                            this.OnPingResult((PingReply)null, (Exception)ex);
                        }
                        catch (SocketException ex)
                        {
                            this.OnPingResult((PingReply)null, (Exception)ex);
                        }
                    }
                    this.OnStateChangedEvent(PingManagerStates.Idle);
                    // Wakes up as soon as Stop() cancels, instead of sleeping for the whole interval.
                    if (token.WaitHandle.WaitOne(this.TimeBetweenPings))
                        break;
                }
            }
            finally
            {
                this.OnStateChangedEvent(PingManagerStates.Stopped);
            }
        }

        /// <summary>
        /// Event handler for when the ping manager state is about to change.
        /// </summary>
        /// <param name="newState">The new state of the ping manager.</param>
        protected virtual void OnStateChangedEvent(PingManagerStates newState)
        {
            this.PingManagerStateChanged?.Invoke(this, new PingManagerStateChangedEventArgs(newState));
        }

        /// <summary>
        /// Event handler for when the ping manager state is about to change.
        /// </summary>
        /// <param name="reply">The reply from the ping request, or null.</param>
        /// <param name="exception">The exception that was thrown, if any, or null.</param>
        protected virtual void OnPingResult(PingReply reply, Exception exception)
        {
            this.PingResult?.Invoke(this, new PingResultEventArgs(reply, exception));
        }
    }
}
