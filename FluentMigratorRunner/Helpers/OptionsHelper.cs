using EnvDTE;
using FluentMigratorRunner.Models;
using Newtonsoft.Json;
using System.IO;

namespace FluentMigratorRunner.Helpers
{
    public static class OptionsHelper
    {
        public static DTE Dte;

        public static Options GetOptions()
        {
            var solutionFilePath = Dte.Solution.FileName;

            if (!File.Exists(solutionFilePath)) return new Options();

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            var settingFilePath = Path.Combine(solutionFolder, "FluentMigratorRunner.json");

            if (File.Exists(settingFilePath))
            {
                var json = File.ReadAllText(settingFilePath);
                return JsonConvert.DeserializeObject<Options>(json);
            }

            return new Options();
        }

        public static void SaveOptions(Options options)
        {
            var json = JsonConvert.SerializeObject(options);

            var solutionFilePath = Dte.Solution.FileName;

            if (!File.Exists(solutionFilePath)) return;

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            File.WriteAllText(Path.Combine(solutionFolder, "FluentMigratorRunner.json"), json);
        }
    }
}
