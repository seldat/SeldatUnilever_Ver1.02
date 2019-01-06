using SeldatMRMS.Management.DoorServices;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using SelDatUnilever_Ver1._00.Management.ChargerCtrl;
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

namespace SelDatUnilever_Ver1._00.Management.UnityService
{
    /// <summary>
    /// Interaction logic for ConfigUnities.xaml
    /// </summary>
    public partial class ConfigUnities : Window
    {
        public RobotManagementService robotManagementService { get; set; }
        public DoorManagementService doorManagementService { get; set; }
        public TrafficManagementService trafficService { get; set; }
        public ChargerManagementService chargerService;
        public ConfigUnities()
        {
            InitializeComponent();
        }
        public void Registry(RobotManagementService robotManagementService)
        {
            this.robotManagementService = robotManagementService;
        }
        public void Registry(DoorManagementService doorManagementService)
        {
            this.doorManagementService = doorManagementService;
        }
        public void Registry(ChargerManagementService chargerService)
        {
            this.chargerService = chargerService;

        }
        public void Registry(TrafficManagementService trafficService)
        {
            this.trafficService = trafficService;

        }
        private void Btn_Add_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
