using SeldatMRMS.Communication;
using SelDatUnilever_Ver1._00.Management;
using SelDatUnilever_Ver1._00.Management.ChargerCtrl;
using System;
using System.ComponentModel;
using System.Windows;
using WebSocketSharp;

namespace SeldatMRMS.Management.RobotManagent
{

    public class RobotUnityControl : RosSocket
    {
        
        public event Action<int> FinishStatesCallBack;
        public event Action<LaserErrorCode> AGVLaserErrorCallBack;
        public event Action<LaserWarningCode> AGVLaserWarningCallBack;
        public event Action<Pose, Object> PoseHandler;
        public event Action<Object, ConnectionStatus> ConnectionStatusHandler;

        private const float delBatterry = 2;
        public class Pose
        {
            public Pose(Point p, double Angle) // Angle gốc
            {
                this.Position = p;
                this.AngleW = Angle * Math.PI / 180.0;
                this.Angle = Angle;
            }
            public Pose (double X, double Y, double Angle) // Angle gốc
            {
                this.Position = new Point(X, Y);
                this.AngleW = Angle*Math.PI/180.0;
                this.Angle = Angle;
            }
            public Pose () { }
            public void Destroy () // hủy vị trí robot để robot khác có thể làm việc trong quá trình detect
            {
                //this.Position = new Point (-1000, -1000);
                //this.AngleW = 0;
            }
            public Point Position { get; set; }
            public double AngleW { get; set; } // radian
            public double Angle { get; set; } // radian
        }
        public enum RobotSpeedLevel {
            ROBOT_SPEED_NORMAL = 100,
            ROBOT_SPEED_SLOW = 50,
            ROBOT_SPEED_STOP = 0,
        }
        public bool getBattery () {
            return false;// properties.RequestChargeBattery;
        }

        public class PropertiesRobotUnity : NotifyUIBase
        {
            [CategoryAttribute("ID Settings"), DescriptionAttribute("Name of the customer")]
            private String _NameId;
            public String NameId { get => _NameId; set { _NameId = value; RaisePropertyChanged("NameID"); } }
            private String _Label;
            public String Label { get => _Label; set { _Label = value; RaisePropertyChanged("Label"); } }
            private String _Url { get; set; }
            public String Url { get => _Url; set { _Url = value; RaisePropertyChanged("Url"); } }
            public Pose pose= new Pose();
            public String URL;
            public bool IsConnected { get; set; }
            private double _L1 { get; set; }
            private double _L2 { get; set; }
            private double _WS { get; set; }
            private double _Width { get; set; }
            private double _Length { get; set; }
            private double _Height { get; set; }
            [CategoryAttribute("Laser"), DescriptionAttribute("Name of the customer")]
            public String LaserOperation;
            [CategoryAttribute("Battery"), DescriptionAttribute("Name of the customer")]
            private float _BatteryLevelRb;
            public float BatteryLevelRb { get => _BatteryLevelRb; set { _BatteryLevelRb = value; RaisePropertyChanged("BatteryLevelRb"); } }
            private float _BatteryLowLevel;
            public float BatteryLowLevel { get => _BatteryLowLevel; set { _BatteryLowLevel = value; RaisePropertyChanged("BatteryLowLevel"); } }
            public bool RequestChargeBattery;
            private ChargerCtrl.ChargerId _ChargeID;
            public ChargerCtrl.ChargerId ChargeID { get => _ChargeID; set { _ChargeID = value; RaisePropertyChanged("ChargeID"); } }
            private double _DistanceIntersection;
            public double DistInter { get => _DistanceIntersection; set { _DistanceIntersection = value; RaisePropertyChanged("Distance Intersection"); } }
            public double L1 { get => _L1; set { _L1 = value; RaisePropertyChanged("L1"); } }
            public double L2 { get => _L2; set { _L2 = value; RaisePropertyChanged("L2"); } }
            public double WS { get => _WS; set { _WS = value; RaisePropertyChanged("WS"); } }


            private double _Scale { get; set; }
            public double Scale { get => _Scale; set { _Scale = value; RaisePropertyChanged("Scale"); } }

            public double Width { get => _Width; set { _Width = value; RaisePropertyChanged("Width"); } }
            public double Length { get => _Length; set { _Length = value; RaisePropertyChanged("Length"); } }
            public double Height { get => _Height; set { _Height = value; RaisePropertyChanged("Height"); } }
            public String problemContent;
            public String solvedProblemContent;
            public String detailInfo;
            private String _ipMcuCtrl;
            public String ipMcuCtrl{ get => _ipMcuCtrl; set { _ipMcuCtrl = value; RaisePropertyChanged("IpMCU"); } }
            private int _portMcuCtrl;
            public int portMcuCtrl{ get => _portMcuCtrl; set {_portMcuCtrl = value; RaisePropertyChanged("PortMCU"); } }
        }

        public enum RequestCommandLineDetect {
            REQUEST_CHARGECTRL_CANCEL = 1201,
            REQUEST_LINEDETECT_PALLETUP = 1203,
            REQUEST_LINEDETECT_PALLETDOWN = 1204,
            // REQUEST_LINEDETECT_GETIN_CHARGER = 1206,
            // REQUEST_LINEDETECT_GETOUT_CHARGER = 1207,
            REQUEST_LINEDETECT_READYAREA = 1208,
        }

        public enum RequestCommandPosPallet{

            REQUEST_LINEDETECT_COMING_POSITION = 1205,
            REQUEST_TURN_LEFT = 1210,
            REQUEST_TURN_RIGHT = 1211,
            REQUEST_FORWARD_DIRECTION = 1212,
            REQUEST_GOBACK_FRONTLINE = 1213,
            REQUEST_TURNOFF_PC = 1214
        }

        public enum ResponseCommand
        {
            RESPONSE_NONE = 0,
            RESPONSE_LASER_CAME_POINT = 2000,
            RESPONSE_LINEDETECT_PALLETUP = 3203,
            RESPONSE_LINEDETECT_PALLETDOWN = 3204,
            RESPONSE_FINISH_GOTO_POSITION = 3205,
            RESPONSE_FINISH_DETECTLINE_GETIN_CHARGER = 3206,
            RESPONSE_FINISH_DETECTLINE_GETOUT_CHARGER = 3207,
            RESPONSE_FINISH_TURN_LEFT = 3210,
            RESPONSE_FINISH_TURN_RIGHT = 3211,
            RESPONSE_FINISH_GOBACK_FRONTLINE = 3213,
            RESPONSE_ERROR = 3215
        }

        public virtual void updateparams () { }
        public virtual void OnOccurencyTrigger () { }
        public virtual void OnBatteryLowTrigger () { }
        public struct ParamsRosSocket {
            public int publication_RobotInfo;
            public int publication_RobotParams;
            public int publication_ServerRobotCtrl;
            public int publication_CtrlRobotHardware;
            public int publication_DriveRobot;
            public int publication_BatteryRegister;
            public int publication_EmergencyRobot;
            public int publication_ctrlrobotdriving;
            public int publication_robotnavigation;
            public int publication_linedetectionctrl;
            public int publication_checkAliveTimeOut;
            public int publication_postPallet;
            public int publication_cmdAreaPallet;

            /*of chau test*/
            public int publication_finishedStates;
            public int publication_batteryvol;
            public int publication_TestLaserError;
            public int publication_TestLaserWarning;
        }

        public struct LaserErrorCode {
            public bool LaserErrorConnect;
            public bool LaserErrorShutdown;
            public bool LaserErrorLostSpeed;
            public bool LaserErrorLostPath;
        }

        public struct LaserWarningCode {
            public bool LaserWarningObstacle;
            public bool LaserWarningLowBattey;
            public bool LaserWarningCharging;
            public bool LaserWarningHazardoes;
            public bool LaserWarningBackward;
        }

        ParamsRosSocket paramsRosSocket;
        public PropertiesRobotUnity properties = new PropertiesRobotUnity();
        protected virtual void SupervisorTraffic() { }
        public RobotUnityControl()
        {

        }
        public void createRosTerms () {
            int subscription_robotInfo = this.Subscribe ("/amcl_pose", "geometry_msgs/PoseWithCovarianceStamped", AmclPoseHandler);
            paramsRosSocket.publication_ctrlrobotdriving = this.Advertise ("/ctrlRobotDriving", "std_msgs/Int32");
            int subscription_finishedStates = this.Subscribe ("/finishedStates", "std_msgs/Int32", FinishedStatesHandler);
            paramsRosSocket.publication_robotnavigation = this.Advertise ("/robot_navigation", "geometry_msgs/PoseStamped");
            paramsRosSocket.publication_checkAliveTimeOut = this.Advertise ("/checkAliveTimeOut", "std_msgs/String");
            paramsRosSocket.publication_linedetectionctrl = this.Advertise ("/linedetectionctrl", "std_msgs/Int32");
            paramsRosSocket.publication_postPallet = this.Advertise ("/pospallet", "std_msgs/Int32");
            paramsRosSocket.publication_cmdAreaPallet = this.Advertise ("/cmdAreaPallet", "std_msgs/String");
            float subscription_publication_batteryvol = this.Subscribe ("/battery_vol", "std_msgs/Int32", BatteryVolHandler);
            int subscription_AGV_LaserError = this.Subscribe ("/AGV_LaserError", "std_msgs/String", AGVLaserErrorHandler);
            int subscription_AGV_LaserWarning = this.Subscribe ("/AGV_LaserWarning", "std_msgs/String", AGVLaserWarningHandler);
            /*of chau test*/
          //  paramsRosSocket.publication_finishedStates = this.Advertise ("/finishedStates", "std_msgs/Int32");
            //paramsRosSocket.publication_batteryvol = this.Advertise ("/battery_vol", "std_msgs/Float32");
         //   paramsRosSocket.publication_TestLaserError = this.Advertise ("/AGV_LaserError", "std_msgs/String");
          //  paramsRosSocket.publication_TestLaserWarning = this.Advertise ("/AGV_LaserWarning", "std_msgs/String");
        }

        private void BatteryVolHandler (Communication.Message message) {
            StandardInt32 batVal = (StandardInt32) message;
            properties.BatteryLevelRb = batVal.data;
            if (properties.RequestChargeBattery == false) {
                if (properties.BatteryLevelRb < properties.BatteryLowLevel) {
                    properties.RequestChargeBattery = true;
                }
            } else {
                if (properties.BatteryLevelRb > (properties.BatteryLowLevel + delBatterry)) {
                    properties.RequestChargeBattery = false;
                }
            }
        }

        private void AmclPoseHandler (Communication.Message message) {
            GeometryPoseWithCovarianceStamped standardString = (GeometryPoseWithCovarianceStamped) message;
            double posX = (double) standardString.pose.pose.position.x;
            double posY = (double) standardString.pose.pose.position.y;
            double posThetaZ = (double) standardString.pose.pose.orientation.z;
            double posThetaW = (double) standardString.pose.pose.orientation.w;
            double posTheta = (double) 2 * Math.Atan2 (posThetaZ, posThetaW);
            properties.pose.Position = new Point (posX, posY);
            properties.pose.AngleW = posTheta;
            properties.pose.Angle = posTheta * 180 / Math.PI;
            Draw ();
            TrafficUpdate();


        }
        private void FinishedStatesHandler (Communication.Message message) {
            try
            {
                StandardInt32 standard = (StandardInt32)message;
                FinishStatesCallBack(standard.data);
            }
            catch { }

        }
        private void AGVLaserErrorHandler (Communication.Message message) {
            StandardString standard = (StandardString) message;
            LaserErrorCode er = new LaserErrorCode ();
            bool tamddd = standard.data[0].Equals('1');
            try
            {
                if (standard.data[0].Equals('1')) {
                    er.LaserErrorConnect = true;
                } else {
                    er.LaserErrorConnect = false;
                }
                if (standard.data[1].Equals ('1')) {
                    er.LaserErrorShutdown = true;
                } else {
                    er.LaserErrorShutdown = false;
                }
                if (standard.data[2].Equals ('1')) {
                    er.LaserErrorLostSpeed = true;
                } else {
                    er.LaserErrorLostSpeed = false;
                }
                if (standard.data[3].Equals ('1')) {
                    er.LaserErrorLostPath = true;
                } else {
                    er.LaserErrorLostPath = false;
                }
            } catch (System.Exception) {
                Console.WriteLine ("Cannot parse error laser");
            }
            // AGVLaserErrorCallBack (er);
        }

        private void AGVLaserWarningHandler (Communication.Message message) {
            StandardString standard = (StandardString) message;
            LaserWarningCode war = new LaserWarningCode ();
            try {
                if (standard.data[0].Equals ('1')) {
                    war.LaserWarningObstacle = true;
                } else {
                    war.LaserWarningObstacle = false;
                }
                if (standard.data[1].Equals ('1')) {
                    war.LaserWarningLowBattey = true;
                } else {
                    war.LaserWarningLowBattey = false;
                }
                if (standard.data[2].Equals ('1')) {
                    war.LaserWarningCharging = true;
                } else {
                    war.LaserWarningCharging = false;
                }
                if (standard.data[3].Equals ('1')) {
                    war.LaserWarningHazardoes = true;
                } else {
                    war.LaserWarningHazardoes = false;
                }
                if (standard.data[4].Equals ('1')) {
                    war.LaserWarningHazardoes = true;
                } else {
                    war.LaserWarningHazardoes = false;
                }
            } catch (System.Exception) {
                Console.WriteLine ("Cannot parse warning laser");
            }
            // AGVLaserWarningCallBack (war);
        }

        public void TestLaserError (String cmd) {
            StandardString msg = new StandardString ();
            msg.data = cmd;
            this.Publish (paramsRosSocket.publication_TestLaserError, msg);
        }
        public void TestLaserWarning (String cmd) {
            StandardString msg = new StandardString ();
            msg.data = cmd;
            this.Publish (paramsRosSocket.publication_TestLaserWarning, msg);
        }

        public void FinishedStatesPublish (int message) {
            StandardInt32 msg = new StandardInt32 ();
            msg.data = message;
            this.Publish (paramsRosSocket.publication_finishedStates, msg);
        }

        public void BatteryPublish (float message) {
            StandardFloat32 msg = new StandardFloat32 ();
            msg.data = message;
            this.Publish (paramsRosSocket.publication_batteryvol, msg);
        }
        public virtual void UpdateProperties(PropertiesRobotUnity proR)
        {
            properties = proR;
        }

        public void SendPoseStamped (Pose pose) {
            GeometryPoseStamped data = new GeometryPoseStamped ();
            data.header.frame_id = "map";
            data.pose.position.x = (float) pose.Position.X;
            data.pose.position.y = (float) pose.Position.Y;
            data.pose.position.z = 0;
            double theta = pose.AngleW;
            data.pose.orientation.z = (float) Math.Sin (theta / 2);
            data.pose.orientation.w = (float) Math.Cos (theta / 2);
            this.Publish (paramsRosSocket.publication_robotnavigation, data);
        }
        public void SetSpeed (RobotSpeedLevel robotspeed) {
            StandardInt32 msg = new StandardInt32 ();
            msg.data = Convert.ToInt32 (robotspeed);
            this.Publish (paramsRosSocket.publication_ctrlrobotdriving, msg);
        }

        public void SendCmdLineDetectionCtrl (RequestCommandLineDetect cmd) {
            StandardInt32 msg = new StandardInt32 ();
            msg.data = Convert.ToInt32 (cmd);
            this.Publish (paramsRosSocket.publication_linedetectionctrl, msg);
        }

        public void SendCmdPosPallet (RequestCommandPosPallet cmd) {
            StandardInt32 msg = new StandardInt32 ();
            msg.data = Convert.ToInt32 (cmd);
            this.Publish (paramsRosSocket.publication_postPallet, msg);
        }
        public void SendCmdAreaPallet (String cmd) {
            StandardString msg = new StandardString ();
            msg.data = cmd;
            Console.WriteLine(cmd);
            this.Publish (paramsRosSocket.publication_cmdAreaPallet, msg);
        }

        protected override void OnOpenedEvent () {
            properties.IsConnected = true;
            Console.WriteLine ("connected Robot");
            createRosTerms ();
            
            //   ConnectionStatusHandler(this, ConnectionStatus.CON_OK);
        }

        protected override void OnClosedEvent (object sender, CloseEventArgs e) {
            //ConnectionStatusHandler(this, ConnectionStatus.CON_FAILED);
            properties.IsConnected = false;
            this.url = properties.URL;
            base.OnClosedEvent (sender, e);

        }

        public override void Dispose () {
            properties.pose.Destroy ();
            base.Dispose ();
        }
        public virtual void Draw () { }
        public virtual void TrafficUpdate() { }
    }
}
