using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using EnvDTE;
using FluentMigratorRunner.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FluentMigratorRunner
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class FluentMigratorRunnerCommandPackage : Package
    {
        public const string PackageGuidString = "ef89b5ed-1477-474a-b897-ce28eed6ddeb";
        public readonly Guid FluentMigratorRunnerCommandPackageCmdSetGuidString = new Guid("ccb9de86-f9eb-4ca6-8f8d-6db0bb16bc28");

        public FluentMigratorRunnerCommandPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            // Initialize the Fluent Migrator Menu, should only be visible for projects with FluentMigrator reference
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            var menuCommandId = new CommandID(FluentMigratorRunnerCommandPackageCmdSetGuidString, 0x1010);
            var menuItem = new OleMenuCommand(null, menuCommandId);
            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
            mcs.AddCommand(menuItem);

            MigrateUpCommand.Initialize(this);
            MigrateDownCommand.Initialize(this);
            RollbackCommand.Initialize(this);
            ListMigrationsCommand.Initialize(this);
            OptionsCommand.Initialize(this);
            base.Initialize();           
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e) =>
            ((OleMenuCommand)sender).Visible = ProjectHelper.ShouldMenuBeVisible(GetService(typeof(DTE)) as DTE);

        #endregion
    }
}
