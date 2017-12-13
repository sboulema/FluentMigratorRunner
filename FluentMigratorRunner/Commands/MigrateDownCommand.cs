using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows.Forms;
using EnvDTE;
using FluentMigratorRunner.Dialogs;
using FluentMigratorRunner.Helpers;
using FluentMigratorRunner.Models;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FluentMigratorRunner
{
    internal sealed class MigrateDownCommand
    {
        public const int CommandId = 0x0101;
        public static readonly Guid CommandSet = new Guid("ccb9de86-f9eb-4ca6-8f8d-6db0bb16bc28");
        private readonly Package package;

        private MigrateDownCommand(Package package)
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

        public static MigrateDownCommand Instance
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
            Instance = new MigrateDownCommand(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = ServiceProvider.GetService(typeof(DTE)) as DTE;

            using (var form = new MigrationsDialog(dte))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var selectedVersion = form.SelectedVersion;
                    ProjectHelper.Execute(ServiceProvider, TaskEnum.MigrateDown, selectedVersion);
                }
            }
        }
    }
}
