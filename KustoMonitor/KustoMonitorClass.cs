using System;
using System.Data;
using System.IO;
using System.Reflection;
using BaseMonitor;
using Kusto.Data.Net.Client;
using Utility;

namespace KustoMonitor
{
    public class KustoMonitorClass : BaseMonitorClass
    {
        /// <summary>
        /// Initialize KustoMonitorClass.
        /// </summary>
        public KustoMonitorClass()
        {
            this.RulesFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConstStrings.RulesFolderName);
        }

        /// <summary>
        /// After querying, if we need alert, return true, otherwise return false
        /// </summary>
        /// <param name="rule">The specified MonitorRule instance.</param>
        /// <returns>If need alert, return true, otherwise return false.</returns>
        public override bool ExecuteRule(MonitorRule rule, out string result)
        {
            object value = null;
            result = string.Empty;
            try
            {
                //Then there is the code to execute the Kusto query based on rule
                var client = KustoClientFactory.CreateCslQueryProvider(rule.DataSource);
                IDataReader reader = client.ExecuteQuery(rule.AlertQuery);
                while (reader.Read())
                {
                    value = reader.GetValue(0);
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("{0} throw exception : {1}", rule.RuleUniqueIdentity, ex.ToString());
                return false;
            }

            result = value.ToString();
            Double.TryParse(result, out double returnObj);
            return CommonHelper.CheckResult(rule.Operation, returnObj, rule.Threshold, rule.RuleUniqueIdentity);
        }
    }
}
