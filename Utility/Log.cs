using System;
using System.IO;
using System.Threading;

namespace Utility
{
    /// <summary>
    /// The Class used to Log.
    /// </summary>
    public static class Log
    {
        private static TextWriter Tw;
        private static readonly object SyncObject = new object();

        /// <summary>
        /// The Directory Log.
        /// </summary>
        public static string LogDirectory { get; private set; }

        /// <summary>
        /// The file path of log.
        /// </summary>
        public static string LogFilePath { get; private set; }

        /// <summary>
        /// The count of error.
        /// </summary>
        public static int ErrorCount { get; private set; }

        /// <summary>
        /// Initialize Log instance
        /// </summary>
        static Log()
        {
            LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            };

            LogFilePath = Path.Combine(LogDirectory,
                Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName) + DateTime.UtcNow.ToFileTime() + ".log");

            if (File.Exists(LogFilePath))
            {
                File.Delete(LogFilePath);
            }

            Tw = TextWriter.Synchronized(new StreamWriter(LogFilePath, true));
        }

        /// <summary>
        /// Write debug log.
        /// </summary>
        /// <param name="message"> A composite format string.</param>
        /// <param name="para">An object array that contains zero or more objects to format.</param>
        public static void WriteDebugLog(string message, params object[] para)
        {
#if DEBUG
            WriteLog(message, para);
#endif
        }

        /// <summary>
        /// Write error log.
        /// </summary>
        /// <param name="message"> A composite format string.</param>
        /// <param name="para">An object array that contains zero or more objects to format.</param>
        public static void WriteErrorLog(string message, params object[] para)
        {
            ErrorCount++;
            WriteLog("Error: " + message, para);
        }

        /// <summary>
        /// Write info log.
        /// </summary>
        /// <param name="message"> A composite format string.</param>
        /// <param name="para">An object array that contains zero or more objects to format.</param>
        public static void WriteInfoLog(string message, params object[] para)
        {
            WriteLog("Info: " + message, para);
        }

        /// <summary>
        /// Write log.
        /// </summary>
        /// <param name="message"> A composite format string.</param>
        /// <param name="para">An object array that contains zero or more objects to format.</param>
        public static void WriteLog(string message, params object[] para)
        {
            try
            {
                string formatedString = string.Format(message, para);
                WriteLog(formatedString, Tw);
            }
            catch (Exception)
            {
                WriteDirect("Encountered exception when writing log: " + message);
                throw;
            }
        }

        /// <summary>
        /// Write log directly.
        /// </summary>
        /// <param name="message">The specified message to write.</param>
        public static void WriteDirect(string message)
        {
            WriteLog(message, Tw);
        }

        public static void CloseLog()
        {
            Tw.Close();
        }

        public static void ReopenLog()
        {
            Tw = TextWriter.Synchronized(new StreamWriter(LogFilePath, true));
        }

        private static void WriteLog(string message, TextWriter tw)
        {
            lock (SyncObject)
            {
                tw.WriteLine("[{2}] {0} {1} ", DateTime.UtcNow.ToLongTimeString(), DateTime.UtcNow.ToLongDateString(), Thread.CurrentThread.ManagedThreadId);
                tw.WriteLine(": {0}", message);
                tw.Flush();
            }
        }
    }
}
