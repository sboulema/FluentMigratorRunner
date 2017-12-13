using EnvDTE;
using FluentMigratorRunner.Models;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace FluentMigratorRunner.Helpers
{
    /// <summary>
    /// Store solution specific options in a json file in the solution .vs folder
    /// </summary>
    /// <remarks>
    /// Remember to set the public Dte property before use
    /// </remarks>
    public static class OptionsHelper
    {
        public static DTE Dte;
        private const string DotVsFolder = ".vs";

        /// <summary>
        /// Load options from file
        /// </summary>
        /// <returns></returns>
        public static Options GetOptions()
        {
            var solutionFilePath = Dte.Solution.FileName;

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
        /// <param name="options"></param>
        public static void SaveOptions(Options options)
        {
            var json = JsonConvert.SerializeObject(options);

            var solutionFilePath = Dte.Solution.FileName;

            if (!File.Exists(solutionFilePath)) return;

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            var settingFileName = Assembly.GetExecutingAssembly().GetName().Name;
            File.WriteAllText(Path.Combine(solutionFolder, DotVsFolder, $"{settingFileName}.json"), json);
        }
    }
}
