﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using SeldatUnilever_Ver1._02.Management.McuCom;
using SelDatUnilever_Ver1._00.Management.ChargerCtrl;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnity;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SelDatUnilever_Ver1._00.Management.ChargerCtrl.ChargerCtrl;
using static SelDatUnilever_Ver1._00.Management.ComSocket.RouterComPort;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SeldatMRMS
{
    public class ProcedureRobotToCharger : ProcedureControlServices
    {

        Thread ProRobotToCharger;
        public RobotUnity robot;
        ResponseCommand resCmd;
        RobotGoToCharge StateRobotToCharge;
        public ChargerCtrl chargerCtrl;
        DataReceive batLevel;
        DataReceive statusCharger;
        Stopwatch sw = new Stopwatch();
        const UInt32 TIME_OUT_WAIT_TURNOFF_PC = 60000 * 5;
        const UInt32 TIME_OUT_WAIT_STATE = 60000 * 2;
        const UInt32 TIME_OUT_ROBOT_RECONNECT_SERVER = 60000 * 10;
        const UInt32 TIME_COUNT_GET_BAT_LEVEL = 1000;
        const UInt32 TIME_DELAY_RELEASE_CHARGE = 60000 * 5;
        private UInt32 timeCountGetBatLevel = 0;
        public override event Action<Object> ReleaseProcedureHandler;
        // public override event Action<Object> ErrorProcedureHandler;
        public byte getBatteryLevel()
        {
            return batLevel.data[0];
        }
        public byte getStatusCharger()
        {
            return statusCharger.data[0];
        }
        public ProcedureRobotToCharger(RobotUnity robot, ChargerManagementService charger, ChargerId id) : base(robot)
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
            procedureCode = ProcedureCode.PROC_CODE_ROBOT_TO_CHARGE;
            ProRun = true;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
        }
        public void Destroy()
        {
            // StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_RELEASED;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            ProRun = false;
            UpdateInformationInProc(this, ProcessStatus.F);
            selectHandleError = SelectHandleError.CASE_ERROR_EXIT;
         //   this.robot.DestroyRegistrySolvedForm();
        }
        public void Procedure(object ojb)
        {
            ProcedureRobotToCharger RbToChar = (ProcedureRobotToCharger)ojb;
            RobotUnity rb = RbToChar.robot;
            ErrorCodeCharger result;
            McuCtrl mcuCtrl = new McuCtrl(rb);
            robot.ShowText(" Start -> " + procedureCode);
            while (ProRun)
            {
                switch (StateRobotToCharge)
                {
                    case RobotGoToCharge.ROBCHAR_IDLE:
                        robot.ShowText("ROBCHAR_IDLE");
                        break;
                    // case RobotGoToCharge.ROBCHAR_CHARGER_CHECKSTATUS:
                    //     if(true == chargerCtrl.WaitState(ChargerState.ST_READY,TIME_OUT_WAIT_STATE)){
                    //         StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_ALLOW_CUTOFF_POWER_ROBOT;
                    //     }
                    //     break; //kiểm tra kết nối và trạng thái sạc
                    case RobotGoToCharge.ROBCHAR_ROBOT_GOTO_CHARGER:
                        robot.ShowText("ROBCHAR_ROBOT_GOTO_CHARGER");
                        robot.TurnOnSupervisorTraffic(false);
                        if (rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_GETIN_CHARGER))
                        {
                            //Thread.Sleep(1000);
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_ALLOW_CUTOFF_POWER_ROBOT;
                            robot.ShowText("ROBCHAR_ROBOT_ALLOW_CUTOFF_POWER_ROBOT");
                        }
                        break;
                    case RobotGoToCharge.ROBCHAR_ROBOT_ALLOW_CUTOFF_POWER_ROBOT:
                        try
                        {
                            if (resCmd == ResponseCommand.RESPONSE_FINISH_DETECTLINE_GETIN_CHARGER)
                            {
                                if (rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_TURNOFF_PC))
                                {
                                    rb.Dispose();
                                    StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_WAITTING_CUTOFF_POWER_PC;
                                    sw.Start();
                                    robot.ShowText("ROBCHAR_ROBOT_WAITTING_CUTOFF_POWER_PC");
                                }
                            }
                            else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                            {
                                errorCode = ErrorCode.DETECT_LINE_CHARGER_ERROR;
                                CheckUserHandleError(this);
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CONNECT_CHARGER_ERROR;
                            CheckUserHandleError(this);
                        }
                        break; //cho phép cắt nguồn robot
                    case RobotGoToCharge.ROBCHAR_ROBOT_WAITTING_CUTOFF_POWER_PC:
                        if (true != rb.properties.IsConnected)
                        {
                            robot.ShowText("Sleep 30s waitting turnoff power");
                            Thread.Sleep(30000);
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_START_CHARGE;
                            sw.Stop();
                            robot.ShowText("ROBCHAR_ROBOT_START_CHARGE");
                        }
                        else
                        {
                            if (sw.ElapsedMilliseconds > TIME_OUT_WAIT_TURNOFF_PC)
                            {
                                sw.Stop();
                                errorCode = ErrorCode.CAN_NOT_TURN_OFF_PC;
                                CheckUserHandleError(this);
                                StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_ALLOW_CUTOFF_POWER_ROBOT;
                            }
                        }
                        break;

                    case RobotGoToCharge.ROBCHAR_ROBOT_START_CHARGE:
                        try
                        {
                            if (true == chargerCtrl.StartCharge())
                            {
                                StateRobotToCharge = RobotGoToCharge.ROBCHAR_WAITTING_ROBOT_CONTACT_CHARGER;
                                robot.ShowText("ROBCHAR_WAITTING_ROBOT_CONTACT_CHARGER");
                            }
                            else
                            {
                                errorCode = ErrorCode.CONNECT_CHARGER_ERROR;
                                CheckUserHandleError(this);
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CONNECT_CHARGER_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case RobotGoToCharge.ROBCHAR_WAITTING_ROBOT_CONTACT_CHARGER:
                        try
                        {
                            result = chargerCtrl.WaitState(ChargerState.ST_CHARGING, TIME_OUT_WAIT_STATE);
                            if (ErrorCodeCharger.TRUE == result)
                            {
                                StateRobotToCharge = RobotGoToCharge.ROBCHAR_WAITTING_CHARGEBATTERY;
                                robot.ShowText("ROBCHAR_WAITTING_CHARGEBATTERY");
                            }
                            else
                            {
                                if (result == ErrorCodeCharger.ERROR_CONNECT)
                                {
                                    errorCode = ErrorCode.CONNECT_CHARGER_ERROR;
                                }
                                else
                                {
                                    errorCode = ErrorCode.CONTACT_CHARGER_ERROR;
                                }
                                CheckUserHandleError(this);
                                //StateRobotToCharge = RobotGoToCharge.ROBCHAR_FINISHED_CHARGEBATTERY;
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CONNECT_CHARGER_ERROR;
                            CheckUserHandleError(this);
                        }
                        break; //robot tiep xuc tram sac        
                    //case RobotGoToCharge.ROBCHAR_ROBOT_ALLOW_CUTOFF_POWER_ROBOT:
                    //    rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_TURNOFF_PC);
                    //    StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_WAITTING_CUTOFF_POWER_PC;
                    //    robot.ShowText("ROBCHAR_ROBOT_WAITTING_CUTOFF_POWER_PC"); 
                    //    sw.Start ();
                    //    break; //cho phép cắt nguồn robot
                    //case RobotGoToCharge.ROBCHAR_ROBOT_WAITTING_CUTOFF_POWER_PC:
                    //    if (true != rb.properties.IsConnected) {
                    //        StateRobotToCharge = RobotGoToCharge.ROBCHAR_WAITTING_CHARGEBATTERY;
                    //        robot.ShowText("ROBCHAR_WAITTING_CHARGEBATTERY"); 
                    //    } else {
                    //        if (sw.ElapsedMilliseconds > TIME_OUT_WAIT_TURNOFF_PC) {
                    //            sw.Stop ();
                    //            errorCode = ErrorCode.CAN_NOT_TURN_OFF_PC;
                    //            CheckUserHandleError (this);
                    //            StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_ALLOW_CUTOFF_POWER_ROBOT;
                    //        }
                    //    }
                    //    break;
                    case RobotGoToCharge.ROBCHAR_WAITTING_CHARGEBATTERY:
#if false  //for test
                        StateRobotToCharge = RobotGoToCharge.ROBCHAR_FINISHED_CHARGEBATTERY;
#else
#if true
                        try
                        {
                            timeCountGetBatLevel++;
                            if (timeCountGetBatLevel >= TIME_COUNT_GET_BAT_LEVEL)
                            {
                                timeCountGetBatLevel = 0;
                                result = mcuCtrl.GetBatteryLevel(ref batLevel);
                                Console.WriteLine("=================****+++++bat level {0}+++++++++++++++++++", batLevel.data[0]);
                                if (ErrorCodeCharger.TRUE == result)
                                {
                                    if (batLevel.data[0] >= 99)
                                    {
                                        Thread.Sleep((int)TIME_DELAY_RELEASE_CHARGE);
                                        StateRobotToCharge = RobotGoToCharge.ROBCHAR_FINISHED_CHARGEBATTERY;
                                        robot.ShowText("ROBCHAR_FINISHED_CHARGEBATTERY");
                                    }
                                }
                                else
                                {
                                    if (result == ErrorCodeCharger.ERROR_CONNECT)
                                    {
                                        errorCode = ErrorCode.CONNECT_BOARD_CTRL_ROBOT_ERROR;
                                    }
                                    CheckUserHandleError(this);
                                }
                                rb.properties.BatteryLevelRb = (float)batLevel.data[0];
                            }

                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CONNECT_BOARD_CTRL_ROBOT_ERROR;
                            CheckUserHandleError(this);
                        }
#else
                        try {
                            result = chargerCtrl.GetBatteryAndStatus (ref batLevel, ref statusCharger);
                            if (ErrorCodeCharger.TRUE == result) {
                                if ((batLevel.data[0] == 100) || (statusCharger.data[0] == (byte) ChargerState.ST_CHARGE_FULL)) {
                                    StateRobotToCharge = RobotGoToCharge.ROBCHAR_FINISHED_CHARGEBATTERY;
                                    robot.ShowText("ROBCHAR_FINISHED_CHARGEBATTERY"); 
                                }
                            } else {
                                if (result == ErrorCodeCharger.ERROR_CONNECT) {
                                    errorCode = ErrorCode.CONNECT_CHARGER_ERROR;
                                }
                                CheckUserHandleError (this);
                            }
                            rb.properties.BatteryLevelRb = (float) batLevel.data[0];
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CONNECT_CHARGER_ERROR;
                            CheckUserHandleError (this);
                        }
#endif
#endif
                        break; //dợi charge battery và thông tin giao tiếp server và trạm sạc

                    case RobotGoToCharge.ROBCHAR_FINISHED_CHARGEBATTERY:
                        try
                        {
                            if (true == chargerCtrl.StopCharge())
                            {
                                Console.WriteLine("Stop charger success");
                            }
                            else
                            {
                                errorCode = ErrorCode.CONNECT_CHARGER_ERROR;
                                CheckUserHandleError(this);
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CONNECT_CHARGER_ERROR;
                            CheckUserHandleError(this);
                        }

                        Thread.Sleep(10000);

                        try
                        {
                            if (true == mcuCtrl.TurnOnPcRobot())
                            {
                                Thread.Sleep(10000);
                                rb.Start(rb.properties.Url);
                                StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_WAITTING_RECONNECTING;
                                robot.ShowText("ROBCHAR_ROBOT_WAITTING_RECONNECTING");
                            }
                            else
                            {
                                errorCode = ErrorCode.CAN_NOT_TURN_ON_PC;
                                CheckUserHandleError(this);
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CONNECT_BOARD_CTRL_ROBOT_ERROR;
                            CheckUserHandleError(this);
                        }
                        break; //Hoàn Thành charge battery và thông tin giao tiếp server và trạm sạc
                    case RobotGoToCharge.ROBCHAR_ROBOT_WAITTING_RECONNECTING:
                        if (true == CheckReconnectServer(TIME_OUT_ROBOT_RECONNECT_SERVER))
                        {
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_GETOUT_CHARGER;
                            robot.ShowText("ROBCHAR_ROBOT_GETOUT_CHARGER");
                        }
                        else
                        {
                            errorCode = ErrorCode.ROBOT_CANNOT_CONNECT_SERVER_AFTER_CHARGE;
                            CheckUserHandleError(this);
                        }
                        break; //Robot mở nguồng và đợi connect lại
                    case RobotGoToCharge.ROBCHAR_ROBOT_GETOUT_CHARGER:
                        if (rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_GETOUT_CHARGER))
                        {
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_WAITTING_GETOUT_CHARGER;
                            robot.ShowText("ROBCHAR_ROBOT_WAITTING_GETOUT_CHARGER");
                        }
                        break;
                    case RobotGoToCharge.ROBCHAR_ROBOT_WAITTING_GETOUT_CHARGER:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_DETECTLINE_GETOUT_CHARGER)
                        {
                            StateRobotToCharge = RobotGoToCharge.ROBCHAR_ROBOT_RELEASED;
                            robot.ShowText("ROBCHAR_ROBOT_RELEASED");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_CHARGER_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case RobotGoToCharge.ROBCHAR_ROBOT_RELEASED:
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_CHARGE;
                        // if (errorCode == ErrorCode.RUN_OK) {
                        ReleaseProcedureHandler(this);
                        // } else {
                        //     ErrorProcedureHandler (this);
                        // }
                        ProRun = false;
                        robot.ShowText("RELEASED");
                        UpdateInformationInProc(this, ProcessStatus.S);
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
                if (sw.ElapsedMilliseconds > timeOut)
                {
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
        //List<DataRobotToReady> DataRobotToReadyList;
        Thread ProRobotToReady;
        ChargerManagementService charger;
        public RobotUnity robot;
        ResponseCommand resCmd;
        RobotGoToReady StateRobotGoToReady;
        TrafficManagementService Traffic;
        public override event Action<Object> ReleaseProcedureHandler;
        // public override event Action<Object> ErrorProcedureHandler;
        public ProcedureRobotToReady(RobotUnity robot, ChargerId id, TrafficManagementService trafficService, ChargerManagementService chargerService) : base(robot)
        {
            StateRobotGoToReady = RobotGoToReady.ROBREA_IDLE;
            this.robot = robot;
            this.Traffic = trafficService;
            this.charger = chargerService;
            points.PointFrontLine = this.charger.PropertiesCharge_List[(int)id - 1].PointFrontLine;
            points.PointOfCharger = this.charger.PropertiesCharge_List[(int)id - 1].PointOfPallet;
            procedureCode = ProcedureCode.PROC_CODE_ROBOT_TO_READY;
        }

        public void Start(RobotGoToReady state = RobotGoToReady.ROBREA_ROBOT_GOTO_FRONTLINE_READYSTATION)
        {
            errorCode = ErrorCode.RUN_OK;
            robot.ProcedureAs = ProcedureControlAssign.PRO_READY;
            StateRobotGoToReady = state;
            ProRobotToReady = new Thread(this.Procedure);
            ProRobotToReady.Start(this);
            procedureCode = ProcedureCode.PROC_CODE_ROBOT_TO_READY;
            ProRun = true;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
        }
        public void Destroy()
        {
            // StateRobotGoToReady = RobotGoToReady.ROBREA_ROBOT_RELEASED;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            ProRun = false;
            UpdateInformationInProc(this, ProcessStatus.F);
            selectHandleError = SelectHandleError.CASE_ERROR_EXIT;
        }

        public void Procedure(object ojb)
        {
            ProcedureRobotToReady RbToRd = (ProcedureRobotToReady)ojb;
            RobotUnity rb = RbToRd.robot;
            DataRobotToReady p = RbToRd.points;
            TrafficManagementService Traffic = RbToRd.Traffic;
            robot.ShowText(" Start -> " + procedureCode);
            while (ProRun)
            {
                switch (StateRobotGoToReady)
                {
                    case RobotGoToReady.ROBREA_IDLE:
                        robot.ShowText("ROBREA_IDLE");
                        break;
                    case RobotGoToReady.ROBREA_ROBOT_GOTO_FRONTLINE_READYSTATION: // ROBOT cho tiến vào vị trí đầu line charge su dung laser
                        if (rb.SendPoseStamped(p.PointFrontLine))
                        {
                            StateRobotGoToReady = RobotGoToReady.ROBREA_ROBOT_WAITTING_GOTO_READYSTATION;
                            robot.ShowText("ROBREA_ROBOT_WAITTING_GOTO_READYSTATION");
                        }
                        break;
                    case RobotGoToReady.ROBREA_ROBOT_WAITTING_GOTO_READYSTATION: // Robot dang di toi dau line ready station
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                            //if ( robot.ReachedGoal())
                        {
                            //rb.SendCmdAreaPallet(RbToRd.points.PointOfCharger);
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            // rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_READYAREA);
                            StateRobotGoToReady = RobotGoToReady.ROBREA_ROBOT_WAIITNG_DETECTLINE_TO_READYSTATION;
                            robot.ShowText("ROBREA_ROBOT_WAIITNG_DETECTLINE_TO_READYSTATION");
                        }
                        else if (Traffic.RobotIsInArea("READY", robot.properties.pose.Position))
                        {
                           // robot.TurnOnSupervisorTraffic(false);
                        }
                        break;
                    case RobotGoToReady.ROBREA_ROBOT_WAIITNG_DETECTLINE_TO_READYSTATION: // đang đợi dò line để đến vị trí line trong buffer
                        if (rb.SendCmdAreaPallet(RbToRd.points.PointOfCharger))
                        {
                            StateRobotGoToReady = RobotGoToReady.ROBREA_ROBOT_WAITTING_CAME_POSITION_READYSTATION;
                            robot.ShowText("ROBREA_ROBOT_WAITTING_CAME_POSITION_READYSTATION");
                        }
                        break;
                    case RobotGoToReady.ROBREA_ROBOT_WAITTING_CAME_POSITION_READYSTATION: // đến vị trả robot về robotmanagement để nhận quy trình mới
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOTO_POSITION)
                        {
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            StateRobotGoToReady = RobotGoToReady.ROBREA_ROBOT_RELEASED;
                            robot.ShowText("ROBREA_ROBOT_RELEASED");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case RobotGoToReady.ROBREA_ROBOT_RELEASED:
                        robot.TurnOnSupervisorTraffic(false);
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_READY;
                        // if (errorCode == ErrorCode.RUN_OK) {
                        ReleaseProcedureHandler(this);
                        // } else {
                        //     ErrorProcedureHandler (this);
                        // }
                        robot.setColorRobotStatus(RobotStatusColorCode.ROBOT_STATUS_OK);
                        ProRun = false;
                        robot.ShowText("RELEASED");
                        UpdateInformationInProc(this, ProcessStatus.S);
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