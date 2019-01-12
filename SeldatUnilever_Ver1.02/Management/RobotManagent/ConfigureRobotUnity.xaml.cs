﻿using Newtonsoft.Json;
using SeldatMRMS;
using SeldatMRMS.Management.RobotManagent;
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
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;

namespace SeldatUnilever_Ver1._02.Management.RobotManagent
{
    /// <summary>
    /// Interaction logic for ConfigureRobotUnity.xaml
    /// </summary>
    public partial class ConfigureRobotUnity : Window
    {
        private RobotManagementService robotManagementService;
        public ConfigureRobotUnity(RobotManagementService robotManagementService )
        {
            InitializeComponent();
            this.robotManagementService = robotManagementService;
            DataContext = robotManagementService;
        }

        private void FixedBtn_Click(object sender, RoutedEventArgs e)
        {
            PropertiesRobotUnity properties = (sender as Button).DataContext as PropertiesRobotUnity;
            robotManagementService.FixedPropertiesRobotUnity(properties.NameId,properties);
            robotManagementService.SaveConfig(JsonConvert.SerializeObject(MainDataGrid.ItemsSource, Formatting.Indented));
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
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
    }
}
