using SeldatUnilever_Ver1._02.DTO;
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

namespace SeldatUnilever_Ver1._02.Management.Statistics
{
    /// <summary>
    /// Interaction logic for Statistics.xaml
    /// </summary>
    public partial class Statistics : Window
    {
        StatisticsModel statisticsModel;
        public Statistics(string cultureName = null)
        {
            InitializeComponent();
            ApplyLanguage(cultureName);
            Loaded += Statistics_Loaded;
            statisticsModel = new StatisticsModel(this);
            DataContext = statisticsModel;
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

        private void Statistics_Loaded(object sender, RoutedEventArgs e)
        {
            statisticsModel.ReloadListProduct();
            statisticsModel.ReloadListProductDetail();
            statisticsModel.ReloadListOperationType();
            statisticsModel.ReloadListRobot();
            statisticsModel.ReloadListDevice();
            statisticsModel.ReloadListBuffer();
            statisticsModel.ReloadListTimeWork();
        }


        private void CmbDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            statisticsModel.ReloadListProduct();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (dtRobotProcess item in statisticsModel.listRobotProcess)
            {
                Console.WriteLine(item.operationType);
            }
        }

        private void CmbProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int productDetail = -1;
            if (cmbProductDetail.SelectedValue != null && cmbProductDetail.SelectedValue.ToString() != "")
            {
                productDetail = int.Parse(cmbProductDetail.SelectedValue.ToString());
            }
            statisticsModel.ReloadListProductDetail();
            if (productDetail != -1)
            {
                cmbProductDetail.SelectedValue = productDetail;
            }
        }

        private void CmbDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            int product = -1;
            if (cmbProduct.SelectedValue != null && cmbProduct.SelectedValue.ToString() != "")
            {
                product = int.Parse(cmbProduct.SelectedValue.ToString());
            }
            statisticsModel.ReloadListProduct();
            if (product != -1)
            {
                cmbProduct.SelectedValue = product;
            }
        }

        private void CmbShift_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbShift.SelectedValue != null && int.Parse(cmbShift.SelectedValue.ToString()) > 0)
            {
                dtpActiveDate.IsEnabled = true;
            }
            else
            {
                dtpActiveDate.IsEnabled = false;
            }
        }


        private void Search_Click(object sender, RoutedEventArgs e)
        {
            statisticsModel.ReloadDataGridTask();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
