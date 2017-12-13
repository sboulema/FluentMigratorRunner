using System;
using System.ComponentModel.Design;
using FluentMigratorRunner.Helpers;
using FluentMigratorRunner.Models;
using Microsoft.VisualStudio.Shell;

namespace FluentMigratorRunner
{
    internal sealed class MigrateUpCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("ccb9de86-f9eb-4ca6-8f8d-6db0bb16bc28");
        private readonly Package package;

        private MigrateUpCommand(Package package)
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

        public static MigrateUpCommand Instance
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
            Instance = new MigrateUpCommand(package);
        }

        private void MenuItemCallback(object sender, EventArgs e) => 
            ProjectHelper.Execute(ServiceProvider, TaskEnum.MigrateUp);
    }
}
