using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using Utility;

namespace BaseMonitor
{
    /// <summary>
    /// Base monitor class
    /// </summary>
    public abstract class BaseMonitorClass
    {
        /// <summary>
        /// After querying, if we need alert, return true, otherwise return false
        /// </summary>
        /// <param name="rule">The specified MonitorRule instance.</param>
        /// <returns>If need alert, return true, otherwise return false.</returns>
        public abstract bool ExecuteRule(MonitorRule rule, out string result);

        /// <summary>
        /// The folder to store the rule files.
        /// </summary>
        public string RulesFolder { get; set; }

        /// <summary>
        /// Load all MonitorRules from RulesFolder.
        /// </summary>
        /// <returns>A list of MonitorRule instances.</returns>
        public virtual List<MonitorRule> LoadRules()
        {
            if (string.IsNullOrWhiteSpace(RulesFolder))
            {
                throw new Exception("RulesFolder is null or empty.");
            }

            string[] ruleFiles = Directory.GetFiles(RulesFolder);
            List<MonitorRule> monitorRules = new List<MonitorRule>();
            foreach (string fileName in ruleFiles)
            {
                try
                {
                    string fileContent = File.ReadAllText(fileName);
                    List<MonitorRule> tempRules = JsonConvert.DeserializeObject<List<MonitorRule>>(fileContent);

                    tempRules.ForEach(x =>
                    {
                        x = RulePreProcess.AutoMakeRuleUniqueIdentity(x, fileName);
                        x = RulePreProcess.ReplaceAtLastQueryTime(x);

                        if (tempRules.FindAll(y => y.RuleId == x.RuleId).Count > 1)
                        {
                            Log.WriteErrorLog("RuleId {0} is duplicated in {1}.", x.RuleId, fileName);
                        }
                    });

                    monitorRules.AddRange(tempRules);
                }
                catch (Exception ex)
                {
                    Log.WriteErrorLog("{0} parse wrong, exception: {1}", fileName, ex.ToString());
                }
            }

            return monitorRules;
        }

        /// <summary>
        /// Execute all Rules and send mail if need alert.
        /// </summary>
        public void ExecuteRules()
        {
            List<MonitorRule> rules = LoadRules();
            foreach (MonitorRule rule in rules)
            {
                try
                {
                    if (rule.IsEnable)
                    {
                        DateTime lastQueryTime = DateTime.UtcNow;

                        Log.WriteInfoLog("Rule: {0} start to execute", rule.RuleUniqueIdentity);
                        var needAlert = ExecuteRule(rule, out string result);

                        // If the result is empty, the ExecuteRule throws exception. The error log is written in ExecuteRule method.
                        if (string.IsNullOrWhiteSpace(result))
                        {
                            continue;
                        }

                        Log.WriteInfoLog("Rule: {0} execute done, result is {1}", rule.RuleUniqueIdentity, result);
                        if (needAlert)
                        {
                            Log.WriteInfoLog("Rule: {0} meet the condition.", rule.RuleUniqueIdentity);
                            TakeAlertAction(rule, result);
                        }
                        else
                        {
                            Log.WriteInfoLog("Rule: {0} don't meet the condition and no need alert.", rule.RuleUniqueIdentity);
                        }

                        ExecutionInfoStore.SetLastQueryTime(rule.RuleUniqueIdentity, lastQueryTime);
                    }
                    else
                    {
                        Log.WriteInfoLog("Rule: {0} is disabled.", rule.RuleUniqueIdentity);
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteErrorLog("{0} throw exception: {1}", rule.RuleUniqueIdentity, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Take Alert action: Send Email
        /// </summary>
        /// <param name="rule">The specified MonitorRule instance.</param>
        /// <param name="result">The value returned by ExcuteRule method.</param>
        public virtual void TakeAlertAction(MonitorRule rule, string result)
        {
            string subject = string.Format("Interop Monitoring Alert Fired:{0}", rule.RuleId);

            //Write the common method to fire the alert action, and this can also be overwritten by derived classed
            if ((DateTime.UtcNow - ExecutionInfoStore.GetLastActionTime(rule.RuleUniqueIdentity)).TotalMinutes > rule.AlertSuppressionWindowInMinutes)
            {
                Log.WriteInfoLog("Rule: {0} compare with last alert time and need alert, start to send alert mail.", rule.RuleUniqueIdentity);
                string ccEmail = ConfigurationManager.AppSettings[ConstStrings.ActionEmailServerity + rule.AlertSeverity.ToString()];
                var body = AlertMail.GenerateAlertMail(rule, result);
                EmailHelper.SendEmail(subject, body, rule.EmailNotificationAddress, ccEmail);
                ExecutionInfoStore.SetLastActionTime(rule.RuleUniqueIdentity, DateTime.UtcNow);
                Log.WriteInfoLog("Rule: {0} send rule alert mail done", rule.RuleUniqueIdentity);
            }
            else
            {
                Log.WriteInfoLog("Rule: {0} compare with last alert time and no need to alert", rule.RuleUniqueIdentity);
            }
        }
    }
}
