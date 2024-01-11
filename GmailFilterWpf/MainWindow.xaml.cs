using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using GmailFilterLibrary;

namespace GmailFilterWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GmailFilter1 _gmf;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Connecting...";
                StatusText.Foreground = Brushes.Black;
                _gmf = new GmailFilter1();

                _gmf.Log = (m) => { Dispatcher.Invoke(() => { StatusText.Text = m; }, DispatcherPriority.Render); };
                _gmf.Connect(CredentialsFileText.Text, TokenFileText.Text);
                _gmf.Log("Connected");

                int numDaysToLoad = int.Parse(DaysToLoadText.Text);

                _gmf.LoadEmails(numDaysToLoad);
                _gmf.Log("Done Loading");
            }
            catch (Exception ex)
            {
                StatusText.Foreground = Brushes.Red; 
                StatusText.Text = ex.Message; 
            }
        }
    }
}
