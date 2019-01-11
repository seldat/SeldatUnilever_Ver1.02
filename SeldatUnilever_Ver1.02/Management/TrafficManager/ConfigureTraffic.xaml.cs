using Newtonsoft.Json;
using SelDatUnilever_Ver1._00.Management.TrafficManager;
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
using static SelDatUnilever_Ver1._00.Management.TrafficManager.TrafficRounterService;

namespace SeldatUnilever_Ver1._02.Management.TrafficManager
{
    /// <summary>
    /// Interaction logic for ConfigureTraffic.xaml
    /// </summary>
    public partial class ConfigureTraffic : Window
    {
        TrafficRounterService trafficRounterService;
        public ConfigureTraffic(TrafficRounterService trafficRounterService)
        {
            InitializeComponent();
            this.trafficRounterService = trafficRounterService;
            DataContext = trafficRounterService;
        }

        private void FixedBtn_Click(object sender, RoutedEventArgs e)
        {
            ZoneRegister zr = (sender as Button).DataContext as ZoneRegister;
            trafficRounterService.FixedPropertiesZoneRegister(zr.NameId);
            trafficRounterService.SaveConfig(JsonConvert.SerializeObject(MainDataGrid.ItemsSource, Formatting.Indented));
        }

        private void addbtn_Click(object sender, RoutedEventArgs e)
        {
            trafficRounterService.AddConfig();
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            ZoneRegister zr = (sender as Button).DataContext as ZoneRegister;
            trafficRounterService.ClearZoneRegister(zr.NameId);
            trafficRounterService.SaveConfig(JsonConvert.SerializeObject(MainDataGrid.ItemsSource, Formatting.Indented));
        }
    }
}
