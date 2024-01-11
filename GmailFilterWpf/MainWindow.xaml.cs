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

        private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Connecting...";
                StatusText.Foreground = Brushes.Black;
                _gmf = new GmailFilter1();

                _gmf.Log = (m) =>
                {
                    Trace.WriteLine(m);
                };
                
                _gmf.Connect(CredentialsFileText.Text, TokenFileText.Text);
                StatusText.Text = "Connected.";
                StatusText.Foreground = Brushes.Blue;

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
