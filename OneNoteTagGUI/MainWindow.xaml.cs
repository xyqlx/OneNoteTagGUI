using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OneNoteTagGUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        PageBrowser pageBrowser;
        public MainWindow()
        {
            this.SizeChanged += MainWindow_SizeChanged;
            InitializeComponent();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ScrollViewerContent.Height = this.ActualHeight - 260;
        }

        private void InitData()
        {
            pageBrowser = new PageBrowser(Xyqlx.OneNote.App.Notebooks.First(x => x.Name == "介质").Sections.First(x => x.Name == "新页面"));
            this.MainStack.DataContext = pageBrowser;
            this.IsEnabled = true;
            this.TextBoxTags.Focus();
            Task.Run(() => pageBrowser.ClacCommonTags());
        }

        private void Btn_Open(object sender, RoutedEventArgs e)
        {
            pageBrowser.Open();
        }
        private void Btn_Next(object sender, RoutedEventArgs e)
        {
            pageBrowser.MoveToNext();
        }
        private void Btn_Previous(object sender, RoutedEventArgs e)
        {
            pageBrowser.MoveToPrevious();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listbox = (ListBox)sender;
            if (listbox.SelectedIndex == -1)
                return;
            var tagname = (string)listbox.SelectedValue;
            pageBrowser.RemoveTag(tagname);
            this.TextBoxTags.Focus();
        }

        private void ListBoxCommon_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listbox = (ListBox)sender;
            if (listbox.SelectedIndex == -1)
                return;
            var tagname = (string)listbox.SelectedValue;
            pageBrowser.AddTags(new string[] { tagname });
            this.TextBoxTags.Focus();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    pageBrowser.AddTags(this.TextBoxTags.Text.Split(' ', ';'));
                    this.TextBoxTags.Text = "";
                    if (e.KeyboardDevice.Modifiers != ModifierKeys.Control)
                        pageBrowser.MoveToNext();
                    break;
                case Key.Up:
                    pageBrowser.MoveToPrevious();
                    break;
                case Key.Down:
                    pageBrowser.MoveToNext();
                    break;
                case Key.PageUp:
                    pageBrowser.MoveToTop();
                    break;
                case Key.PageDown:
                    pageBrowser.MoveToBottom();
                    break;
            }
        }

        private void TextBoxPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                TextBox textBox = (TextBox)sender;
                textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
            else
            {
                TextBox_KeyDown(sender, e);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitData();
        }

        private void TextBoxFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                pageBrowser.UpdateFilters(TextBoxFilter.Text);
            }
        }
    }
}
