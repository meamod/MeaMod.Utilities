using System.Net.NetworkInformation;
using MeaMod.Utilities.NetworkTools.IcmpPing;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class PingManagerMoreTests
    {
        [TestMethod]
        public void ConstructorRejectsBadArguments()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new PingManager(" ", TimeSpan.FromSeconds(1)));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PingManager("127.0.0.1", TimeSpan.FromMilliseconds(500)));
        }

        [TestMethod]
        public void PropertiesReflectTheConstructorArguments()
        {
            var manager = new PingManager("127.0.0.1", TimeSpan.FromSeconds(2));

            Assert.AreEqual("127.0.0.1", manager.HostName);
            Assert.AreEqual(TimeSpan.FromSeconds(2), manager.TimeBetweenPings);
        }

        [TestMethod]
        public void StopWithoutStartRaisesStopped()
        {
            var manager = new PingManager("127.0.0.1", TimeSpan.FromSeconds(1));
            PingManagerStates? state = null;
            manager.PingManagerStateChanged += (_, e) => state = e.NewState;

            manager.Stop();

            Assert.AreEqual(PingManagerStates.Stopped, state);
        }

        [TestMethod]
        [Timeout(60_000)]
        public void UnresolvableHostReportsAPingException()
        {
            var manager = new PingManager("host.invalid", TimeSpan.FromSeconds(1));
            using var got = new ManualResetEventSlim();
            using var stopped = new ManualResetEventSlim();
            PingResultEventArgs? result = null;
            var states = new List<PingManagerStates>();
            manager.PingResult += (_, e) => { result ??= e; got.Set(); };
            manager.PingManagerStateChanged += (_, e) =>
            {
                lock (states) states.Add(e.NewState);
                if (e.NewState == PingManagerStates.Stopped) stopped.Set();
            };

            manager.Start();
            Assert.IsTrue(got.Wait(TimeSpan.FromSeconds(30)), "no ping result");
            manager.Stop();
            Assert.IsTrue(stopped.Wait(TimeSpan.FromSeconds(10)));

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Reply);
            Assert.IsInstanceOfType<PingException>(result.LastException);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.ToString()));
            lock (states)
            {
                CollectionAssert.Contains(states, PingManagerStates.Pinging);
                CollectionAssert.Contains(states, PingManagerStates.Idle);
            }
        }

        [TestMethod]
        public void SuccessfulReplyFormatsLikePing()
        {
            using var ping = new Ping();
            PingReply reply = ping.Send("127.0.0.1", 2000);
            if (reply.Status != IPStatus.Success)
                Assert.Inconclusive("loopback did not answer: " + reply.Status);

            var args = new PingResultEventArgs(reply, null);

            Assert.IsTrue(args.Success);
            Assert.AreSame(reply, args.Reply);
            Assert.IsNull(args.LastException);
            StringAssert.StartsWith(args.ToString(), "Reply from 127.0.0.1");
        }

        [TestMethod]
        public void UnsuccessfulReplyIsNotASuccess()
        {
            using var ping = new Ping();
            PingReply reply;
            try
            {
                // TEST-NET-1 is never routed, so this times out or is reported unreachable.
                reply = ping.Send("192.0.2.1", 500);
            }
            catch (PingException)
            {
                Assert.Inconclusive("no network stack available for the probe");
                return;
            }

            if (reply.Status == IPStatus.Success)
                Assert.Inconclusive("192.0.2.1 unexpectedly answered");

            var args = new PingResultEventArgs(reply, null);

            Assert.IsFalse(args.Success);
            StringAssert.Contains(args.ToString(), reply.Status.ToString());
        }

        [TestMethod]
        public void StateChangedEventArgsCarriesTheState()
        {
            var args = new PingManagerStateChangedEventArgs(PingManagerStates.Idle);
            Assert.AreEqual(PingManagerStates.Idle, args.NewState);

            args.NewState = PingManagerStates.Unknown;
            Assert.AreEqual(PingManagerStates.Unknown, args.NewState);
        }

        [TestMethod]
        public void ResultEventArgsSettersWork()
        {
            var args = new PingResultEventArgs(null, null) { Success = true, LastException = new InvalidOperationException("x") };

            Assert.IsTrue(args.Success);
            Assert.IsInstanceOfType<InvalidOperationException>(args.LastException);
            args.Reply = null;
            Assert.IsNull(args.Reply);
        }
    }
}
