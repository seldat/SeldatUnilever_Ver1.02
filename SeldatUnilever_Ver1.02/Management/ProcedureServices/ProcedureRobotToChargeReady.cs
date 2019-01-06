using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using SelDatUnilever_Ver1._00.Management.ChargerCtrl;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SelDatUnilever_Ver1._00.Management.ChargerCtrl.ChargerCtrl;
using static SelDatUnilever_Ver1._00.Management.ComSocket.RouterComPort;

namespace SeldatMRMS
{
    public class ProcedureRobotToCharger : ProcedureControlServices
    {

        Thread ProRobotToCharger;
        RobotUnity robot;
        ResponseCommand resCmd;
        RobotGoToCharge StateRobotToCharge;
        ChargerCtrl chargerCtrl;
        DataReceive batLevel;
        DataReceive statusCharger;
        const UInt32 TIME_OUT_WAIT_STATE = 60000;
        const UInt32 TIME_OUT_ROBOT_RECONNECT_SERVER = 180000;
        public override event Action<Object> ReleaseProcedureHandler;
        public override event Action<Object> ErrorProcedureHandler;
        public byte getBatteryLevel(){
            return batLevel.data[0];
        }
        public byte getStatusCharger(){
            return statusCharger.data[0];
        }
        public ProcedureRobotToCharger(RobotUnity robot,ChargerManagementService charger,ChargerId id) : base(robot)
        {
            StateRobotToCharge = RobotGoToCharge.ROBCHAR_IDLE;
            batLevel = new DataReceive();
            statusCharger = new DataReceive();
            this.robot = robot;
            chargerCtrl = charger.ChargerStationList[id];

            //ChargerId id_t = id;
            //switch (id_t)
            //{
            //    case ChargerId.CHARGER_ID_1:
            //        chargerCtrl = charger.ChargerStation_1;
            //        break;
            //    case ChargerId.CHARGER_ID_2:
            //        chargerCtrl = charger.ChargerStation_2;
            //        break;
            //    case ChargerId.CHARGER_ID_3:
            //        chargerCtrl = charger.ChargerStation_3;
            //        break;
            //    default: break;
            //}
            procedureCode = ProcedureCode.PROC_CODE_ROBOT_TO_CHARGE;
        }
        public void Start(RobotGoToCharge state = RobotGoToCharge.ROBCHAR_ROBOT_GOTO_CHARGER)
        {
            errorCode = ErrorCode.RUN_OK;
            robot.ProcedureAs = ProcedureControlAssign.PRO_CHARGE;
            StateRobotToCharge = state;
            ProRobotToCharger = new Thread(this.Procedure);
            ProRobotToCharger.Start(this);
            ProRun = true;
        }
        public void Destroy()
        {
            // StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_RELEASED;
            ProRun = false;
        }
        public void Procedure(object ojb)
        {
            ProcedureRobotToCharger RbToChar = (ProcedureRobotToCharger)ojb;
            RobotUnity rb = RbToChar.robot;
            while (ProRun)
            {
                switch (StateRobotToCharge)
                {
                    case RobotGoToCharge.ROBCHAR_IDLE: break;
                    // case RobotGoToCharge.ROBCHAR_CHARGER_CHECKSTATUS:
                    //     if(true == chargerCtrl.WaitState(ChargerState.ST_READY,TIME_OUT_WAIT_STATE)){
                    //         StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_ALLOW_CUTOFF_POWER_ROBOT;
                    //     }
                    //     break; //kiểm tra kết nối và trạng thái sạc
                    case RobotGoToCharge.ROBCHAR_ROBOT_GOTO_CHARGER:
                        rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_CHARGEAREA);
                        break;
                    case RobotGoToCharge.ROBCHAR_ROBOT_START_CHARGE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOTO_POSITION)
                        {
                            chargerCtrl.StartCharge();
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_WAITTING_ROBOT_CONTACT_CHARGER;
                        }
                        else if(resCmd == ResponseCommand.RESPONSE_ERROR){
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_RELEASED;    
                        }
                        break;
                    case RobotGoToCharge.ROBCHAR_WAITTING_ROBOT_CONTACT_CHARGER: 
                        if (true == chargerCtrl.WaitState(ChargerState.ST_CHARGING,TIME_OUT_WAIT_STATE))
                        {
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_ALLOW_CUTOFF_POWER_ROBOT;
                        }
                        else if (true == chargerCtrl.WaitState(ChargerState.ST_ERROR,TIME_OUT_WAIT_STATE))
                        {
                            errorCode = ErrorCode.CONTACT_CHARGER_ERROR;
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_RELEASED;     
                        }
                        break; //robot tiep xuc tram sac        
                    case RobotGoToCharge.ROBCHAR_ROBOT_ALLOW_CUTOFF_POWER_ROBOT:
                        rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_TURNOFF_PC);
                        StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_WAITTING_CUTOFF_POWER_PC;
                        break; //cho phép cắt nguồn robot
                    case RobotGoToCharge.ROBCHAR_ROBOT_WAITTING_CUTOFF_POWER_PC:
                        if(true != rb.properties.IsConnected){
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_WAITTING_CHARGEBATTERY;    
                        }
                        break;
                    case RobotGoToCharge.ROBCHAR_WAITTING_CHARGEBATTERY:
                        if (true == chargerCtrl.GetBatteryAndStatus(ref rb,ref batLevel,ref statusCharger))
                        {
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_FINISHED_CHARGEBATTERY;
                        }
                        rb.properties.BatteryLevelRb = (float)batLevel.data[0];
                        break; //dợi charge battery và thông tin giao tiếp server và trạm sạc
                    case RobotGoToCharge.ROBCHAR_FINISHED_CHARGEBATTERY:
                        StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_WAITING_RECONNECTING;    
                        break; //Hoàn Thành charge battery và thông tin giao tiếp server và trạm sạc
                    case RobotGoToCharge.ROBCHAR_ROBOT_WAITING_RECONNECTING:
                        if(true == CheckReconnectServer(TIME_OUT_ROBOT_RECONNECT_SERVER)){
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_STATUS_GOOD_OPERATION;  
                        }
                        else{
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_STATUS_BAD_OPERATION;
                        }
                        break; //Robot mở nguồng và đợi connect lại
                    case RobotGoToCharge.ROBCHAR_ROBOT_STATUS_GOOD_OPERATION: 
                        StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_RELEASED;
                        break;
                    case RobotGoToCharge.ROBCHAR_ROBOT_STATUS_BAD_OPERATION: 
                        StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_RELEASED;
                        break;
                    case RobotGoToCharge.ROBCHAR_ROBOT_RELEASED:
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_CHARGE;
                        if(errorCode == ErrorCode.RUN_OK){
                            ReleaseProcedureHandler(this);
                        }
                        else{
                            ErrorProcedureHandler(this);    
                        }
                        ProRun = false;
                        break; // trả robot về robotmanagement để nhận quy trình mới
                    default:
                        break;
                }
                Thread.Sleep(5);
            }
            StateRobotToCharge = RobotGoToCharge.ROBCHAR_IDLE;
        }

        private bool CheckReconnectServer(UInt32 timeOut)
        {
            bool result = true;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            do
            {
                Thread.Sleep(1000);
                if(sw.ElapsedMilliseconds > timeOut){
                    result = false;
                    break;
                }
            } while (true != robot.properties.IsConnected);
            sw.Stop();
            return result;
        }
        public override void FinishStatesCallBack(Int32 message)
        {
            this.resCmd = (ResponseCommand)message;
        }
    }
    public class ProcedureRobotToReady : ProcedureControlServices
    {
        public struct DataRobotToReady
        {
            public Pose PointFrontLine;
            public String PointOfCharger;
        }
        DataRobotToReady points;
        List<DataRobotToReady> DataRobotToReadyList;
        Thread ProRobotToReady;
        RobotUnity robot;
        ResponseCommand resCmd;
        RobotGoToReady StateRobotGoToReady;
        TrafficManagementService Traffic;
        public override event Action<Object> ReleaseProcedureHandler;
        public override event Action<Object> ErrorProcedureHandler;
        public ProcedureRobotToReady(RobotUnity robot,ChargerId id,TrafficManagementService trafficService) : base(robot)
        {
            StateRobotGoToReady = RobotGoToReady.ROBREA_IDLE;
            this.robot = robot;
            this.Traffic = trafficService;
            LoadChargerConfigure();
            points = DataRobotToReadyList[(int)id - 1];
            procedureCode = ProcedureCode.PROC_CODE_ROBOT_TO_READY;
        }
        public void LoadChargerConfigure()
        {
            string name = "Charger";
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Configure.xlsx");
            string constr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                            path +
                            ";Extended Properties='Excel 12.0 XML;HDR=YES;';";
            OleDbConnection con = new OleDbConnection(constr);
            OleDbCommand oconn = new OleDbCommand("Select * From [" + name + "$]", con);
            con.Open();

            OleDbDataAdapter sda = new OleDbDataAdapter(oconn);
            DataTable data = new DataTable();
            sda.Fill(data);
            DataRobotToReadyList  = new List<DataRobotToReady>();
            foreach (DataRow row in data.Rows)
            {
                DataRobotToReady ptemp = new DataRobotToReady();
                ptemp.PointFrontLine = new Pose(double.Parse(row.Field<String>("PointFrontLine").Split(',')[0]),
                                                double.Parse(row.Field<String>("PointFrontLine").Split(',')[1]),
                                                double.Parse(row.Field<String>("PointFrontLine").Split(',')[2]));
                ptemp.PointOfCharger =  row.Field<String>("PointOfCharger");
                DataRobotToReadyList.Add(ptemp);
            }
            con.Close();
        }
        public void Start(RobotGoToReady state = RobotGoToReady.ROBREA_ROBOT_GOTO_FRONTLINE_READYSTATION)
        {
            errorCode = ErrorCode.RUN_OK;
            robot.ProcedureAs = ProcedureControlAssign.PRO_READY;
            StateRobotGoToReady = state;
            ProRobotToReady = new Thread(this.Procedure);
            ProRobotToReady.Start(this);
            ProRun = true;
        }
        public void Destroy()
        {
            // StateRobotGoToReady = RobotGoToReady.ROBREA_ROBOT_RELEASED;
            ProRun = false;
        }

        public void Procedure(object ojb)
        {
            ProcedureRobotToReady RbToRd = (ProcedureRobotToReady)ojb;
            RobotUnity rb = RbToRd.robot;
            DataRobotToReady p = RbToRd.points;
            TrafficManagementService Traffic = RbToRd.Traffic;
            while (ProRun)
            {
                switch (StateRobotGoToReady)
                {
                    case RobotGoToReady.ROBREA_IDLE:
                        break;
                    case RobotGoToReady.ROBREA_ROBOT_GOTO_FRONTLINE_READYSTATION: // ROBOT cho tiến vào vị trí đầu line charge su dung laser
                        rb.SendPoseStamped(p.PointFrontLine);
                        StateRobotGoToReady = RobotGoToReady.ROBREA_ROBOT_WAITTING_GOTO_READYSTATION;
                        break;
                    case RobotGoToReady.ROBREA_ROBOT_WAITTING_GOTO_READYSTATION: // Robot dang di toi dau line ready station
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                        {
                            rb.SendCmdAreaPallet(RbToRd.points.PointOfCharger);
                            // rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_READYAREA);
                            StateRobotGoToReady = RobotGoToReady.ROBREA_ROBOT_WAITTING_CAME_POSITION_READYSTATION;
                        }
                        else if(Traffic.RobotIsInArea("",robot.properties.pose.Position))
                        {
                            robot.TurnOnSupervisorTraffic(false);
                        }
                        break;
                    // case RobotGoToReady.ROBREA_ROBOT_WAIITNG_DETECTLINE_TO_READYSTATION: // đang đợi dò line để đến vị trí line trong buffer
                    //     if (true == rb.CheckPointDetectLine(p.PointOfCharger, rb))
                    //     {
                    //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                    //         StateRobotGoToReady = RobotGoToReady.ROBREA_ROBOT_WAITTING_CAME_POSITION_READYSTATION;
                    //     }
                    //     break;
                    case RobotGoToReady.ROBREA_ROBOT_WAITTING_CAME_POSITION_READYSTATION: // đến vị trả robot về robotmanagement để nhận quy trình mới
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOTO_POSITION)
                        {
                            StateRobotGoToReady = RobotGoToReady.ROBREA_ROBOT_RELEASED;
                        }
                        else if(resCmd == ResponseCommand.RESPONSE_ERROR){
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            StateRobotGoToReady = RobotGoToReady.ROBREA_ROBOT_RELEASED;    
                        }
                        break;
                    case RobotGoToReady.ROBREA_ROBOT_RELEASED:
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_READY;
                        if(errorCode == ErrorCode.RUN_OK){
                            ReleaseProcedureHandler(this);
                        }
                        else{
                            ErrorProcedureHandler(this);    
                        }
                        ProRun = false;
                        break;
                }
                Thread.Sleep(5);
            }
            StateRobotGoToReady = RobotGoToReady.ROBREA_IDLE;
        }

        public override void FinishStatesCallBack(Int32 message)
        {
            this.resCmd = (ResponseCommand)message;
        }
    }
}
