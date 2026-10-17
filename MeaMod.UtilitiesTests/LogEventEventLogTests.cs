using System.Diagnostics;
using System.Security;
using System.Security.Principal;
using RootLogEvent = MeaMod.Utilities.LogEvent;

namespace MeaMod.Utilities.Tests
{
    /// <summary>
    /// Tests that touch the real Windows event log. Writing uses the pre-registered "Application" source, which
    /// interactive users may write to; creating sources and logs needs elevation and is skipped otherwise.
    /// </summary>
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class LogEventEventLogTests
    {
        private const string Marker = "MeaMod.Utilities.Tests event log write test";

        private static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        private static bool? SourceExists(string source)
        {
            try
            {
                return EventLog.SourceExists(source);
            }
            catch (SecurityException)
            {
                return null;
            }
        }

        [TestMethod]
        public void WriteToAnExistingSourceWritesAnEntry()
        {
            if (SourceExists("Application") != true)
                Assert.Inconclusive("the Application source cannot be used from this account");

            string message = Marker + " " + Guid.NewGuid().ToString("N");

            RootLogEvent.Write("Application", message, "Application", EventLogEntryType.Information, 1000, false);

            using var log = new EventLog("Application");
            int count = log.Entries.Count;
            bool found = false;
            for (int i = count - 1; i >= Math.Max(0, count - 100) && !found; i--)
                found = log.Entries[i].Message.Contains(message, StringComparison.Ordinal);

            Assert.IsTrue(found, "the entry was not written to the Application log");
        }

        [TestMethod]
        public void CheckSourceExistsCreatesTheSourceAndLogWhenElevated()
        {
            if (!IsElevated)
            {
                Assert.Inconclusive("creating event sources requires elevation");
                return;
            }

            const string logName = "MeaModTst";
            const string source = "MeaModTstSource";
            try
            {
                Assert.IsTrue(RootLogEvent.CheckSourceExists(source, logName), "first call");
                Assert.IsTrue(RootLogEvent.CheckSourceExists(source, logName), "second call");
                Assert.IsTrue(EventLog.SourceExists(source));
            }
            finally
            {
                if (EventLog.SourceExists(source))
                    EventLog.DeleteEventSource(source);
                if (EventLog.Exists(logName))
                    EventLog.Delete(logName);
            }
        }
    }
}
