using DoorControllerService;
using SeldatMRMS;
using SeldatMRMS.Management.DoorServices;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using System;
using System.Diagnostics;
using System.Threading;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;

namespace SeldatUnilever_Ver1._02.Management.ProcedureServices
{
    public class ProcedureForkLiftToMachine : ProcedureControlServices
    {
        ForkLiftToMachine StateForkLiftToMachine;
        Thread ProForkLiftToMachine;
        public RobotUnity robot;
        public DoorService door;
        ResponseCommand resCmd;
        TrafficManagementService Traffic;

        public override event Action<Object> ReleaseProcedureHandler;
        // public override event Action<Object> ErrorProcedureHandler;
        public ProcedureForkLiftToMachine(RobotUnity robot, DoorManagementService doorservice, TrafficManagementService traffiicService) : base(robot)
        {
            StateForkLiftToMachine = ForkLiftToMachine.FORMAC_IDLE;
            resCmd = ResponseCommand.RESPONSE_NONE;
            this.robot = robot;
            door = doorservice.DoorMezzamineUp;
            this.Traffic = traffiicService;
            procedureCode = ProcedureCode.PROC_CODE_FORKLIFT_TO_BUFFER;

        }
        public void Start(ForkLiftToMachine state = ForkLiftToMachine.FORMAC_ROBOT_GOTO_CHECKIN_GATE)
        {
            errorCode = ErrorCode.RUN_OK;
            robot.ProcedureAs = ProcedureControlAssign.PRO_FORKLIFT_TO_MACHINE;
            StateForkLiftToMachine = state;
            ProForkLiftToMachine = new Thread(this.Procedure);
            ProForkLiftToMachine.Start(this);
            ProRun = true;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
        }
        public void Destroy()
        {
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            ProRun = false;
            UpdateInformationInProc(this, ProcessStatus.F);
        }
        public void Procedure(object ojb)
        {
            ProcedureForkLiftToMachine FlToMach = (ProcedureForkLiftToMachine)ojb;
            RobotUnity rb = FlToMach.robot;
            DoorService ds = FlToMach.door;
            TrafficManagementService Traffic = FlToMach.Traffic;
            robot.ShowText(" Start -> " + procedureCode);
            while (ProRun)
            {
                switch (StateForkLiftToMachine)
                {
                    case ForkLiftToMachine.FORMAC_IDLE:
                        robot.ShowText("FORMAC_IDLE");
                        break;
                    case ForkLiftToMachine.FORMAC_ROBOT_GOTO_CHECKIN_GATE: //gui toa do di den khu vuc checkin cong
                        if (rb.PreProcedureAs == ProcedureControlAssign.PRO_READY)
                        {
                            rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            Stopwatch sw = new Stopwatch();
                            sw.Start();
                            do
                            {
                                if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                                {
                                    resCmd = ResponseCommand.RESPONSE_NONE;
                                    if (Traffic.RobotIsInArea("OPA4", rb.properties.pose.Position))
                                    {
                                        rb.SendPoseStamped(ds.config.PointFrontLine);
                                        StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_CAME_CHECKIN_GATE;
                                        robot.ShowText("FORMAC_ROBOT_CAME_CHECKIN_GATE");
                                    }
                                    else
                                    {
                                        rb.SendPoseStamped(ds.config.PointCheckInGate);
                                        StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_WAITTING_GOTO_CHECKIN_GATE;
                                        robot.ShowText("FORMAC_ROBOT_WAITTING_GOTO_CHECKIN_GATE");
                                    }
                                    break;
                                }
                                else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                                {
                                    errorCode = ErrorCode.DETECT_LINE_ERROR;
                                    CheckUserHandleError(this);
                                    break;
                                }
                                if (sw.ElapsedMilliseconds > TIME_OUT_WAIT_GOTO_FRONTLINE)
                                {
                                    errorCode = ErrorCode.DETECT_LINE_ERROR;
                                    CheckUserHandleError(this);
                                    break;
                                }
                                Thread.Sleep(100);
                            } while (true);
                            sw.Stop();
                        }
                        else
                        {
                            if (Traffic.RobotIsInArea("OPA4", rb.properties.pose.Position))
                            {
                                rb.SendPoseStamped(ds.config.PointFrontLine);
                                StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_CAME_CHECKIN_GATE;
                                robot.ShowText("FORMAC_ROBOT_CAME_CHECKIN_GATE");
                            }
                            else
                            {
                                rb.SendPoseStamped(ds.config.PointCheckInGate);
                                StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_WAITTING_GOTO_CHECKIN_GATE;
                                robot.ShowText("FORMAC_ROBOT_WAITTING_GOTO_CHECKIN_GATE");
                            }
                        }
                        break;
                    case ForkLiftToMachine.FORMAC_ROBOT_WAITTING_GOTO_CHECKIN_GATE:
                        // if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT && robot.ReachedGoal())
                        if (robot.ReachedGoal())
                        {
                            robot.SetTrafficAtCheckIn(true);
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            rb.UpdateRiskAraParams(0, rb.properties.L2, rb.properties.WS, rb.properties.DistInter);
                            StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_CAME_CHECKIN_GATE;
                            robot.ShowText("FORMAC_ROBOT_CAME_CHECKIN_GATE");
                        }
                        break;
                    case ForkLiftToMachine.FORMAC_ROBOT_CAME_CHECKIN_GATE: // đã đến vị trí, kiem tra va cho khu vuc cong san sang de di vao.
                                                                          // robot.ShowText( "FORMAC_ROBOT_WAITTING_GOTO_GATE ===> FLAG " + Traffic.HasRobotUnityinArea(ds.config.PointFrontLine.Position));
                        if (false == Traffic.HasRobotUnityinArea(ds.config.PointFrontLine.Position))
                        {
                            robot.SetTrafficAtCheckIn(false);
                            rb.UpdateRiskAraParams(40, rb.properties.L2, rb.properties.WS, rb.properties.DistInter);
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            rb.SendPoseStamped(ds.config.PointFrontLine);
                            StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_WAITTING_GOTO_GATE;
                            robot.ShowText("FORMAC_ROBOT_WAITTING_GOTO_GATE");
                        }
                        break;
                    case ForkLiftToMachine.FORMAC_ROBOT_WAITTING_GOTO_GATE:
                        //if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT && robot.ReachedGoal())
                        if (robot.ReachedGoal())
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_CAME_GATE_POSITION;
                            robot.ShowText("FORMAC_ROBOT_CAME_GATE_POSITION");
                        }
                        break;
                    case ForkLiftToMachine.FORMAC_ROBOT_CAME_GATE_POSITION: // da den khu vuc cong , gui yeu cau mo cong.
                        if (ds.Open(DoorService.DoorType.DOOR_BACK))
                        {
                            StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_WAITTING_OPEN_DOOR;
                            robot.ShowText("FORMAC_ROBOT_WAITTING_OPEN_DOOR");
                        }
                        else
                        {
                            errorCode = ErrorCode.CONNECT_DOOR_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToMachine.FORMAC_ROBOT_WAITTING_OPEN_DOOR: //doi mo cong
                        if (true == ds.WaitOpen(DoorService.DoorType.DOOR_BACK, TIME_OUT_OPEN_DOOR))
                        {
                            StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_OPEN_DOOR_SUCCESS;
                            robot.ShowText("FORMAC_ROBOT_OPEN_DOOR_SUCCESS");
                        }
                        else
                        {
                            errorCode = ErrorCode.OPEN_DOOR_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToMachine.FORMAC_ROBOT_OPEN_DOOR_SUCCESS: // mo cua thang cong ,gui toa do line de robot di vao gap hang
                        // rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETUP);
                        rb.SendCmdAreaPallet(ds.config.infoPallet);
                        StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_WAITTING_PICKUP_PALLET_IN;
                        robot.ShowText("FORMAC_ROBOT_WAITTING_PICKUP_PALLET_IN");
                        break;
                    case ForkLiftToMachine.FORMAC_ROBOT_WAITTING_PICKUP_PALLET_IN: // doi robot gap hang
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETUP)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            // FlToMach.UpdatePalletState(PalletStatus.F);
                            //   rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE;
                            robot.ShowText("FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToMachine.FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            if (ds.Close(DoorService.DoorType.DOOR_BACK))
                            {
                                StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_WAITTING_CLOSE_GATE;
                                robot.ShowText("FORMAC_ROBOT_WAITTING_CLOSE_GATE");
                            }
                            else
                            {
                                errorCode = ErrorCode.CONNECT_DOOR_ERROR;
                                CheckUserHandleError(this);
                            }
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToMachine.FORMAC_ROBOT_WAITTING_CLOSE_GATE: // doi dong cong.
                        try
                        {
                            if (true == ds.WaitClose(DoorService.DoorType.DOOR_BACK, TIME_OUT_CLOSE_DOOR))
                            {
                                robot.TurnOnCtrlSelfTraffic(true);
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                                rb.SendPoseStamped(FlToMach.GetFrontLineMachine());
                                StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE;
                                robot.ShowText("FORMAC_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE");
                            }
                            else
                            {
                                errorCode = ErrorCode.CLOSE_DOOR_ERROR;
                                CheckUserHandleError(this);
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;

                    case ForkLiftToMachine.FORMAC_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE:
                        try
                        {
                            //  if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT && robot.ReachedGoal())
                            if (robot.ReachedGoal())
                            {
                                robot.TurnOnCtrlSelfTraffic(false);
                                rb.SendCmdAreaPallet(FlToMach.GetInfoOfPalletMachine(PistonPalletCtrl.PISTON_PALLET_DOWN));
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE;
                                robot.ShowText("FORMAC_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE");
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;

                    case ForkLiftToMachine.FORMAC_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETDOWN)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            // FlToMach.UpdatePalletState(PalletStatus.W);
                            //   rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE;
                            robot.ShowText("FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToMachine.FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE: // đợi
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            StateForkLiftToMachine = ForkLiftToMachine.FORMAC_ROBOT_RELEASED;
                            robot.ShowText("FORMAC_ROBOT_RELEASED");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToMachine.FORMAC_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
                        robot.TurnOnCtrlSelfTraffic(true);
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_FORKLIFT_TO_BUFFER;
                        // if (errorCode == ErrorCode.RUN_OK) {
                        ReleaseProcedureHandler(this);
                        // } else {
                        //     ErrorProcedureHandler (this);
                        // }
                        ProRun = false;
                        robot.ShowText("RELEASED");
                        UpdateInformationInProc(this, ProcessStatus.S);
                        break;
                    default:
                        break;
                }
                Thread.Sleep(5);
            }
            StateForkLiftToMachine = ForkLiftToMachine.FORMAC_IDLE;
        }
        public override void FinishStatesCallBack(Int32 message)
        {
            this.resCmd = (ResponseCommand)message;
        }
    }
}
