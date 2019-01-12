using SeldatMRMS;
using SeldatMRMS.Management.DoorServices;
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

namespace SeldatUnilever_Ver1._02.Management.DoorServices
{
    /// <summary>
    /// Interaction logic for DoorConfigure.xaml
    /// </summary>
    public partial class DoorConfigure : Window
    {
        DoorManagementService doorManagementService;
        public DoorConfigure(DoorManagementService doorManagementService)
        {
            InitializeComponent();
            this.doorManagementService = doorManagementService;
            DataContext = doorManagementService;
        }

        private void FixedBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Global_Object.userLogin == (int)Global_Object.PRIORITYLOGIN.PRIORITYLOGIN_ADMIN0)
            {
                MainDataGrid.IsEnabled = true;
            }
            else
            {
                MainDataGrid.IsEnabled = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
