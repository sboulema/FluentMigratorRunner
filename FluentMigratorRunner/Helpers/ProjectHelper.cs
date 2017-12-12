using EnvDTE;
using FluentMigratorRunner.Models;
using System.IO;
using VSLangProj;
using Process = System.Diagnostics.Process;

namespace FluentMigratorRunner.Helpers
{
    public static class ProjectHelper
    {
        private static string GetProjectAssemblyPath(Project project)
        {
            if (project == null) return string.Empty;

            var fullPath = project.Properties.Item("FullPath").Value.ToString();
            var outputPath = project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
            var outputFileName = project.Properties.Item("AssemblyName").Value.ToString();

            return $"{Path.Combine(fullPath, outputPath, outputFileName)}.dll";
        }

        private static Project GetSelectedProject(DTE dte)
        {
            if (dte.ActiveWindow.Type == vsWindowType.vsWindowTypeSolutionExplorer)
            {
                return dte.ActiveSolutionProjects.GetValue(0);
            }

            return null;
        }

        private static string GetMigratePath(Project project)
        {
            var path = (project.Object as VSProject)?.References.Find("FluentMigrator")?.Path;
            if (string.IsNullOrEmpty(path)) return string.Empty;

            return Path.Combine(Path.GetDirectoryName(path), @"..\..\", @"tools\Migrate.exe");
        }

        private static string GetTask(TaskEnum task)
        {
            switch (task)
            {
                case TaskEnum.MigrateUp:
                    return "migrate:up";
                case TaskEnum.MigrateDown:
                    return "migrate:down";
                case TaskEnum.Rollback:
                    return "rollback";
                case TaskEnum.ListMigrations:
                    return "listmigrations";
                default:
                    return string.Empty;
            }
        }

        public static void Execute(DTE dte, DbEnum db, string connection, TaskEnum task)
        {
            var project = GetSelectedProject(dte);

            if (project == null) return;

            var assembly = GetProjectAssemblyPath(project);
            var migrate = GetMigratePath(project);
            var taskArgument = GetTask(task);

            Process.Start("cmd.exe", $@"/k """"{migrate}"" -c=""{connection}"" -db={db} --task:{taskArgument} -target=""{assembly}""");
        }
    }
}
