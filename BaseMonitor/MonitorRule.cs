namespace BaseMonitor
{
    public class MonitorRule
    {
        /// <summary>
        /// Rule unique identifier
        /// </summary>
        public string RuleId { get; set; }

        /// <summary>
        /// Query sentence
        /// </summary>
        public string AlertQuery { get; set; }

        /// <summary>
        /// Rule is Enable
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// Connect data string 
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// Alert severity
        /// </summary>
        public int AlertSeverity { get; set; }

        /// <summary>
        /// The Rule Owner
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Email notification address
        /// </summary>
        public string EmailNotificationAddress { get; set; }

        /// <summary>
        /// Alert suppression Window in minutes
        /// </summary>
        public int AlertSuppressionWindowInMinutes { get; set; }

        /// <summary>
        /// Number threshold
        /// </summary>
        public double Threshold { get; set; }

        /// <summary>
        /// How to compare with threshold
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Custom parameter use in the query assembly
        /// </summary>
        public string CustomParameter { get; set; }

        /// <summary>
        /// Rule Unique Identity
        /// </summary>
        public string RuleUniqueIdentity { get; set; }
    }
}
