//using CommandPalette.View;
//using EnvDTE;
//using EnvDTE80;
//using Microsoft.VisualStudio.Shell.Interop;

//namespace CommandPalette
//{
//    [Command(PackageIds.MyCommand)]
//    internal sealed class MyCommand : BaseCommand<MyCommand>
//    {
//        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
//        {
//            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
//            DTE2 _dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE2;
//            CPWindow commandPalette = new CPWindow();
//            if ((bool)commandPalette.ShowDialog())
//            {
//                _dte.ExecuteCommand(commandPalette.SelectedVSCommand.Name);
//            }
//        }
//    }
//}
