using System.Windows.Forms;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class ControlExtensionsTests
    {
        [TestMethod]
        public void RunOnUIThreadRunsInlineOnTheControlsThread()
        {
            StaThread.Run(() =>
            {
                using var control = new Control();
                _ = control.Handle;
                int ranOn = 0;

                control.RunOnUIThread(() => ranOn = Environment.CurrentManagedThreadId);

                Assert.AreEqual(Environment.CurrentManagedThreadId, ranOn);
            });
        }

        [TestMethod]
        public void RunOnUIThreadMarshalsFromAnotherThread()
        {
            StaThread.Run(() =>
            {
                using var control = new Control();
                _ = control.Handle;
                int uiThread = Environment.CurrentManagedThreadId;
                int ranOn = 0;

                var worker = new Thread(() => control.RunOnUIThread(() => Volatile.Write(ref ranOn, Environment.CurrentManagedThreadId)));
                worker.Start();
                worker.Join();

                Assert.IsTrue(StaThread.PumpUntil(() => Volatile.Read(ref ranOn) != 0, 5000), "the action was never run");
                Assert.AreEqual(uiThread, ranOn);
            });
        }
    }
}
