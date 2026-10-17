using System;

namespace MeaMod.Utilities.Logging
{
    /// <summary>
    /// Kept for source and binary compatibility only. The implementation lives in <see cref="global::MeaMod.Utilities.LogEvent"/>;
    /// its static members remain callable through this type name, so existing callers keep compiling.
    /// </summary>
    [Obsolete("Use MeaMod.Utilities.LogEvent instead; this duplicate type will be removed in a future version.", false)]
    public class LogEvent : global::MeaMod.Utilities.LogEvent
    {
    }
}
