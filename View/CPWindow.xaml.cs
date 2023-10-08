using CommandPalette;
using CommandPalette.ViewModel;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using SearchingKeyboardShortcut;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CPSettings = CommandPalette.Options.Settings;

namespace CommandPalette.View
{
    /// <summary>
    /// Interaction logic for CPWindow.xaml
    /// </summary>
    public partial class CPWindow : Window
    {
        private HotKeyEditorHelper  _hotKeyEditor = new HotKeyEditorHelper();
        private CommandsViewModel   _dataContext;
        private DTE2                _dte;
        private const string        _applicationName = "Command Palette";
        private Options.Settings    _settings;

        public  VSCommand           SelectedVSCommand { get; private set; }

        public CPWindow()
        {
            InitializeComponent();

#if __CommandPalette
            ThreadHelper.ThrowIfNotOnUIThread();
            _dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE2;
            _dataContext = new ViewModel.CommandsViewModel(_dte, PostRefresh);
            this.DataContext = _dataContext;

            this._settings = CommandPalettePackage.GetSettings();

            // To avoid showing in Alt+Tab
            this.Owner = Application.Current.MainWindow;
#else
            this.Background = System.Windows.Media.Brushes.White;
            _dataContext = new ViewModel.CommandsViewModel(_dte, PostRefresh);
            this.DataContext = _dataContext;

            this._settings = Options.Settings.Load();
#endif

            // Remove minimize and maximize icons in title bar
            this.SourceInitialized += (x, y) => { this.HideMinimizeAndMaximizeButtons(); };
            this.PreviewKeyDown += CPWindow_PreviewKeyDown;
            this.MouseDoubleClick += CPWindow_MouseDoubleClick;

            // Set the previous command
            _dataContext.SearchingString = _settings.PreviousCommand;
        }

        private void UpdateWindowTitle()
        {
            this.Title = String.Format("{0} [{1} of {2}]", _applicationName, dataGrid.Items.Count, _dataContext.ItemsSource.Count);
        }

        private void PostRefresh()
        {
            if (dataGrid.Items.Count > 0)
            {
                dataGrid.SelectedIndex = 0;
                dataGrid.ScrollIntoView(dataGrid.SelectedItem);
            }
            UpdateWindowTitle();
        }

        private void CloseWindow()
        {
            Debug.WriteLine(dataGrid.SelectedItem.ToString());
            SelectedVSCommand = dataGrid.SelectedItem as VSCommand;
            _settings.PreviousCommand = _dataContext.SearchingString;

#if !__CommandPalette
            _settings.Save();
#endif

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
            txtSearch.SelectionStart = 0;
            txtSearch.SelectionLength = _dataContext.SearchingString.Length;
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
        }

        private void txtSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("txtSearch_PreviewKeyDown");
            if (dataGrid.Items.Count == 0)
            {
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
                    if (dataGrid.SelectedIndex == -1) dataGrid.SelectedIndex = 0;
                    var key = e.Key;
                    var target = GetCell();
                    target.Focus();
                    var routedEvent = Keyboard.KeyDownEvent;
                    target.RaiseEvent(
                        new KeyEventArgs(
                            Keyboard.PrimaryDevice,
                            PresentationSource.FromVisual(target),
                            0,
                            key)
                        { RoutedEvent = routedEvent });
                    FocusSearchingBox();
                    e.Handled = true;
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

        private DataGridCell GetCell()
        {
            var row = dataGrid.ItemContainerGenerator.ContainerFromItem(dataGrid.SelectedItem) as DataGridRow;
            if (row != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);
                if (presenter == null)
                {
                    dataGrid.ScrollIntoView(row, dataGrid.Columns[0]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(row);
                }
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(0);
                return cell;
            }
            return null;
        }

        private static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FocusSearchingBox();
            if (dataGrid.Items.Count > 0)
            {
                dataGrid.SelectedIndex = 0;
                dataGrid.ScrollIntoView(dataGrid.SelectedItem);
            }
            UpdateWindowTitle();
        }

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    internal static class WindowExtensions
    {
        // from winuser.h
        private const int GWL_STYLE = -16,
                          WS_MAXIMIZEBOX = 0x10000,
                          WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        extern private static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        extern private static int SetWindowLong(IntPtr hwnd, int index, int value);

        internal static void HideMinimizeAndMaximizeButtons(this Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
        }
    }
}
