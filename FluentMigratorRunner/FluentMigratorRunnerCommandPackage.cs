﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace FluentMigratorRunner
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(FluentMigratorRunnerCommandPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class FluentMigratorRunnerCommandPackage : Package
    {
        public const string PackageGuidString = "ef89b5ed-1477-474a-b897-ce28eed6ddeb";

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
            MigrateUpCommand.Initialize(this);
            MigrateDownCommand.Initialize(this);
            RollbackCommand.Initialize(this);
            ListMigrationsCommand.Initialize(this);
            OptionsCommand.Initialize(this);
            base.Initialize();           
        }

        #endregion
    }
}
