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
    }
}
