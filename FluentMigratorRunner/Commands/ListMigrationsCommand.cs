using System;
using System.ComponentModel.Design;
using EnvDTE;
using FluentMigratorRunner.Helpers;
using FluentMigratorRunner.Models;
using Microsoft.VisualStudio.Shell;

namespace FluentMigratorRunner
{
    internal sealed class ListMigrationsCommand
    {
        public const int CommandId = 0x0103;
        public static readonly Guid CommandSet = new Guid("ccb9de86-f9eb-4ca6-8f8d-6db0bb16bc28");
        private readonly Package package;

        private ListMigrationsCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static ListMigrationsCommand Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static void Initialize(Package package)
        {
            Instance = new ListMigrationsCommand(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = ServiceProvider.GetService(typeof(DTE)) as DTE;
            OptionsHelper.Dte = dte;
            var options = OptionsHelper.GetOptions();

            ProjectHelper.Execute(dte, options.DbType, options.Connection, TaskEnum.ListMigrations);
        }
    }
}
