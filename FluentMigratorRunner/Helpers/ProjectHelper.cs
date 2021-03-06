﻿using EnvDTE;
using FluentMigratorRunner.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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

        /// <summary>
        /// Fluent Migrator menu should only be visible if we can find a reference 
        /// to the Fluent Migrator NuGet package
        /// </summary>
        /// <param name="dte"></param>
        /// <returns></returns>
        public static bool ShouldMenuBeVisible(DTE dte) =>
            HasFluentMigratorReference(GetSelectedProject(dte));

        /// <summary>
        /// Given a project find a reference to the FluentMigrator Nuget package 
        /// and create the the path to the include Migrate executable
        /// </summary>
        /// <param name="project">A DTE project</param>
        /// <returns></returns>
        public static string GetMigratePath(Project project)
        {
            var path = GetMigratePathForFrameWorkProject(project);

            if (!string.IsNullOrEmpty(path)) return path;

            path = GetMigratePathForCoreProject(project);

            return path;
        }

        private static string GetMigratePathForFrameWorkProject(Project project)
        {
            var path = (project.Object as VSProject)?.References.Find("FluentMigrator")?.Path;
            if (string.IsNullOrEmpty(path)) return string.Empty;

            return Path.Combine(Path.GetDirectoryName(path), @"..\..\", @"tools\Migrate.exe");
        }

        private static string GetMigratePathForCoreProject(Project project)
        {
            var nugetFile = Path.Combine(Path.GetDirectoryName(project.FileName), "obj", 
                Path.GetFileNameWithoutExtension(project.FileName) + ".csproj.nuget.g.props");
            var nugetPackageRoot = XDocument.Load(nugetFile).Root.DescendantNodes().OfType<XElement>()
                .First(x => x.Name.LocalName.Equals("NuGetPackageFolders"))
                .Value.Split(';').FirstOrDefault();
            if (string.IsNullOrEmpty(nugetPackageRoot)) return string.Empty;

            var fluentMigratorVersion = GetFluentMigratorReferenceForCoreProject(project).Attribute("Version").Value;

            return Path.Combine(nugetPackageRoot, "FluentMigrator", fluentMigratorVersion, @"tools\Migrate.exe");
        }

        private static XElement GetFluentMigratorReferenceForCoreProject(Project project) =>
            XDocument.Load(project.FileName).Root.DescendantNodes().OfType<XElement>()
                .FirstOrDefault(x => x.Name.LocalName.Equals("PackageReference") &&
                                     x.Attribute("Include").Value.Equals("FluentMigrator"));

        /// <summary>
        /// Check if the given project either has a reference or a dependency on the NuGet FluentMigrator package
        /// </summary>
        /// <param name="project">A DTE project</param>
        /// <returns></returns>
        private static bool HasFluentMigratorReference(Project project)
        {
            var hasReference = (project.Object as VSProject)?.References.Find("FluentMigrator") != null;

            if (hasReference) return hasReference;

            hasReference = XDocument.Load(project.FileName).Root.DescendantNodes().OfType<XElement>()
                .Any(x => x.Name.LocalName.Equals("PackageReference") &&
                          x.Attribute("Include").Value.Equals("FluentMigrator"));

            return hasReference;
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
                    return "rollback:toversion";
                case TaskEnum.ListMigrations:
                    return "listmigrations";
                default:
                    return string.Empty;
            }
        }

        private static string GetVersion(string version) => 
            string.IsNullOrEmpty(version) ? string.Empty : $"--version={version}";

        private static void BuildProject(DTE dte, Project project)
        {
            var solutionBuild = dte.Solution.SolutionBuild;
            solutionBuild.BuildProject(solutionBuild.ActiveConfiguration.Name, project.UniqueName, true);
        }

        public static void Execute(IServiceProvider serviceProvider, TaskEnum task, string version = "")
        {
            var dte = serviceProvider.GetService(typeof(DTE)) as DTE;
            var options = OptionsHelper.GetOptions(dte);

            Execute(dte, options.DbType, options.Connection, task, version);
        }

        private static void Execute(DTE dte, DbEnum db, string connection, TaskEnum task, string version)
        {
            var project = GetSelectedProject(dte);

            if (project == null) return;

            BuildProject(dte, project);

            var assembly = GetProjectAssemblyPath(project);
            var migrate = GetMigratePath(project);
            var taskArgument = GetTask(task);
            var versionArgument = GetVersion(version);

            Process.Start("cmd.exe", $@"/k """"{migrate}"" -c=""{connection}"" -db={db} --task:{taskArgument} {versionArgument} -target=""{assembly}""");
        }

        public static List<string> GetMigrations(DTE dte)
        {
            var migrations = new List<string>();

            var options = OptionsHelper.GetOptions(dte);

            var project = GetSelectedProject(dte);

            if (project == null) return migrations;

            BuildProject(dte, project);

            var assembly = GetProjectAssemblyPath(project);
            var migrate = GetMigratePath(project);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $@"/k """"{migrate}"" -c=""{options.Connection}"" -db={options.DbType} --task:listmigrations -target=""{assembly}""",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();

            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();

                if (string.IsNullOrEmpty(line)) break;

                var match = Regex.Match(line, @"\[\+\] (\d*):");
                if (match.Success)
                {
                    migrations.Add(match.Groups[1].Value);
                }
            }

            return migrations;
        }
    }
}
