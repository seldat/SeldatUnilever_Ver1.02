using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace SeldatUnilever_Ver1._02.Management.RobotManagent
{
    /// <summary>
    /// Interaction logic for RobotLogOut.xaml
    /// </summary>
    public partial class RobotLogOut : Window
    {
        public RobotLogOut(String name)
        {
            InitializeComponent();
            
            
        }
        public void ShowText(String src,String txt)
        {
            Task.Run(() =>
            {
                try { 
                    Dispatcher.Invoke(() =>
                    {
                        this.Title = src;
                        txt_logout.AppendText(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss tt") + ": " + txt + Environment.NewLine);
                        
                    });
                }
                catch { }
            });
        }
        public void ShowTextTraffic(String txt)
        {
            Task.Run(() =>
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        txt_logout_traffic.AppendText(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss tt") + ": " + txt + Environment.NewLine);

                    });
                }
                catch { }
            });
        }
        public void Clear()
        {
            txt_logout.Document.Blocks.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }
    }
}
