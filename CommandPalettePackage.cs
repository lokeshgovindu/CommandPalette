global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using CommandPalette.View;
using EnvDTE80;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;

namespace CommandPalette
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.CommandPaletteString)]
    public sealed class CommandPalettePackage : ToolkitPackage
    {
        private static DTE2 _dte;
        private static IAsyncServiceProvider _serviceProvider;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await this.RegisterCommandsAsync();

            _serviceProvider = this;
            var dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE));
            if (dte == null)
                throw new Exception("Not able to retrieve SDTE");

            _dte = (DTE2)dte;
            if (_dte == null)
            {
                throw new Exception("Not able to retrieve DTE2");
            }

            // Add our command handlers for menu (commands must exist in the .vsct file)
            //OleMenuCommandService mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            //if (null != mcs)
            //{
            //    // Create the command for the menu item.
            //    CommandID menuCommandID = new CommandID(PackageGuids.CommandPalette, PackageIds.RunCmd);
            //    MenuCommand menuItem = new MenuCommand(CommandPaletteClick, menuCommandID);
            //    mcs.AddCommand(menuItem);
            //}
        }

        private void CommandPaletteClick(object sender, EventArgs e)
        {
            CPWindow commandPalette = new CPWindow();
            if ((bool)commandPalette.ShowDialog())
            {
                //await VS.MessageBox.ShowAsync(commandPalette.SelectedVSCommand.ToString());
                _dte.ExecuteCommand(commandPalette.SelectedVSCommand.Name);
            }
        }
        internal static DTE2 GetDTE()
        {
            return _dte;
        }
    }
}
