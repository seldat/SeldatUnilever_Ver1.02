﻿using SeldatMRMS.RobotView;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using static SelDatUnilever_Ver1._00.Management.ChargerCtrl.ChargerCtrl;

namespace SeldatMRMS.Management.RobotManagent {
    public class RobotUnity : RobotBaseService {

        Ellipse headerPoint;
        Path riskArea;
        double angle = 0.0f;
        public Point org = new Point (600, 350);
        public double rad = 0;
        public double anglestep = 0;
        Rect area = new Rect (30, 30, 500, 500);
        Point loc = new Point (0, 0);
        public event Action<string> RemoveHandle;
        public enum RobotStatusColorCode {
            ROBOT_STATUS_OK = 0,
            ROBOT_STATUS_RUNNING,
            ROBOT_STATUS_ERROR,
            ROBOT_STATUS_WAIT_FIX,
        }
        public struct Props {
            public string name;
            public bool isSelected;
            public bool isHovering;
            public Grid mainGrid;
            public Grid statusGrid;

            public Label rbID;
            public Label rbTask;
            public Rectangle headLed;
            public Rectangle tailLed;
            public TranslateTransform rbTranslate;
            public TransformGroup rbTransformGroup;
            public RotateTransform rbRotateTransform;
            public TranslateTransform contentTranslate;
            public TransformGroup contentTransformGroup;
            public RotateTransform contentRotateTransform;
            public Border statusBorder;
            public List<Point> eightCorner;
        }
        public Canvas canvas;
        public Props props;
        public Border border;
        public LoadedConfigureInformation loadConfigureInformation;
        public RobotManagementService robotService;
        public SolvedProblem solvedProblem;
        public Robot3D robot3DModel = null;
        public RobotUnity(){
            solvedProblem = new SolvedProblem();

        }
        MenuItem problemSolutionItem = new MenuItem();
        MenuItem startItem = new MenuItem();
        MenuItem pauseItem = new MenuItem();
        MenuItem connectItem = new MenuItem();
        MenuItem disconnectItem = new MenuItem();
        MenuItem addReadyListItem = new MenuItem();
        MenuItem addWaitTaskListItem = new MenuItem();
        MenuItem logOutItem = new MenuItem();
        public void Initialize(Canvas canvas)
        {
            this.canvas = canvas;
            //ModelVisual3D layer = new ModelVisual3D();
           // robot3DModel = new Robot3D(properties.NameId, layer);
            riskArea = new Path ();
            riskArea.Stroke = new SolidColorBrush (Colors.MediumBlue);
            riskArea.StrokeThickness = 1;
            border = new Border ();
            border.ToolTip = "";
            border.ToolTipOpening += ChangeToolTipContent;
            props.isSelected = false;
            props.isHovering = false;
            border.ContextMenu = new ContextMenu ();
            
            
            problemSolutionItem.Header = "Problem Solution";
            problemSolutionItem.Click += PoblemSolutionItem;
            //===================================
           
            startItem.Header = "Start";
            startItem.Click += StartMenu;
            //===================================
           
            pauseItem.Header = "Pause";
            pauseItem.Click += PauseMenu;
            pauseItem.IsEnabled = true;

            logOutItem.Header = "Log";
            logOutItem.Click += LogOut;
            logOutItem.IsEnabled = true;

            addReadyListItem.Header = "Add To Ready Mode";
            addReadyListItem.Click += AddReadyListMenu;
            addReadyListItem.IsEnabled = true;

            addWaitTaskListItem.Header = "Add To WaitTask Mode";
            addWaitTaskListItem.Click += AddWaitTaskListMenu;
            addWaitTaskListItem.IsEnabled = true;



            border.ContextMenu.Items.Add(problemSolutionItem);
            border.ContextMenu.Items.Add(startItem);
            border.ContextMenu.Items.Add(pauseItem);
            border.ContextMenu.Items.Add(logOutItem);
            border.ContextMenu.Items.Add(addReadyListItem);
            border.ContextMenu.Items.Add(addWaitTaskListItem);
            
            //====================EVENT=====================
            //MouseLeave += MouseLeavePath;
            //MouseMove += MouseHoverPath;
            //MouseLeftButtonDown += MouseLeftButtonDownPath;
            //MouseRightButtonDown += MouseRightButtonDownPath;
            //===================CREATE=====================
            //Name = "Robotx" + Global_Mouse.EncodeTransmissionTimestamp();
            props.mainGrid = new Grid ();
            props.statusGrid = new Grid ();
            props.statusBorder = new Border ();
            props.rbID = new Label ();
            props.rbTask = new Label ();
            props.headLed = new Rectangle ();
            props.tailLed = new Rectangle ();
            props.eightCorner = new List<Point> ();
            for (int i = 0; i < 8; i++) {
                Point temp = new Point ();
                props.eightCorner.Add (temp);
            }
            props.rbRotateTransform = new RotateTransform ();
            props.rbTranslate = new TranslateTransform ();
            props.rbTransformGroup = new TransformGroup ();
            props.contentRotateTransform = new RotateTransform ();
            props.contentTranslate = new TranslateTransform ();
            props.contentTransformGroup = new TransformGroup ();
            // robotProperties = new Properties(this);
            //===================STYLE=====================
            //Robot border
            border.Width = 22;
            border.Height = 15;
            border.BorderThickness = new Thickness (1);
            border.BorderBrush = new SolidColorBrush (Colors.Linen);
            border.Background = new SolidColorBrush (Colors.Blue);
            border.CornerRadius = new CornerRadius (3);
            border.RenderTransformOrigin = new Point (0.5, 0.5);
            //mainGrid
            props.mainGrid.Background = new SolidColorBrush (Colors.Transparent);
            for (int i = 0; i < 3; i++) {
                ColumnDefinition colTemp = new ColumnDefinition ();
                //colTemp.Name = properties.NameID + "xL" + i;
                if ((i == 0) || (i == 2)) {
                    colTemp.Width = new GridLength (1);
                }
                props.mainGrid.ColumnDefinitions.Add (colTemp);
            }
            //headLed
            props.headLed.Height = 7;
            props.headLed.Fill = new SolidColorBrush (Colors.LightYellow);
            Grid.SetColumn (props.headLed, 2);
            //tailLed
            props.tailLed.Height = 7;
            props.tailLed.Fill = new SolidColorBrush (Colors.OrangeRed);
            Grid.SetColumn (props.tailLed, 0);
            //statusBorder
            props.statusBorder.Width = 10;
            props.statusBorder.Height = 13;
            props.statusBorder.RenderTransformOrigin = new Point (0.5, 0.5);
            Grid.SetColumn (props.statusBorder, 1);
            //statusGrid
            for (int i = 0; i < 2; i++) {
                RowDefinition rowTemp = new RowDefinition ();
                // rowTemp.Name = properties.NameID + "xR" + i;
                props.statusGrid.RowDefinitions.Add (rowTemp);
            }
            //rbID
            props.rbID.Padding = new Thickness (0);
            props.rbID.Margin = new Thickness (-5, 0, -5, 0);
            props.rbID.HorizontalAlignment = HorizontalAlignment.Center;
            props.rbID.VerticalAlignment = VerticalAlignment.Bottom;
            props.rbID.Content = "27";
            props.rbID.Foreground = new SolidColorBrush (Colors.Yellow);
            props.rbID.FontFamily = new FontFamily ("Calibri");
            props.rbID.FontSize = 6;
            props.rbID.FontWeight = FontWeights.Bold;
            Grid.SetRow (props.rbID, 0);

            //rbTask
            props.rbTask.Padding = new Thickness (0);
            props.rbTask.Margin = new Thickness (-5, -1, -5, -1);
            props.rbTask.HorizontalAlignment = HorizontalAlignment.Center;
            props.rbTask.VerticalAlignment = VerticalAlignment.Top;
            props.rbTask.Content = "9999";
            props.rbTask.Foreground = new SolidColorBrush (Colors.LawnGreen);
            props.rbTask.FontFamily = new FontFamily ("Calibri");
            props.rbTask.FontSize = 6;
            props.rbTask.FontWeight = FontWeights.Bold;
            Grid.SetRow (props.rbTask, 1);

            //===================CHILDREN===================
            props.statusGrid.Children.Add (props.rbID);
            props.statusGrid.Children.Add (props.rbTask);
            props.statusBorder.Child = props.statusGrid;
            props.mainGrid.Children.Add (props.headLed);
            props.mainGrid.Children.Add (props.tailLed);
            props.mainGrid.Children.Add (props.statusBorder);
            props.rbTransformGroup.Children.Add (props.rbRotateTransform);
            props.rbTransformGroup.Children.Add (props.rbTranslate);
            border.RenderTransform = props.rbTransformGroup;
            props.contentTransformGroup.Children.Add (props.contentRotateTransform);
            props.contentTransformGroup.Children.Add (props.contentTranslate);
            props.statusBorder.RenderTransform = props.contentTransformGroup;
            border.Child = props.mainGrid;
            this.canvas.Children.Add (border);
            headerPoint = new Ellipse ();
            headerPoint.Width = 5;
            headerPoint.Height = 5;
            headerPoint.Fill = new SolidColorBrush (Colors.Red);
            canvas.Children.Add (riskArea);
            canvas.Children.Add (headerPoint);
            Draw ();

        }

        private void Itemw0_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        public void RegistrySolvedForm(Object obj)
        {
          //  if(obj.GetType()==typeof(ProcedureControlServices))
            {
                solvedProblem.Registry(obj);
                //solvedProblem.Show();
            }
        }
        public void RegistryRobotService(RobotManagementService robotService)
        {
            this.robotService = robotService;
        }
        public void DestroyRegistrySolvedForm()
        {
            //  if(obj.GetType()==typeof(ProcedureControlServices))
            {
                solvedProblem.Dispose();
                //solvedProblem.Show();
            }
        }
        public void DisplaySolvedForm()
        {
            if (solvedProblem.obj != null)
                solvedProblem.Show();
            else
                MessageBox.Show("Không có nội dung lỗi!!");
        }
        public Point CirclePoint(double radius, double angleInDegrees, Point origin)
        {
            double x = (double)(radius * Math.Cos(angleInDegrees * Math.PI / 180)) + origin.X;
            double y = (double)(radius * Math.Sin(angleInDegrees * Math.PI / 180)) + origin.Y;
            return new Point (x, y);
        }
        //public void setColorRobotStatus(RobotStatusColorCode rsc)
        //{
        //    switch (rsc)
        //    {
        //        case RobotStatusColorCode.ROBOT_STATUS_OK:
        //            border.Background = new SolidColorBrush(Colors.Blue);
        //            break;
        //        case RobotStatusColorCode.ROBOT_STATUS_ERROR:
        //            border.Background = new SolidColorBrush(Colors.Red);
        //            break;
        //    }
        //}

        public void setColorRobotStatus (RobotStatusColorCode rsc) {
            switch (rsc) {
                case RobotStatusColorCode.ROBOT_STATUS_OK:
                    {
                        RobotUnity robotTemp = new RobotUnity ();
                        robotTemp = this;
                        robotTemp.border.Background = new SolidColorBrush (Colors.Blue);
                        break;
                    }
                case RobotStatusColorCode.ROBOT_STATUS_ERROR:
                    {
                        RobotUnity robotTemp = new RobotUnity ();
                        robotTemp = this;
                        robotTemp.border.Background = new SolidColorBrush (Colors.Red);
                        break;
                    }
                case RobotStatusColorCode.ROBOT_STATUS_RUNNING:
                    {
                        RobotUnity robotTemp = new RobotUnity ();
                        robotTemp = this;
                        robotTemp.border.Background = new SolidColorBrush (Colors.Green);
                        break;
                    }
                case RobotStatusColorCode.ROBOT_STATUS_WAIT_FIX:
                    {
                        RobotUnity robotTemp = new RobotUnity ();
                        robotTemp = this;
                        robotTemp.border.Background = new SolidColorBrush (Colors.Yellow);
                        break;
                    }
            }
        }
        private void ChangeToolTipContent (object sender, ToolTipEventArgs e) {
            border.ToolTip = "1234567890";
        }

        private void PoblemSolutionItem(object sender, RoutedEventArgs e)
        {
            DisplaySolvedForm();
        }
        private void PauseMenu(object sender, RoutedEventArgs e)
        {
            SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
        }
        private void AddReadyListMenu(object sender, RoutedEventArgs e)
        {        
            string msgtext = "Hãy đảm bảo Robot đã được đưa về vị trí trạm Sạc, Quy trình trước đó sẽ được hủy !";
            string txt = "Cảnh báo";
            MessageBoxButton button = MessageBoxButton.OKCancel;
            MessageBoxResult result = MessageBox.Show(msgtext, txt, button);
            switch (result)
            {
                case MessageBoxResult.OK:
                    DisposeProcedure();
                    TurnOnSupervisorTraffic(false);
                    robotService.RemoveRobotUnityReadyList(this.properties.NameId);
                    robotService.RemoveRobotUnityWaitTaskList(this.properties.NameId);
                    robotService.AddRobotUnityReadyList(this);
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
        }
        public void LogOut(object sender, RoutedEventArgs e)
        {
            robotLogOut.Show();
        }
        public void ShowText(String text)
        {
            robotLogOut.ShowText(this.properties.Label,text);
           
        }
        private void AddWaitTaskListMenu(object sender, RoutedEventArgs e)
        {


            string msgtext = "Quy trình trước đó sẽ được hủy, Robot sẽ bắt đầu với quy trình mới !";
            string txt = "Cảnh báo";
            MessageBoxButton button = MessageBoxButton.OKCancel;
            MessageBoxResult result = MessageBox.Show(msgtext, txt, button);
            switch (result)
            {
                case MessageBoxResult.OK:
                    DisposeProcedure();
                    TurnOnSupervisorTraffic(true);
                    robotService.RemoveRobotUnityReadyList(this.properties.NameId);
                    robotService.RemoveRobotUnityWaitTaskList(this.properties.NameId);
                    robotService.AddRobotUnityWaitTaskList(this);
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
        }
        private void StartMenu(object sender, RoutedEventArgs e)
        {
            SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
        }
        public void RemoveDraw()
        {
            this.canvas.Children.Remove(border);
            this.canvas.Children.Remove(headerPoint);
            this.canvas.Children.Remove(riskArea);
            //RemoveHandle(props.name);
        }      
        public override void Draw () {

           if (webSocket != null)
            {
              if (webSocket.IsAlive)
                {

                    //Render Robot
                    this.border.Dispatcher.BeginInvoke(new System.Threading.ThreadStart(() =>
                     {

                         props.rbRotateTransform.Angle = -properties.pose.Angle;
                         Point cPoint = Global_Object.CoorCanvas(properties.pose.Position);
                         props.rbTranslate = new TranslateTransform(cPoint.X - (border.Width / 2), cPoint.Y - (border.Height / 2));
                         props.rbTransformGroup.Children[1] = props.rbTranslate;
                     //Render Status
                     props.contentRotateTransform.Angle = (properties.pose.Angle);
                         props.contentTranslate = new TranslateTransform(0, 0);
                         props.contentTransformGroup.Children[1] = props.contentTranslate;
                     // headerPoint.RenderTransform = new TranslateTransform(MiddleHeader().X-5, MiddleHeader().Y-5);
                     // headerPoint.RenderTransform = new TranslateTransform(Global_Object.CoorCanvas(MiddleHeader()).X-5, Global_Object.CoorCanvas(MiddleHeader()).Y+0.5);
                     headerPoint.RenderTransform = new TranslateTransform(MiddleHeaderCv().X - 2.5, MiddleHeaderCv().Y - 1);

                         PathGeometry pgeometry = new PathGeometry();
                         PathFigure pF = new PathFigure();
                         pF.StartPoint = TopHeaderCv();

                     // pF.StartPoint = new Point(TopHeader().X * 10, TopHeader().Y * 10);
                     LineSegment pp = new LineSegment();

                         pF.Segments.Add(new LineSegment() { Point = BottomHeaderCv() });
                         pF.Segments.Add(new LineSegment() { Point = BottomTailCv() });
                         pF.Segments.Add(new LineSegment() { Point = TopTailCv() });
                         pF.Segments.Add(new LineSegment() { Point = TopHeaderCv() });
                     // pF.Segments.Add(new LineSegment() { Point = new Point(BottomHeader().X*10, BottomHeader().Y * 10) });
                     //pF.Segments.Add(new LineSegment() { Point = new Point(BottomTail().X * 10, BottomTail().Y * 10) });
                     //  pF.Segments.Add(new LineSegment() { Point = new Point(TopTail().X * 10, TopTail().Y * 10) });
                     //pF.Segments.Add(new LineSegment() { Point = new Point(TopHeader().X * 10, TopHeader().Y * 10) });
                     pgeometry.Figures.Add(pF);
                         riskArea.Data = pgeometry;

                         props.rbID.Content = properties.pose.Position.X.ToString("0");
                         props.rbTask.Content = properties.pose.Position.Y.ToString("0");


                     }));
                }
            }

        }

        public override void UpdateProperties (PropertiesRobotUnity proR) {

            base.UpdateProperties (proR);
            DfL1 = proR.L1;
            DfL2 = proR.L2;
            DfWS = proR.WS;
            DfDistanceInter = proR.DistInter;

            DfL1Cv = proR.L1* properties.Scale;
            DfL2Cv = proR.L2* properties.Scale;
            DfWSCv = proR.WS* properties.Scale;
            DfDistInterCv = proR.DistInter* properties.Scale;

            L1Cv = proR.L1* properties.Scale;
            L2Cv = proR.L2* properties.Scale;
            WSCv = proR.WS* properties.Scale;
            DistInterCv = proR.DistInter*properties.Scale;         
            Draw ();

        }
    }
}
