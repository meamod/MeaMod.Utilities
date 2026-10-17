#if !NETSTANDARD
using System;
using System.Windows.Forms;

namespace MeaMod.Utilities
{
    /// <summary>Extension methods for <see cref="Control"/>.</summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// Runs <paramref name="code"/> on the thread that owns <paramref name="this"/>: inline when already on it,
        /// otherwise posted with <see cref="Control.BeginInvoke(Delegate)"/> without blocking the caller.
        /// </summary>
        /// <param name="this">The control whose thread the code must run on.</param>
        /// <param name="code">The code to run.</param>
        public static void RunOnUIThread(this Control @this, Action code)
        {
            if (@this.InvokeRequired)
            {
                @this.BeginInvoke(code);
            }
            else
            {
                code.Invoke();
            }
        }
    }
}
#endif
