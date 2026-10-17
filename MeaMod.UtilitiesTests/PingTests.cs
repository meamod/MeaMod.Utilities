using System.Net.Sockets;
using MeaMod.Utilities.NetworkTools.IcmpPing;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class PingTests
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public void PingResultEventArgsToStringSurvivesAnExceptionWithoutInnerException()
        {
            // SocketException has no InnerException; the old code dereferenced it.
            var args = new PingResultEventArgs(null, new SocketException((int)SocketError.HostNotFound));

            Assert.IsFalse(args.Success);
            Assert.IsFalse(string.IsNullOrWhiteSpace(args.ToString()));
        }

        [TestMethod]
        public void PingResultEventArgsToStringSurvivesNoReplyAndNoException()
        {
            var args = new PingResultEventArgs(null, null);

            Assert.IsFalse(args.Success);
            Assert.IsFalse(string.IsNullOrWhiteSpace(args.ToString()));
        }

        [TestMethod]
        public void StopImmediatelyAfterStartStopsTheLoop()
        {
            // The old Stop() only cancelled when the task was already Running, so Start();Stop() left the loop running forever.
            var manager = new PingManager("127.0.0.1", TimeSpan.FromSeconds(1));
            using var stopped = new ManualResetEventSlim();
            int results = 0;
            manager.PingManagerStateChanged += (_, e) => { if (e.NewState == PingManagerStates.Stopped) stopped.Set(); };
            manager.PingResult += (_, _) => Interlocked.Increment(ref results);

            manager.Start();
            manager.Stop();

            Assert.IsTrue(stopped.Wait(TimeSpan.FromSeconds(10), TestContext.CancellationToken), "Stopped state was never raised");
            int afterStop = Volatile.Read(ref results);
            Thread.Sleep(2500);
            Assert.AreEqual(afterStop, Volatile.Read(ref results), "pings continued after Stop()");
        }

        [TestMethod]
        public void StopDoesNotWaitForTheWholeInterval()
        {
            // The old loop slept for TimeBetweenPings with Thread.Sleep, ignoring cancellation.
            var manager = new PingManager("127.0.0.1", TimeSpan.FromSeconds(20));
            using var firstResult = new ManualResetEventSlim();
            using var stopped = new ManualResetEventSlim();
            manager.PingResult += (_, _) => firstResult.Set();
            manager.PingManagerStateChanged += (_, e) => { if (e.NewState == PingManagerStates.Stopped) stopped.Set(); };

            manager.Start();
            Assert.IsTrue(firstResult.Wait(TimeSpan.FromSeconds(10), TestContext.CancellationToken), "no ping result");

            manager.Stop();
            Assert.IsTrue(stopped.Wait(TimeSpan.FromSeconds(3), TestContext.CancellationToken), "Stop() took longer than 3 seconds");
        }

        [TestMethod]
        public void StartTwiceDoesNotRunTwoLoops()
        {
            var manager = new PingManager("127.0.0.1", TimeSpan.FromSeconds(1));
            using var stopped = new ManualResetEventSlim();
            int pinging = 0;
            manager.PingManagerStateChanged += (_, e) =>
            {
                if (e.NewState == PingManagerStates.Pinging) Interlocked.Increment(ref pinging);
                if (e.NewState == PingManagerStates.Stopped) stopped.Set();
            };

            manager.Start();
            manager.Start();
            Thread.Sleep(2500);
            manager.Stop();
            Assert.IsTrue(stopped.Wait(TimeSpan.FromSeconds(10), TestContext.CancellationToken));

            // One loop pinging every second for ~2.5 s gives about 3 Pinging states; two loops would give about 6.
            Assert.IsLessThanOrEqualTo(4, Volatile.Read(ref pinging), $"{pinging} Pinging states");
        }
    }
}
