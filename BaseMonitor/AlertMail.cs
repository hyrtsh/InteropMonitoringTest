using System.Text;
using BaseMonitor.Properties;
using Newtonsoft.Json;
using Utility;

namespace BaseMonitor
{
    public class AlertMail
    {
        /// <summary>
        /// Generate the alert mail
        /// </summary>
        /// <param name="rule">The specified MonitorRule instance.</param>
        /// <param name="result">The result of executing this monitor rule.</param>
        /// <returns>Generated alert mail content</returns>
        public static string GenerateAlertMail(MonitorRule rule, string result)
        {
            
            return Resources.AlertMailTemplate.Replace(ConstStrings.AlertContentPlaceHolder_RuleId, rule.RuleId)
                .Replace(ConstStrings.AlertContentPlaceHolder_DetailTable, GenerateDetailTable(rule, result));
        }

        private static string GenerateDetailTable(MonitorRule rule, string result)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<table>");
            sb.Append(string.Format("<tr><td><b>Rule</b></td><td>{0}</td></tr>", rule.RuleId));
            sb.Append(string.Format("<tr><td><b>Severity</b></td><td>{0}</td></tr>", rule.AlertSeverity));
            sb.Append(string.Format("<tr><td><b>Alert Condition</b></td><td>{0}</td></tr>", rule.Operation + rule.Threshold));
            sb.Append(string.Format("<tr><td><b>Actual Result</b></td><td>{0}</td></tr>", result));
            sb.Append(string.Format("<tr><td><b>Rule Detail</b></td><td>{0}</td></tr>", JsonConvert.SerializeObject(rule, Formatting.Indented).Replace("\r\n", "<br>")));
            sb.Append("</table>");

            return sb.ToString();
        }
    }
}
