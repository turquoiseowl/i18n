using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using i18n.Domain.Concrete;

namespace VSPackage.i18n_POTGenerator
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidi18n_POTGeneratorPkgString)]
    public sealed class i18n_POTGeneratorPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public i18n_POTGeneratorPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                var menuCommandID = new CommandID(GuidList.guidi18n_POTGeneratorCmdSet, (int)PkgCmdIDList.cmdidGeneratePOTFile);
                var menuItem = new MenuCommand(_menuItemCallback, menuCommandID);
                //var menuItem = new OleMenuCommand(_menuItemCallback, _menuItemChange, _menuItemQueryStatus, menuCommandID);
                mcs.AddCommand( menuItem );
            }
        }

        private object _getCurrentProject()
        {
            var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));

            var activeProjects = dte.ActiveSolutionProjects as object[];

            if (activeProjects != null)
                return activeProjects[0];
            else
                return null;
        }

        private void _menuItemQueryStatus(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            if (command != null)
            {
                var currentProject = _getCurrentProject();
                command.Visible = (currentProject!=null);
            }
        }

        private void _menuItemChange(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            if (command != null)
            {
                var currentProject = _getCurrentProject();
                command.Visible = (currentProject != null);
            }
        }

        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void _menuItemCallback(object sender, EventArgs e)
        {
            dynamic currentProject = _getCurrentProject();
            
            if (currentProject != null)
            {
                var dir = new DirectoryInfo((string) currentProject.FullName).Parent;

                // ReSharper disable PossibleNullReferenceException
                var configPath = dir.FullName + "\\web.config";
                // ReSharper restore PossibleNullReferenceException. Can't be null as is being picked from project
                if (!File.Exists(configPath))
                {
                    throw new Exception("Project does not have a web.config file");
                }

                var settings = new i18nSettings(new WebConfigSettingService(configPath));
                var rep = new POTranslationRepository(settings);

                var nugget = new FileNuggetFinder(settings);
                var items = nugget.ParseAll();
                rep.SaveTemplate(items);

                var ts = new TranslationMerger(rep);
                ts.MergeAllTranslation(items);
                var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                var clsid = Guid.Empty;
                int result;
                ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                           0,
                           ref clsid,
                           "i18n.POTGenerator",
                           string.Format(CultureInfo.CurrentCulture, "POT File generated correctly"),
                           string.Empty,
                           0,
                           OLEMSGBUTTON.OLEMSGBUTTON_OK,
                           OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                           OLEMSGICON.OLEMSGICON_INFO,
                           0,        // false
                           out result));

            }
        }
    }
}
