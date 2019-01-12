﻿using SeldatMRMS.Management.DoorServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public DoorConfigure(DoorManagementService doorManagementService, string cultureName = null)
        {
            InitializeComponent();
            ApplyLanguage(cultureName);
            this.doorManagementService = doorManagementService;
            DataContext = doorManagementService;
        }

        public void ApplyLanguage(string cultureName = null)
        {
            if (cultureName != null)
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);

            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "vi-VN":
                    dict.Source = new Uri("..\\Lang\\Vietnamese.xaml", UriKind.Relative);
                    break;
                // ...
                default:
                    dict.Source = new Uri("..\\Lang\\English.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }

        private void FixedBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
