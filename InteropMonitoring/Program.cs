using System;
using System.IO;
using Utility;

namespace InteropMonitoring
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MonitorManager mm = new MonitorManager();
                mm.TriggerMointors();
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
            finally
            {
                ExecutionInfoStore.SerializeStore();

                if (Log.ErrorCount > 0)
                {
                    EmailHelper.SendEmail(string.Format("InteropMonitoring project throws {0} exception{1}.",
                        Log.ErrorCount,
                        Log.ErrorCount > 1 ? "s" : string.Empty),
                        string.Format("Log folder: {0}", Log.LogDirectory),
                        string.Empty,
                        string.Empty,
                        Log.LogFilePath);
                }
                else
                {
                    EmailHelper.SendEmail("InteropMonitoring project works fine.",
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        Log.LogFilePath);
                }
            }

        }
    }
}
