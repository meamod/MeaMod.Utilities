using System;
using System.Diagnostics;

namespace MeaMod.Utilities
{
    /// <summary>The LogEvent class contains methods to access log to the Windows Event Log</summary>
    public class LogEvent
    {
        /// <summary>Writes <paramref name="message"/> to Application with type Information and ID 1000</summary>
        /// <param name="source">The source by which the application is registered on the specified computer.</param>
        /// <param name="message">The string to write to the event log.</param>
        /// <param name="isConsole">Write to console instead of Event Log</param>
        public static void Write(string source, string message, bool isConsole = false)
        {
            Write(source, message, "Application", EventLogEntryType.Information, 1000, isConsole);
        }

        /// <summary>Writes <paramref name="message"/> to <paramref name="logName"/> with type Information and ID 1000</summary>
        /// <param name="source">The source by which the application is registered on the specified computer.</param>
        /// <param name="message">The string to write to the event log.</param>
        /// <param name="logName">The destination log to write to.</param>
        /// <param name="isConsole">Write to console instead of Event Log</param>
        public static void Write(string source, string message, string logName, bool isConsole = false)
        {
            Write(source, message, logName, EventLogEntryType.Information, 1000, isConsole);
        }

        /// <summary>Writes <paramref name="message"/> to <paramref name="logName"/> with <paramref name="eventType"/> and ID 1000</summary>
        /// <param name="source">The source by which the application is registered on the specified computer.</param>
        /// <param name="message">The string to write to the event log.</param>
        /// <param name="logName">The destination log to write to.</param>
        /// <param name="eventType">One of the EventLogEntryType values.</param>
        /// <param name="isConsole">Write to console instead of Event Log</param>
        public static void Write(string source, string message, string logName, EventLogEntryType eventType, bool isConsole = false)
        {
            Write(source, message, logName, eventType, 1000, isConsole);
        }

        /// <summary>Writes <paramref name="message"/> to <paramref name="logName"/> with <paramref name="eventType"/> and <paramref name="eventID"/></summary>
        /// <param name="source">The source by which the application is registered on the specified computer.</param>
        /// <param name="message">The string to write to the event log.</param>
        /// <param name="logName">The destination log to write to.</param>
        /// <param name="eventType">One of the EventLogEntryType values.</param>
        /// <param name="eventID">The application-specific identifier for the event.</param>
        /// <param name="isConsole">Write to console instead of Event Log</param>
        public static void Write(string source, string message, string logName, EventLogEntryType eventType, int eventID, bool isConsole = false)
        {
            if (isConsole == true)
            {
                Console.WriteLine(message);
            }
            else
            {
                try
                {
                    if (!EventLog.SourceExists(source))
                    {
                        EventLog.CreateEventSource(source, logName);
                    }

                    using (var objEventLog = new EventLog { Source = source })
                    {
                        objEventLog.WriteEntry(message, eventType, eventID);
                    }
                }
                catch (Exception)
                {
                }

                Debug.WriteLine(message);
            }
        }

        /// <summary>Writes to the Windows Event Log</summary>
        /// <param name="source">The source by which the application is registered on the specified computer.</param>
        /// <param name="message">The string to write to the event log.</param>
        /// <param name="isConsole">Write to console instead of Event Log</param>
        [Obsolete("This method is obsolete. Call Write instead.", false)]
        public static void WriteConsole(string source, string message, bool isConsole = false)
        {
            Write(source, message, "Application", EventLogEntryType.Information, 1000, isConsole);
        }

        /// <summary>Writes to the Windows Event Log</summary>
        /// <param name="source">The source by which the application is registered on the specified computer.</param>
        /// <param name="message">The string to write to the event log.</param>
        /// <param name="logName">The destination log to write to.</param>
        /// <param name="isConsole">Write to console instead of Event Log</param>
        [Obsolete("This method is obsolete. Call Write instead.", false)]
        public static void WriteConsole(string source, string message, string logName, bool isConsole = false)
        {
            Write(source, message, logName, EventLogEntryType.Information, 1000, isConsole);
        }

        /// <summary>Writes to the Windows Event Log</summary>
        /// <param name="source">The source by which the application is registered on the specified computer.</param>
        /// <param name="message">The string to write to the event log.</param>
        /// <param name="logName">The destination log to write to.</param>
        /// <param name="eventType">One of the EventLogEntryType values.</param>
        /// <param name="isConsole">Write to console instead of Event Log</param>
        [Obsolete("This method is obsolete. Call Write instead.", false)]
        public static void WriteConsole(string source, string message, string logName, EventLogEntryType eventType, bool isConsole = false)
        {
            Write(source, message, logName, eventType, 1000, isConsole);
        }

        /// <summary>Writes to the Windows Event Log</summary>
        /// <param name="source">The source by which the application is registered on the specified computer.</param>
        /// <param name="message">The string to write to the event log.</param>
        /// <param name="logName">The destination log to write to.</param>
        /// <param name="eventType">One of the EventLogEntryType values.</param>
        /// <param name="eventID">The application-specific identifier for the event.</param>
        /// <param name="isConsole">Write to console instead of Event Log</param>
        [Obsolete("This method is obsolete. Call Write instead.", false)]
        public static void WriteConsole(string source, string message, string logName, EventLogEntryType eventType, int eventID, bool isConsole = false)
        {
            Write(source, message, logName, eventType, eventID, isConsole);
        }

        /// <summary>Check if <paramref name="source"/> exists in <paramref name="logName"/> and try to create it if does not exist</summary>
        /// <param name="source">The source by which the application is registered on the specified computer.</param>
        /// <param name="logName">The destination log name</param>
        /// <returns>True or False if registry source exists</returns>
        public static bool CheckSourceExists(string source, string logName)
        {
            if (EventLog.SourceExists(source))
            {
                using (var evLog = new EventLog() { Source = source })
                {
                    if ((evLog.Log ?? "") != (logName ?? ""))
                    {
                        EventLog.DeleteEventSource(source);
                    }
                }
            }

            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, logName);
                EventLog.WriteEntry(source, string.Format("Event Log Created '{0}'/'{1}'", logName, source), EventLogEntryType.Information);
            }

            var eventLogs = EventLog.GetEventLogs();
            foreach (var e in eventLogs)
            {
                using (e)
                {
                    if ((e.Log ?? "") == (logName ?? ""))
                    {
                        if (e.MaximumKilobytes < 1048576L)
                        {
                            e.MaximumKilobytes = 1048576L;
                        }
                    }
                }
            }

            return EventLog.SourceExists(source);
        }
    }
}