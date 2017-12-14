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
        private readonly Package _package;
        private readonly DTE _dte;

        private ListMigrationsCommand(Package package)
        {
            _package = package ?? throw new ArgumentNullException("package");
            _dte = ServiceProvider.GetService(typeof(DTE)) as DTE;

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
                commandService.AddCommand(menuItem);
            }
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e) => 
            ((OleMenuCommand)sender).Enabled = OptionsHelper.AllOptionsAreSet(_dte);

        public static ListMigrationsCommand Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this._package;
            }
        }

        public static void Initialize(Package package)
        {
            Instance = new ListMigrationsCommand(package);
        }

        private void MenuItemCallback(object sender, EventArgs e) => 
            ProjectHelper.Execute(ServiceProvider, TaskEnum.ListMigrations);
    }
}
