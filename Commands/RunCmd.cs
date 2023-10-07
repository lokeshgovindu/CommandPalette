using CommandPalette.View;
using EnvDTE;

namespace CommandPalette
{
    [Command(PackageIds.RunCmd)]
    internal sealed class RunCmd : BaseCommand<RunCmd>
    {
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            CPWindow commandPalette = new CPWindow();
            if ((bool)commandPalette.ShowDialog())
            {
                CommandPalettePackage.GetDTE().ExecuteCommand(commandPalette.SelectedVSCommand.Name);
            }
            return Task.CompletedTask;
        }
    }
}
