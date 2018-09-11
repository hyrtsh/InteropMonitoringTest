using System;
using Utility;

namespace BaseMonitor
{
    /// <summary>
    /// Class used to pre-process the monitor rule.
    /// </summary>
    public static class RulePreProcess
    {
        /// <summary>
        /// Initialize the RuleUniqueIdentity property of the monitor rule.
        /// </summary>
        /// <param name="tempRule">The specified monitor rule.</param>
        /// <param name="fileName">The file name of this monitor rule.</param>
        /// <returns>The MonitorRule with the RuleUniqueIdentity value.</returns>
        public static MonitorRule AutoMakeRuleUniqueIdentity(MonitorRule tempRule, string fileName)
        {
            string fileLocation = fileName.Split(new string[] { ConstStrings.MonitoringDllFolderName + "\\" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("\\", "_") + "_";
            tempRule.RuleUniqueIdentity = fileLocation + tempRule.RuleId;
            return tempRule;
        }

        /// <summary>
        /// Replace AtLastQueryTime using the actual last query time in AlertQuery property.
        /// </summary>
        /// <param name="tempRule">The specified monitor rule.</param>
        /// <returns>The MonitorRule with the updated AlertQuery value.</returns>
        public static MonitorRule ReplaceAtLastQueryTime(MonitorRule tempRule)
        {
            tempRule.AlertQuery = tempRule.AlertQuery.Replace("@LastQueryTime", ExecutionInfoStore.GetLastQueryTime(tempRule.RuleUniqueIdentity).ToString());
            return tempRule;
        }
    }
}
