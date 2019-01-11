using Newtonsoft.Json;
using SeldatMRMS;
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

using static SelDatUnilever_Ver1._00.Management.ChargerCtrl.ChargerCtrl;

namespace SeldatUnilever_Ver1._02.Management.ChargerCtrl
{
    /// <summary>
    /// Interaction logic for ConfigureCharger.xaml
    /// </summary>
    public partial class ConfigureCharger : Window
    {
        ChargerManagementService chargerManagementService;
        public ConfigureCharger(ChargerManagementService chargerManagementService)
        {
            InitializeComponent();
            this.chargerManagementService = chargerManagementService;
            DataContext = chargerManagementService;
        }

        private void FixedBtn_Click(object sender, RoutedEventArgs e)
        {
            ChargerInfoConfig cf = (sender as Button).DataContext as ChargerInfoConfig;
            this.chargerManagementService.FixedConfigure(cf.Id,cf);
            this.chargerManagementService.SaveConfig(JsonConvert.SerializeObject(MainDataGrid.ItemsSource, Formatting.Indented));
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Global_Object.userLogin == (int)Global_Object.PRIORITYLOGIN.PRIORITYLOGIN_ADMIN0)
            {
                btnFixed.IsReadOnly = true;
            }
            else
            {
                btnFixed.IsReadOnly = false;
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
