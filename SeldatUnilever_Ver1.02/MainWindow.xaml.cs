﻿using SeldatMRMS;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.UnityService;
using SeldatUnilever_Ver1._02.Form;
using SeldatUnilever_Ver1._02.Management.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SeldatUnilever_Ver1._02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public System.Timers.Timer stationTimer;
        public System.Timers.Timer robotTimer;

        public bool drag = true;
        private UnityManagementService unityService;
        private CanvasControlService canvasControlService;
        CtrlRobot ctrR;
        public MainWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            Loaded += MainWindow_Loaded;

            canvasMatrixTransform = new MatrixTransform(1, 0, 0, -1, 0, 0);

            ImageBrush img = LoadImage("Map");
            map.Width = img.ImageSource.Width;
            map.Height = img.ImageSource.Height;
            map.Background = img;
            canvasControlService = new CanvasControlService(this);

        }

        private void OnTimedRedrawStationEvent(object sender, ElapsedEventArgs e)
        {
            Parallel.Invoke(() =>
            {
                //Console.WriteLine("Begin first task...");
                canvasControlService.RedrawAllStation(canvasControlService.GetDataAllStation());
            });
        }
        

        private void OnTimedRedrawRobotEvent(object sender, ElapsedEventArgs e)
        {
           //Task.Run(()=> canvasControlService.RedrawAllStation());
            canvasControlService.RedrawAllRobot();
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CenterWindowOnScreen();
            //TEST frm1 = new TEST();
            //frm1.ShowDialog();
            myManagementWindow.Visibility = Visibility.Hidden;
            //LoginForm frm = new LoginForm(Thread.CurrentThread.CurrentCulture.ToString());
            //frm.ShowDialog();
            //if (Global_Object.userLogin <= 2)
            {
                myManagementWindow.Visibility = Visibility.Visible;
                unityService = new UnityManagementService(this);
                unityService.Initialize();
            }



            stationTimer = new System.Timers.Timer();
            stationTimer.Interval = 5000;
            stationTimer.Elapsed += OnTimedRedrawStationEvent;
            stationTimer.AutoReset = true;
            stationTimer.Enabled = true;

            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                canvasControlService.ReloadAllStation();
            }));
           
         

            /*   Dispatcher.BeginInvoke(new ThreadStart(() =>
               {
                   for (int i = 1; i < 5; i++)
                   {
                       Random posX = new Random();
                       RobotShape rbot = new RobotShape(map);
                       rbot.rad = posX.Next(50, 120);
                       rbot.org = new Point(600 + posX.Next(10, 50), 386 + posX.Next(10, 50));
                       rbot.anglestep = posX.NextDouble() + 0.2;
                       rbot.ReDraw(new Point(0, 0), 0);
                       //rbot.ChangeTask("22");
                       canvasControlService.list_Robot.Add(i.ToString(), rbot);
                       Thread.Sleep(100);
                   }
               }));*/
            /* robotTimer = new System.Timers.Timer();
             robotTimer.Interval = 50;
             robotTimer.Elapsed += OnTimedRedrawRobotEvent;
             robotTimer.AutoReset = true;
             robotTimer.Enabled = true;*/
        }


        private void btn_ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ChangePassForm changePassForm = new ChangePassForm(Thread.CurrentThread.CurrentCulture.ToString());
            changePassForm.ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            myManagementWindow.Visibility = Visibility.Hidden;

            Global_Object.userAuthor = -2;
            Global_Object.userLogin = -2;
            Global_Object.userName = "";

            LoginForm frm = new LoginForm(Thread.CurrentThread.CurrentCulture.ToString());
            frm.ShowDialog();
            if (Global_Object.userLogin <= 2)
            {
                myManagementWindow.Visibility = Visibility.Visible;
            }
        }

        public ImageBrush LoadImage(string name)
        {
            System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject(name);
            ImageBrush img = new ImageBrush();
            img.ImageSource = ImageSourceForBitmap(bmp);
            return img;
        }

        public ImageSource ImageSourceForBitmap(System.Drawing.Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            ApplyLanguage(menuItem.Tag.ToString());

        }

        private void ApplyLanguage(string cultureName = null)
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

            // check/uncheck the language menu items based on the current culture
            foreach (var item in languageMenuItem.Items)
            {
                MenuItem menuItem = item as MenuItem;
                if (menuItem.Tag.ToString() == Thread.CurrentThread.CurrentCulture.Name)
                    menuItem.IsChecked = true;
                else
                    menuItem.IsChecked = false;
            }
        }

        private void Btn_MapReCenter_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Ctrl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                ctrR.Show();
            }
            catch { }
        }

        private void Btn_Robot_Click(object sender, RoutedEventArgs e)
        {
            unityService.OpenConfigureForm("RCF");
        }

        private void Btn_Area_Click(object sender, RoutedEventArgs e)
        {
            unityService.OpenConfigureForm("ACF");
        }

        private void Btn_Charge_Click(object sender, RoutedEventArgs e)
        {
            unityService.OpenConfigureForm("CCF");
        }

        private void Btn_Door_Click(object sender, RoutedEventArgs e)
        {
            unityService.OpenConfigureForm("DCF");
        }

        private void Btn_Statistics_Click(object sender, RoutedEventArgs e)
        {
            Statistics statistics = new Statistics();
            statistics.ShowDialog();
        }
    }
}
