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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SeldatMRMS.Management.RobotManagent
{
    public class RobotUnity : RobotBaseService
    {
        double angle = 0.0f;
        public Point org = new Point(600, 350);
        public double rad = 0;
        public double anglestep = 0;
        Rect area = new Rect(30, 30, 500, 500);
        Point loc = new Point(0, 0);
        public event Action<string> RemoveHandle;
        public struct Props
        {
            public string name;
            public bool isSelected;
            public bool isHovering;
            public Canvas canvas;
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

        public Props props;
        private Border border;
        public LoadedConfigureInformation loadConfigureInformation;
        public RobotUnity()
        {
            //th = new Thread(DrawCircle);
            //DrawCircle();
            //th.IsBackground = true;
            //th.Start();
           
        }
        public void Initialize(DataRow row)
        {
           
                properties.NameID = row.Field<string>("Robot");
                /*properties.URL = row.Field<string>("URL");
                properties.Width = double.Parse(row.Field<string>("Width"));
                properties.Height = double.Parse(row.Field<string>("Height"));
                properties.Length = double.Parse(row.Field<string>("Length"));
                properties.L1 = double.Parse(row.Field<string>("L1"));
                properties.L2 = double.Parse(row.Field<string>("L2"));
                properties.WS = double.Parse(row.Field<string>("WS"));
                properties.DistanceIntersection = double.Parse(row.Field<string>("Distance Intersection"));
                // double oriY = double.Parse(row.Field<string>("ORIGINAL").Split(',')[1]);
                loadConfigureInformation.IsLoadedStatus = true;*/
         
        }

        Canvas canvas;
        Ellipse ep;
        Ellipse ep1;
        Ellipse ep2;
        Ellipse ep3;
        Ellipse ep4;
        Ellipse ep5;
        Ellipse ep6;
        TextBlock textblock;
        double x = 0, y = 0;
        System.Windows.Threading.DispatcherTimer dispatcherTimer;
        public RobotUnity(Canvas canvas)
        {
            #region Timer1
            //dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            //dispatcherTimer.Tick += dispatcherTimer_Tick;
            //dispatcherTimer.Interval = new TimeSpan(5000000);
            #endregion


            this.canvas = canvas;
            /* properties.L1 = 30;
             properties.L2 = 30;
             properties.WS = 40;//40/2
             textblock = new TextBlock();

             ep = new Ellipse();
             ep.Width = 5;
             ep.Height = 5;
             ep.Name = "hello";
             ep.Fill = new SolidColorBrush(Colors.Red);

             ep1 = new Ellipse();
             ep1.Width = 5;
             ep1.Height = 5;
             ep1.Fill = new SolidColorBrush(Colors.Black);
             ep2 = new Ellipse();
             ep2.Width = 5;
             ep2.Height = 5;
             ep2.Fill = new SolidColorBrush(Colors.Blue);
             ep3 = new Ellipse();
             ep3.Width = 5;
             ep3.Height = 5;
             ep3.Fill = new SolidColorBrush(Colors.Red);
             ep4 = new Ellipse();
             ep4.Width = 5;
             ep4.Height = 5;
             ep4.Fill = new SolidColorBrush(Colors.Yellow);
             ep5 = new Ellipse();
             ep5.Width = 5;
             ep5.Height = 5;
             ep5.Fill = new SolidColorBrush(Colors.White);

             ep6 = new Ellipse();
             ep6.Width = 5;
             ep6.Height = 5;
             ep6.Fill = new SolidColorBrush(Colors.White);

             canvas.Children.Add(ep);
             canvas.Children.Add(ep1);
             canvas.Children.Add(ep2);
             canvas.Children.Add(ep3);
             canvas.Children.Add(ep4);
             canvas.Children.Add(ep5);
             // canvas.Children.Add(ep6);
             canvas.Children.Add(textblock);
             //  dispatcherTimer.Start();*/

            border = new Border();
            border.ToolTip = "";
            border.ToolTipOpening += ChangeToolTipContent;
            props.isSelected = false;
            props.isHovering = false;
            border.ContextMenu = new ContextMenu();
            //===================================
            MenuItem editItem = new MenuItem();
            editItem.Header = "Edit";
            editItem.Click += EditMenu;
            //===================================
            MenuItem removeItem = new MenuItem();
            removeItem.Header = "Remove";
            removeItem.Click += RemoveMenu;
            border.ContextMenu.Items.Add(editItem);
            border.ContextMenu.Items.Add(removeItem);
            //====================EVENT=====================
            //MouseLeave += MouseLeavePath;
            //MouseMove += MouseHoverPath;
            //MouseLeftButtonDown += MouseLeftButtonDownPath;
            //MouseRightButtonDown += MouseRightButtonDownPath;
            //===================CREATE=====================
            //Name = "Robotx" + Global_Mouse.EncodeTransmissionTimestamp();
            props.name = properties.NameID;
            props.mainGrid = new Grid();
            props.statusGrid = new Grid();
            props.statusBorder = new Border();
            props.rbID = new Label();
            props.rbTask = new Label();
            props.headLed = new Rectangle();
            props.tailLed = new Rectangle();
            props.eightCorner = new List<Point>();
            for (int i = 0; i < 8; i++)
            {
                Point temp = new Point();
                props.eightCorner.Add(temp);
            }
            props.rbRotateTransform = new RotateTransform();
            props.rbTranslate = new TranslateTransform();
            props.rbTransformGroup = new TransformGroup();
            props.contentRotateTransform = new RotateTransform();
            props.contentTranslate = new TranslateTransform();
            props.contentTransformGroup = new TransformGroup();
            // robotProperties = new Properties(this);
            //===================STYLE=====================
            //Robot border
            border.Width = 22;
            border.Height = 15;
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = new SolidColorBrush(Colors.Linen);
            border.Background = new SolidColorBrush(Colors.Black);
            border.CornerRadius = new CornerRadius(3);
            border.RenderTransformOrigin = new Point(0.5, 0.5);
            //mainGrid
            props.mainGrid.Background = new SolidColorBrush(Colors.Transparent);
            for (int i = 0; i < 3; i++)
            {
                ColumnDefinition colTemp = new ColumnDefinition();
                colTemp.Name = properties.NameID + "xL" + i;
                if ((i == 0) || (i == 2))
                {
                    colTemp.Width = new GridLength(1);
                }
                props.mainGrid.ColumnDefinitions.Add(colTemp);
            }
            //headLed
            props.headLed.Height = 7;
            props.headLed.Fill = new SolidColorBrush(Colors.DodgerBlue);
            Grid.SetColumn(props.headLed, 2);
            //tailLed
            props.tailLed.Height = 7;
            props.tailLed.Fill = new SolidColorBrush(Colors.OrangeRed);
            Grid.SetColumn(props.tailLed, 0);
            //statusBorder
            props.statusBorder.Width = 10;
            props.statusBorder.Height = 13;
            props.statusBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            Grid.SetColumn(props.statusBorder, 1);
            //statusGrid
            for (int i = 0; i < 2; i++)
            {
                RowDefinition rowTemp = new RowDefinition();
                rowTemp.Name = properties.NameID + "xR" + i;
                props.statusGrid.RowDefinitions.Add(rowTemp);
            }
            //rbID
            props.rbID.Padding = new Thickness(0);
            props.rbID.Margin = new Thickness(-5, 0, -5, 0);
            props.rbID.HorizontalAlignment = HorizontalAlignment.Center;
            props.rbID.VerticalAlignment = VerticalAlignment.Bottom;
            props.rbID.Content = "27";
            props.rbID.Foreground = new SolidColorBrush(Colors.Yellow);
            props.rbID.FontFamily = new FontFamily("Calibri");
            props.rbID.FontSize = 6;
            props.rbID.FontWeight = FontWeights.Bold;
            Grid.SetRow(props.rbID, 0);

            //rbTask
            props.rbTask.Padding = new Thickness(0);
            props.rbTask.Margin = new Thickness(-5, -1, -5, -1);
            props.rbTask.HorizontalAlignment = HorizontalAlignment.Center;
            props.rbTask.VerticalAlignment = VerticalAlignment.Top;
            props.rbTask.Content = "9999";
            props.rbTask.Foreground = new SolidColorBrush(Colors.LawnGreen);
            props.rbTask.FontFamily = new FontFamily("Calibri");
            props.rbTask.FontSize = 6;
            props.rbTask.FontWeight = FontWeights.Bold;
            Grid.SetRow(props.rbTask, 1);

            //===================CHILDREN===================
            props.statusGrid.Children.Add(props.rbID);
            props.statusGrid.Children.Add(props.rbTask);
            props.statusBorder.Child = props.statusGrid;
            props.mainGrid.Children.Add(props.headLed);
            props.mainGrid.Children.Add(props.tailLed);
            props.mainGrid.Children.Add(props.statusBorder);
            props.rbTransformGroup.Children.Add(props.rbRotateTransform);
            props.rbTransformGroup.Children.Add(props.rbTranslate);
            border.RenderTransform = props.rbTransformGroup;
            props.contentTransformGroup.Children.Add(props.contentRotateTransform);
            props.contentTransformGroup.Children.Add(props.contentTranslate);
            props.statusBorder.RenderTransform = props.contentTransformGroup;
            props.canvas = canvas;
            border.Child = props.mainGrid;
            props.canvas.Children.Add(border);

        }

        int state = 1;
        int count = 0;
        Random rnd = new Random();
        //private void dispatcherTimer_Tick(object sender, EventArgs e)
        //{

        //   // try
        //    {
        //        state = rnd.Next(1, 6);
        //        count = 0;
        //       // if (count++ > 20) count = 0;
        //        //if(test == 0)
        //        { switch (state)
        //            {
        //                case 1:
        //                   while(count++ <5)
        //                    this.LeftRobot();
        //                    break;
        //                case 2:
        //                    while (count++ < 5)
        //                        this.RightRobot();
        //                    break;
        //                case 3:
        //                    while (count++ < 5)
        //                        this.UpRobot();
        //                    break;
        //                case 4:
        //                    while (count++ < 5)
        //                        this.DownRobot();
        //                    break;
        //                case 5:
        //                    while (count++ < 5)
        //                        this.RotationLeft();
        //                    break;
        //                case 6:
        //                    while (count++ < 5)
        //                       this.RotationRight();
        //                    break;
        //            }       
        //        }
        //       // count = 0;
        //    }
        //   // catch { }
        //}
        public void initialPos(double xx, double yy)
        {
            x = xx;
            y = yy;
            ep.RenderTransform = new TranslateTransform(xx, yy);
            setConner(new Point(xx, yy), angle);
            textblock.Text = this.properties.NameID;
            textblock.FontSize = 8;
            textblock.RenderTransform = new TranslateTransform(xx + 5, yy);
        }
        public void setConner(Point p, double angle)
        {


            textblock.RenderTransform = new TranslateTransform(p.X + 5, p.Y);
            properties.pose.Position = p;
            properties.pose.AngleW = angle;
            ep1.RenderTransform = new TranslateTransform(TopHeader().X, TopHeader().Y);
            ep2.RenderTransform = new TranslateTransform(BottomHeader().X, BottomHeader().Y);
            ep3.RenderTransform = new TranslateTransform(TopTail().X, TopTail().Y);
            ep4.RenderTransform = new TranslateTransform(BottomTail().X, BottomTail().Y);
            ep5.RenderTransform = new TranslateTransform(MiddleHeader().X, MiddleHeader().Y);
            ep6.RenderTransform = new TranslateTransform(MiddleTail().X, MiddleTail().Y);
            //Canvas.SetLeft(ep1, TopHeader().X);
            // Canvas.SetTop(ep1, TopHeader().Y);
            //Canvas.SetLeft(ep2, BottomHeader().X);
            // Canvas.SetTop(ep2, BottomHeader().Y);
            // Canvas.SetLeft(ep3, TopTail().X);
            // Canvas.SetTop(ep3, TopTail().Y);
            // Canvas.SetLeft(ep4, BottomTail().X);
            // Canvas.SetTop(ep4, BottomTail().Y);



        }
        public void UpRobot()
        {

            if (y > 0)
            {
                y = y - ep.Width;
            }
            ep.RenderTransform = new TranslateTransform(x, y);
            setConner(new Point(x, y), angle);
            SupervisorTraffic();
        }
        public void DownRobot()
        {
            if (y < canvas.Height)
                y = y + ep.Width;
            ep.RenderTransform = new TranslateTransform(x, y);
            setConner(new Point(x, y), angle);
            SupervisorTraffic();
        }
        public void LeftRobot()
        {

            if (x > 0)
            {
                x = x - ep.Width;
            }
            ep.RenderTransform = new TranslateTransform(x, y);
            setConner(new Point(x, y), angle);
            SupervisorTraffic();
        }
        public void RightRobot()
        {
            if (x < canvas.Width)
                x = x + ep.Width;
            ep.RenderTransform = new TranslateTransform(x, y);
            setConner(new Point(x, y), angle);
            SupervisorTraffic();
        }

        public void RotationLeft()
        {
            if (angle > -Math.PI)
            {
                angle = angle - 5 * Math.PI / 180;
            }
            ep.RenderTransform = new TranslateTransform(x, y);
            setConner(new Point(x, y), angle);
            SupervisorTraffic();
        }

        public void RotationRight()
        {
            if (angle < Math.PI)
            {
                angle = angle + 5 * Math.PI / 180;
            }
            ep.RenderTransform = new TranslateTransform(x, y);
            setConner(new Point(x, y), angle);
            SupervisorTraffic();
        }
        private void ChangeToolTipContent(object sender, ToolTipEventArgs e)
        {
            border.ToolTip = "1234567890";
        }
        private void EditMenu(object sender, RoutedEventArgs e)
        {
            //robotProperties.ShowDialog();
        }

        private void RemoveMenu(object sender, RoutedEventArgs e)
        {
            Remove();
        }
        public void Remove()
        {
            props.canvas.Children.Remove(border);
            RemoveHandle(props.name);
        }      
        public Point CirclePoint(double radius, double angleInDegrees, Point origin)
        {
            double x = (double)(radius * Math.Cos(angleInDegrees * Math.PI / 180)) + origin.X;
            double y = (double)(radius * Math.Sin(angleInDegrees * Math.PI / 180)) + origin.Y;

            return new Point(x, y);
        }
        public void Draw()
        {
           
                //Render Robot
                props.rbRotateTransform.Angle = properties.pose.AngleW;
                props.rbTranslate = new TranslateTransform(properties.pose.Position.X - (border.Width / 2), properties.pose.Position.Y - (border.Height / 2));
                props.rbTransformGroup.Children[1] = props.rbTranslate;
                //Render Status
                props.contentRotateTransform.Angle = -(properties.pose.AngleW);
                props.contentTranslate = new TranslateTransform(0, 0);
                props.contentTransformGroup.Children[1] = props.contentTranslate;
                // SPECIAL POINTS
                //props.eightCorner[0] = CoorPointAtBorder(new Point((0), (Height / 2)));          //mid-left
                //props.eightCorner[1] = CoorPointAtBorder(new Point((0), (0)));                 //top-left
                //props.eightCorner[2] = CoorPointAtBorder(new Point((Width / 2), (0)));           //top-mid
                //props.eightCorner[3] = CoorPointAtBorder(new Point((Width), (0)));             //top-right
                //props.eightCorner[4] = CoorPointAtBorder(new Point((Width), (Height / 2)));      //mid-right
                //props.eightCorner[5] = CoorPointAtBorder(new Point((Width), (Height)));        //bot-right
                //props.eightCorner[6] = CoorPointAtBorder(new Point((Width / 2), (Height)));      //bot-mid
                //props.eightCorner[7] = CoorPointAtBorder(new Point((0), (Height)));            //bot-left
       
        }

    }
}
