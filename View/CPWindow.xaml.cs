using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using SearchingKeyboardShortcut;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CommandPalette.View
{
    /// <summary>
    /// Interaction logic for CPWindow.xaml
    /// </summary>
    public partial class CPWindow : Window
    {
        private HotKeyEditorHelper _hotKeyEditor = new HotKeyEditorHelper();
        private ViewModel.CommandsViewModel _dataContext;
        private DTE2 _dte;
        public VSCommand SelectedVSCommand { get; private set; }

        public CPWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            InitializeComponent();
            _dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE2;
            _dataContext = new ViewModel.CommandsViewModel(_dte);
            this.DataContext = _dataContext;

            // To avoid showing in Alt+Tab
            this.Owner = Application.Current.MainWindow;

            this.PreviewKeyDown += CPWindow_PreviewKeyDown;
            this.MouseDoubleClick += CPWindow_MouseDoubleClick;
        }

        private void CloseWindow()
        {
            Debug.WriteLine(dataGrid.SelectedItem.ToString());
            SelectedVSCommand = dataGrid.SelectedItem as VSCommand;
            this.DialogResult = true;
            this.Close();
        }

        private void CPWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CloseWindow();
        }

        private void CPWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { Close(); }
        }

        private void CPWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SearchTypeInfo();
            txtSearch.Focus();
            if (e.NewValue == null)
            {
                _hotKeyEditor.Detach(txtSearch);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SearchTypeInfo();
            _dataContext.SearchingString = string.Empty;
        }

        private void SearchTypeInfo()
        {
        }

        public void FocusSearchingBox()
        {
            txtSearch.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _dataContext.OpenSettings();
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Down && e.Key != Key.Up && e.Key != Key.PageDown && e.Key != Key.PageUp)
            {
                txtSearch.Focus();
            }
            //if (e.Key == Key.PageDown || e.Key == Key.PageUp)
            //{
            //    int size = e.Key == Key.PageDown ? 18 : -18;
            //    dataGrid.SelectedIndex = (dataGrid.SelectedIndex + dataGrid.Items.Count + size) % dataGrid.Items.Count;
            //    dataGrid.ScrollIntoView(dataGrid.SelectedItem);
            //}
            //if (e.Key == Key.PageDown || e.Key == Key.PageUp)
            //{
            //    //base.OnPreviewKeyDown(e);
            //    DataGrid grid = sender as DataGrid;
            //    ICollectionView view = CollectionViewSource.GetDefaultView(grid.ItemsSource);

            //    switch (e.Key)
            //    {
            //        case Key.PageDown:
            //            view.MoveCurrentToPrevious();
            //            break;

            //        case Key.PageUp:
            //            view.MoveCurrentToNext();
            //            break;
            //    }
            //}
        }

        private void txtSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (dataGrid.Items.Count == 0)
            {
                return;
            }

            if ((e.Key >= Key.A && e.Key <= Key.Z) || 
                (e.Key >= Key.D0 && e.Key <= Key.D9) ||
                (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                dataGrid.SelectedIndex = 0;
                dataGrid.ScrollIntoView(dataGrid.SelectedItem);
                return;
            }

            switch (e.Key)
            {
                case Key.Down:
                    dataGrid.SelectedIndex = (dataGrid.SelectedIndex + 1) % dataGrid.Items.Count;
                    dataGrid.ScrollIntoView(dataGrid.SelectedItem);
                    break;

                case Key.Up:
                    dataGrid.SelectedIndex = (dataGrid.SelectedIndex + dataGrid.Items.Count - 1) % dataGrid.Items.Count;
                    dataGrid.ScrollIntoView(dataGrid.SelectedItem);
                    break;

                case Key.PageDown:
                case Key.PageUp:
                    int size = e.Key == Key.PageDown ? 15 : -15;
                    dataGrid.SelectedIndex = (dataGrid.SelectedIndex + dataGrid.Items.Count + size) % dataGrid.Items.Count;
                    dataGrid.ScrollIntoView(dataGrid.SelectedItem);

                    //var key = e.Key;
                    //var target = dataGrid;
                    //var routedEvent = Keyboard.PreviewKeyDownEvent;
                    //target.RaiseEvent(
                    //    new KeyEventArgs(
                    //        Keyboard.PrimaryDevice,
                    //        PresentationSource.FromVisual(target),
                    //        0,
                    //        key)
                    //    { RoutedEvent = routedEvent });
                    break;

                case Key.Escape:
                    this.Close();
                    break;

                case Key.Enter:
                    CloseWindow();
                    break;

                default:
                    //dataGrid.SelectedIndex = 0;
                    //dataGrid.ScrollIntoView(dataGrid.SelectedItem);
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FocusSearchingBox();
            dataGrid.SelectedIndex = 0;
        }

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
