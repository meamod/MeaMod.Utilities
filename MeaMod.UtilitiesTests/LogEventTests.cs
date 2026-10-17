#pragma warning disable CS0618 // the WriteConsole overloads and Logging.LogEvent are obsolete but still shipped
using System.Diagnostics;
using RootLogEvent = MeaMod.Utilities.LogEvent;
using LoggingLogEvent = MeaMod.Utilities.Logging.LogEvent;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class LogEventTests
    {
        private static string Capture(Action action)
        {
            var writer = new StringWriter();
            TextWriter original = Console.Out;
            Console.SetOut(writer);
            try
            {
                action();
            }
            finally
            {
                Console.SetOut(original);
            }

            return writer.ToString();
        }

        [TestMethod]
        public void WriteOverloadsWriteToTheConsoleWhenAsked()
        {
            Assert.Contains("one", Capture(() => RootLogEvent.Write("Src", "one", true)));
            Assert.Contains("two", Capture(() => RootLogEvent.Write("Src", "two", "Application", true)));
            Assert.Contains("three", Capture(() => RootLogEvent.Write("Src", "three", "Application", EventLogEntryType.Warning, true)));
            Assert.Contains("four", Capture(() => RootLogEvent.Write("Src", "four", "Application", EventLogEntryType.Error, 1234, true)));
        }

        [TestMethod]
        public void WriteConsoleOverloadsForwardToWrite()
        {
            Assert.Contains("one", Capture(() => RootLogEvent.WriteConsole("Src", "one", true)));
            Assert.Contains("two", Capture(() => RootLogEvent.WriteConsole("Src", "two", "Application", true)));
            Assert.Contains("three", Capture(() => RootLogEvent.WriteConsole("Src", "three", "Application", EventLogEntryType.Warning, true)));
            Assert.Contains("four", Capture(() => RootLogEvent.WriteConsole("Src", "four", "Application", EventLogEntryType.Error, 1234, true)));
        }

        [TestMethod]
        public void WriteToTheEventLogSwallowsFailures()
        {
            // A 300 character source cannot exist and cannot be created, so nothing is written or
            // registered even when the tests run elevated; the failure must be swallowed.
            string source = new('x', 300);

            RootLogEvent.Write(source, "ignored", "Application", EventLogEntryType.Information, 1000, false);
        }

        [TestMethod]
        public void LoggingNamespaceAliasStillWorks()
        {
            Assert.IsInstanceOfType<RootLogEvent>(new LoggingLogEvent());
            Assert.Contains("alias", Capture(() => LoggingLogEvent.Write("Src", "alias", true)));
        }

        [TestMethod]
        public void EventTypeValuesAreStable()
        {
            Assert.AreEqual(1, (int)Logging.EventType.Error);
            Assert.AreEqual(2, (int)Logging.EventType.Warning);
            Assert.AreEqual(3, (int)Logging.EventType.Information);
        }
    }
}
