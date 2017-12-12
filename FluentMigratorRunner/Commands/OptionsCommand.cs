using System;
using System.ComponentModel.Design;
using EnvDTE;
using FluentMigratorRunner.Dialogs;
using Microsoft.VisualStudio.Shell;

namespace FluentMigratorRunner
{
    internal sealed class OptionsCommand
    {
        public const int CommandId = 0x0104;
        public static readonly Guid CommandSet = new Guid("ccb9de86-f9eb-4ca6-8f8d-6db0bb16bc28");
        private readonly Package package;

        private OptionsCommand(Package package)
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

        public static OptionsCommand Instance
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
            Instance = new OptionsCommand(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = ServiceProvider.GetService(typeof(DTE)) as DTE;
            new OptionsDialog(dte).ShowDialog();
        }
    }
}
