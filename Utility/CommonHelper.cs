namespace Utility
{
    /// <summary>
    /// Reuse method helper
    /// </summary>
    public static class CommonHelper
    {
        /// <summary>
        /// Common method to compare actual result with threshold using specified operation.
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="actualResult">The specified actual result.</param>
        /// <param name="threshold">The threshold.</param>
        /// <param name="ruleUniqueIdentity">The unique identity of rule.</param>
        /// <returns>Return the result of comparing actual result with threshold using specified operation.</returns>
        public static bool CheckResult(string operation, double actualResult, double threshold, string ruleUniqueIdentity)
        {
            switch (operation)
            {
                case ">":
                    return actualResult > threshold;
                case "<":
                    return actualResult < threshold;
                case "=":
                    return actualResult == threshold;
                case ">=":
                    return actualResult >= threshold;
                case "<=":
                    return actualResult <= threshold;
                case "!=":
                    return actualResult != threshold;
                default:
                    Log.WriteErrorLog("{0}: Only support following operations: >, <, =, >=, <=, !=.", ruleUniqueIdentity);
                    return false;
            }
        }
    }
}
