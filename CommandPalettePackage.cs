global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using CommandPalette.View;
using EnvDTE80;
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
        DTE2 _dte;
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await this.RegisterCommandsAsync();

            _dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE2;

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(PackageGuids.CommandPalette, PackageIds.MyCommand);
                MenuCommand menuItem = new MenuCommand(CommandPaletteClick, menuCommandID);
                mcs.AddCommand(menuItem);
            }
        }

        private async void CommandPaletteClick(object sender, EventArgs e)
        {
            CPWindow commandPalette = new CPWindow();
            if ((bool)commandPalette.ShowDialog())
            {
                //await VS.MessageBox.ShowAsync(commandPalette.SelectedVSCommand.ToString());
                _dte.ExecuteCommand(commandPalette.SelectedVSCommand.Name);
            }
        }
    }
}