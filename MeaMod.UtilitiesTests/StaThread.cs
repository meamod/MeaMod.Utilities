using System.Runtime.ExceptionServices;
using System.Windows.Forms;

namespace MeaMod.Utilities.Tests
{
    /// <summary>Runs WinForms code on a dedicated STA thread and pumps its message loop.</summary>
    internal static class StaThread
    {
        public static void Run(Action action, int timeoutMilliseconds = 60_000)
        {
            Exception? error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    error = e;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            if (!thread.Join(timeoutMilliseconds))
                throw new TimeoutException("The STA action did not finish in time.");

            if (error != null)
                ExceptionDispatchInfo.Capture(error).Throw();
        }

        /// <summary>Pumps the current thread's message loop until <paramref name="condition"/> holds or the timeout elapses.</summary>
        public static bool PumpUntil(Func<bool> condition, int timeoutMilliseconds)
        {
            long deadline = Environment.TickCount64 + timeoutMilliseconds;
            while (!condition())
            {
                if (Environment.TickCount64 > deadline)
                    return false;
                Application.DoEvents();
                Thread.Sleep(10);
            }

            return true;
        }

        /// <summary>Pumps the current thread's message loop for a fixed time.</summary>
        public static void Pump(int milliseconds)
        {
            long deadline = Environment.TickCount64 + milliseconds;
            while (Environment.TickCount64 < deadline)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
        }
    }
}
