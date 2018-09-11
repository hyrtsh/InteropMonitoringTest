using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Utility;

namespace InteropMonitoring
{
    public class MonitorManager
    {
        /// <summary>
        /// Load DLL in specific folder, load up to two layers of folders
        /// </summary>
        /// <param name="folderPath"></param>
        private void LoadAssemblys(string folderPath)
        {
            var directories = Directory.GetDirectories(folderPath);

            foreach (var directory in directories)
            {
                var assemblys = Directory.GetFiles(directory, "*.dll");

                foreach (var assembly in assemblys)
                {
                    Assembly.LoadFile(assembly);
                }

                var secondDirectories = Directory.GetDirectories(directory);

                foreach (var secondDirectory in secondDirectories)
                {
                    var secondAssemblys = Directory.GetFiles(secondDirectory, "*.dll");

                    foreach (var secondAssembly in secondAssemblys)
                    {
                        Assembly.LoadFile(secondAssembly);
                    }
                }
            }
        }

        /// <summary>
        /// find and return DLL dependence when can't load dependence
        /// </summary>
        /// <param name="sender">itself</param>
        /// <param name="args">event parameter</param>
        /// <returns>Dependence DLL</returns>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Split(',')[0] == args.Name.Split(',')[0]).OrderByDescending(x => x.GetName().Version).FirstOrDefault();

            if (assembly != null)
            {
                return assembly;
            }
            else
            {
                var errorMessage = string.Format("{0} assembly can't load", args.Name);
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Create its sub-class instances and invoke ExceuteRules in BaseMonitorClass with the created instance.
        /// </summary>
        public void TriggerMointors()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            LoadAssemblys(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstStrings.MonitoringDllFolderName));

            Assembly baseMonitorAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name == ConstStrings.BaseMonitorDllName).First();
            Type baseMonitorClass = baseMonitorAssembly.DefinedTypes.Where(x => x.Name.StartsWith(ConstStrings.BaseMonitorDllName)).First();
            MethodInfo executeRulesMethod = baseMonitorClass.GetMethod(ConstStrings.ExecuteRulesMethod);

            List<Task> tasks = new List<Task>();
            int.TryParse(ConfigurationManager.AppSettings[ConstStrings.TimeOutKey], out int timeOut);
            CancellationTokenSource cts = new CancellationTokenSource(timeOut * 1000);

            foreach (string dllName in ConfigurationManager.AppSettings.AllKeys.Where(x => x.Contains(ConstStrings.DllNameKeyword)))
            {
                Task task = Task.Run(() => ExecuteAssemblys(ConfigurationManager.AppSettings[dllName], executeRulesMethod), cts.Token);
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Execute assemblies
        /// </summary>
        /// <param name="dllName">The specified Dll name to execute.</param>
        /// <param name="executeRulesMethod">The MethodInfo instance to invoke.</param>
        private static void ExecuteAssemblys(string dllName, MethodInfo executeRulesMethod)
        {
            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(x => x.GetName().Name == dllName);
                Type assemblyType = assembly.DefinedTypes.Where(x => x.Name.StartsWith(dllName)).First();
                object assemblyObject = Activator.CreateInstance(assemblyType);
                var returnObj = executeRulesMethod.Invoke(assemblyObject, null);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("{0} throws exception: {1}", dllName, ex.ToString());
            }
        }
    }
}
