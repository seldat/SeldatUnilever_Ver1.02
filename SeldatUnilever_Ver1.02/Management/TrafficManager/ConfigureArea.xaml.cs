using Newtonsoft.Json;
using SeldatMRMS;
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
    public partial class ConfigureArea : Window
    {
        TrafficRounterService trafficRounterService;
        public ConfigureArea(TrafficRounterService trafficRounterService)
        {
            InitializeComponent();
            this.trafficRounterService = trafficRounterService;
            DataContext = trafficRounterService;
        }

        private void FixedBtn_Click(object sender, RoutedEventArgs e)
        {
            ZoneRegister zr = (sender as Button).DataContext as ZoneRegister;
            trafficRounterService.FixedPropertiesZoneRegister(zr.NameId);
            trafficRounterService.SaveConfigZone(JsonConvert.SerializeObject(MainDataGrid.ItemsSource, Formatting.Indented));
        }

        private void addbtn_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {

            ZoneRegister zr = (sender as Button).DataContext as ZoneRegister;
            trafficRounterService.ClearZoneRegister(zr.NameId);
            trafficRounterService.SaveConfigZone(JsonConvert.SerializeObject(MainDataGrid.ItemsSource, Formatting.Indented));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(Global_Object.userLogin==(int)Global_Object.PRIORITYLOGIN.PRIORITYLOGIN_ADMIN0)
            {
                btnFixed.IsReadOnly = true;
                btnRemove.IsReadOnly = true;
                btn_addItem.IsEnabled = true;
            }
            else
            {
                btnFixed.IsReadOnly = false;
                btnRemove.IsReadOnly = false;
                btn_addItem.IsEnabled = false;
            }
        }

        private void btn_addItem_Click(object sender, RoutedEventArgs e)
        {
            trafficRounterService.AddConfigZone();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
