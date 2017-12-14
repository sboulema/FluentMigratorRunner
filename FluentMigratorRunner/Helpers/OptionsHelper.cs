using EnvDTE;
using FluentMigratorRunner.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FluentMigratorRunner.Helpers
{
    /// <summary>
    /// Store solution specific options in a json file in the solution .vs folder
    /// </summary>
    public static class OptionsHelper
    {
        private const string DotVsFolder = ".vs";

        /// <summary>
        /// Load options from file
        /// </summary>
        /// <param name="dte"></param>
        /// <returns>Options object</returns>
        public static Options GetOptions(DTE dte)
        {
            var solutionFilePath = dte.Solution.FileName;

            if (!File.Exists(solutionFilePath)) return new Options();

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            var settingFileName = Assembly.GetExecutingAssembly().GetName().Name;
            var settingFilePath = Path.Combine(solutionFolder, DotVsFolder, $"{settingFileName}.json");

            if (File.Exists(settingFilePath))
            {
                var json = File.ReadAllText(settingFilePath);
                return JsonConvert.DeserializeObject<Options>(json);
            }

            return new Options();
        }

        /// <summary>
        /// Save options to file
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="options"></param>
        public static void SaveOptions(DTE dte, Options options)
        {
            var json = JsonConvert.SerializeObject(options);

            var solutionFilePath = dte.Solution.FileName;

            if (!File.Exists(solutionFilePath)) return;

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            var settingFileName = Assembly.GetExecutingAssembly().GetName().Name;
            File.WriteAllText(Path.Combine(solutionFolder, DotVsFolder, $"{settingFileName}.json"), json);
        }

        /// <summary>
        /// Check if all options are set, eg. not empty, null or default
        /// </summary>
        /// <param name="dte"></param>
        /// <returns>True if all options are set</returns>
        public static bool AllOptionsAreSet(DTE dte)
        {
            var options = GetOptions(dte);

            return options.GetType()
                 .GetProperties()
                 .Select(pi => pi.GetValue(options))
                 .Any(value => value is string 
                    ? !string.IsNullOrEmpty(value as string) 
                    : !value.Equals(GetDefault(value.GetType())));
        }

        private static object GetDefault(this Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
    }
}
