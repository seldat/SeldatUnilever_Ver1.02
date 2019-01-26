using SeldatMRMS.RobotView;
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
        public SolvedProblem solvedProblem;
        public Robot3D robot3DModel = null;
        public RobotUnity(){
            solvedProblem = new SolvedProblem();

        }

        public void Initialize(Canvas canvas)
        {
            this.canvas = canvas;
            //ModelVisual3D layer = new ModelVisual3D();
           // robot3DModel = new Robot3D(properties.NameId, layer);
            riskArea = new Path ();
            riskArea.Stroke = new SolidColorBrush (Colors.YellowGreen);
            riskArea.StrokeThickness = 1;
            border = new Border ();
            border.ToolTip = "";
            border.ToolTipOpening += ChangeToolTipContent;
            props.isSelected = false;
            props.isHovering = false;
            border.ContextMenu = new ContextMenu ();
            
            MenuItem problemSolutionItem = new MenuItem();
            problemSolutionItem.Header = "Problem Solution";
            problemSolutionItem.Click += PoblemSolutionItem;
            //===================================
            MenuItem startItem = new MenuItem();
            startItem.Header = "Start";
            startItem.Click += StartMenu;
            //===================================
            MenuItem pauseItem = new MenuItem();
            pauseItem.Header = "Pause";
            pauseItem.Click += PauseMenu;

            MenuItem mainMenuWorkList = new MenuItem();
            mainMenuWorkList.Header = "Registry WorkList";

            MenuItem itemw0 = new MenuItem();
            itemw0.Header = "Ready Area";
            mainMenuWorkList.Items.Add(itemw0);

            MenuItem itemw1 = new MenuItem();
            itemw1.Header = "Wait Task";
            mainMenuWorkList.Items.Add(itemw1);

            MenuItem mainMenuProc = new MenuItem();
            mainMenuProc.Header = "Registry Procedure";

            MenuItem item0 = new MenuItem();
            item0.Header = "All";
            mainMenuProc.Items.Add(item0);
            item0.IsCheckable = true;
            MenuItem item1 = new MenuItem();
            item1.Header = "ForkLift To Buffer";
            mainMenuProc.Items.Add(item1);
            item0.IsCheckable = false;
            MenuItem item2 = new MenuItem();
            item2.Header = "Buffer To Machine";
            mainMenuProc.Items.Add(item2);
            MenuItem item3 = new MenuItem();
            item3.Header = "Buffer To Return";
            mainMenuProc.Items.Add(item3);
            MenuItem item4 = new MenuItem();
            item4.Header = "Return To Gate";
            mainMenuProc.Items.Add(item4);







            border.ContextMenu.Items.Add(problemSolutionItem);
            border.ContextMenu.Items.Add(startItem);
            border.ContextMenu.Items.Add(pauseItem);
            border.ContextMenu.Items.Add(mainMenuWorkList);
            border.ContextMenu.Items.Add(mainMenuProc);

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
            props.headLed.Fill = new SolidColorBrush (Colors.DodgerBlue);
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
        public void RegistrySolvedForm(Object obj)
        {
          //  if(obj.GetType()==typeof(ProcedureControlServices))
            {
                solvedProblem.Registry(obj);
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

            //Render Robot
            props.rbRotateTransform.Angle = properties.pose.AngleW;
            Point cPoint = Global_Object.CoorCanvas (properties.pose.Position);
            props.rbTranslate = new TranslateTransform (cPoint.X - (border.Width / 2), cPoint.Y - (border.Height / 2));
            props.rbTransformGroup.Children[1] = props.rbTranslate;
            //Render Status
            props.contentRotateTransform.Angle = -(properties.pose.AngleW);
            props.contentTranslate = new TranslateTransform (0, 0);
            props.contentTransformGroup.Children[1] = props.contentTranslate;
            headerPoint.RenderTransform = new TranslateTransform (MiddleHeader ().X, MiddleHeader ().Y);
            headerPoint.RenderTransform = new TranslateTransform (Global_Object.CoorCanvas (MiddleHeader ()).X, Global_Object.CoorCanvas (MiddleHeader ()).Y);
            PathGeometry pgeometry = new PathGeometry ();
            PathFigure pF = new PathFigure ();
            pF.StartPoint = TopHeaderCv ();
            LineSegment pp = new LineSegment ();

            pF.Segments.Add (new LineSegment () { Point = BottomHeaderCv () });
            pF.Segments.Add (new LineSegment () { Point = BottomTailCv () });
            pF.Segments.Add (new LineSegment () { Point = TopTailCv () });
            pF.Segments.Add (new LineSegment () { Point = TopHeaderCv () });
            pgeometry.Figures.Add (pF);
            riskArea.Data = pgeometry;
        }

        public override void UpdateProperties (PropertiesRobotUnity proR) {

            base.UpdateProperties (proR);
            DfL1 = proR.L1;
            DfL2 = proR.L2;
            DfWS = proR.WS;
            DfDistanceInter = proR.DistInter;

            DfL1Cv = proR.L1 / properties.Scale;
            DfL2Cv = proR.L2 / properties.Scale;
            DfWSCv = proR.WS / properties.Scale;
            DfDistanceInter = proR.DistInter / properties.Scale;

            L1Cv = proR.L1 / properties.Scale;
            L2Cv = proR.L2 / properties.Scale;
            WSCv = proR.WS / properties.Scale;
            DistInterCv = proR.DistInter / properties.Scale;         
            Draw ();

        }
    }
}
