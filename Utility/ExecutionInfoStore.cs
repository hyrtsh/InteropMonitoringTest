using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace Utility
{
    public class ExecutionInfoStore
    {
        /// <summary>
        /// Used to store the information related with rule execution
        /// </summary>
        private static ConcurrentDictionary<string, StoreItem> ruleExecuteInfoDic;
        private static string executionInfoFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RuleExecuteInfo\store.json");
        private static ReaderWriterLock rwl = new ReaderWriterLock();

        /// <summary>
        /// Get the rule's last query time
        /// </summary>
        /// <param name="ruleId">the rule id</param>
        /// <returns>Last query time</returns>
        public static DateTime GetLastQueryTime(string ruleId)
        {
            DeserializeStore();

            if (ruleExecuteInfoDic.ContainsKey(ruleId))
            {
                return ruleExecuteInfoDic[ruleId].LastQueryTime;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Get the rule's last query time
        /// </summary>
        /// <param name="ruleId">Rule id</param>
        /// <param name="time">Last query time</param>
        public static void SetLastQueryTime(string ruleId, DateTime time)
        {
            DeserializeStore();
            if (ruleExecuteInfoDic.ContainsKey(ruleId))
            {
                ruleExecuteInfoDic[ruleId].LastQueryTime = time;
            }
            else
            {
                StoreItem item = new StoreItem(ruleId);
                item.LastQueryTime = time;
                ruleExecuteInfoDic.TryAdd(ruleId, item);
            }
        }

        /// <summary>
        /// Get rule's last action time
        /// </summary>
        /// <param name="ruleId">Rule id</param>
        /// <returns>Last action time</returns>
        public static DateTime GetLastActionTime(string ruleId)
        {
            DeserializeStore();
            if (ruleExecuteInfoDic.ContainsKey(ruleId))
            {
                return ruleExecuteInfoDic[ruleId].LastActionTime;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Set the rule's last action time
        /// </summary>
        /// <param name="ruleId">Rule id</param>
        /// <param name="time">Last action time</param>
        public static void SetLastActionTime(string ruleId, DateTime time)
        {
            DeserializeStore();
            if (ruleExecuteInfoDic.ContainsKey(ruleId))
            {
                ruleExecuteInfoDic[ruleId].LastActionTime = time;
            }
            else
            {
                StoreItem item = new StoreItem(ruleId);
                item.LastActionTime = time;
                ruleExecuteInfoDic.TryAdd(ruleId, item);
            }
        }

        private static void DeserializeStore()
        {
            if (ruleExecuteInfoDic == null)
            {
                if (File.Exists(executionInfoFile))
                {
                    try
                    {
                        rwl.AcquireWriterLock(5000);
                        ruleExecuteInfoDic = new ConcurrentDictionary<string, StoreItem>(JsonConvert.DeserializeObject<List<StoreItem>>(File.ReadAllText(executionInfoFile)).ToDictionary(x => x.RuleId, x => x));
                    }
                    catch (Exception ex)
                    {
                        Log.WriteErrorLog("{0} cannot be deserialized. {1}", executionInfoFile, ex.ToString());
                    }
                    finally
                    {
                        rwl.ReleaseWriterLock();
                    }
                }
                else
                {
                    ruleExecuteInfoDic = new ConcurrentDictionary<string, StoreItem>();
                }
            }
        }

        /// <summary>
        /// Serialize the execute information for next run
        /// </summary>
        public static void SerializeStore()
        {
            if (ruleExecuteInfoDic != null)
            {
                try
                {
                    string content = JsonConvert.SerializeObject(ruleExecuteInfoDic.Values.ToList());
                    FileInfo info = new FileInfo(executionInfoFile);
                    if (!info.Directory.Exists)
                    {
                        info.Directory.Create();
                    }

                    rwl.AcquireWriterLock(5000);
                    File.WriteAllText(executionInfoFile, content);
                }
                catch (Exception ex)
                {
                    Log.WriteErrorLog("{0}: Write store occur a error. {1}", executionInfoFile, ex.Message);
                }
                finally
                {
                    rwl.ReleaseWriterLock();
                }
            }
        }
    }

    /// <summary>
    /// The class to store a rule execution information
    /// </summary>
    public class StoreItem
    {
        /// <summary>
        /// Rule Id
        /// </summary>
        public string RuleId { get; set; }

        /// <summary>
        /// The last query time
        /// </summary>
        public DateTime LastQueryTime { get; set; }

        /// <summary>
        /// The last action time
        /// </summary>
        public DateTime LastActionTime { get; set; }

        /// <summary>
        /// Initialize a store item
        /// </summary>
        /// <param name="ruleId">The rule id</param>
        public StoreItem(string ruleId)
        {
            this.RuleId = ruleId;
            LastQueryTime = DateTime.MinValue;
            LastActionTime = DateTime.MaxValue;
        }
    }
}
