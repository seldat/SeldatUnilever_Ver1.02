using System;
using System.Diagnostics;
using System.Threading;
using DoorControllerService;
using SeldatMRMS.Management.DoorServices;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;

namespace SeldatMRMS
{

    public class ProcedureForkLiftToBuffer : ProcedureControlServices
    {
        public struct DataForkLiftToBuffer
        {
            // public Pose PointCheckInGate;
            // public Pose PointOfGate;
            // public Pose PointFrontLineGate;
            // public PointDetect PointPickPalletIn;
            // public Pose PointCheckInBuffer;
            // public Pose PointFrontLineBuffer;
            // public PointDetectBranching PointDetectLineBranching;
            // public PointDetect PointDropPallet;
        }
        // DataForkLiftToBuffer points;
        ForkLiftToBuffer StateForkLiftToBuffer;
        Thread ProForkLiftToBuffer;
        public RobotUnity robot;
        public DoorService door;
        ResponseCommand resCmd;
        TrafficManagementService Traffic;

        public override event Action<Object> ReleaseProcedureHandler;
        // public override event Action<Object> ErrorProcedureHandler;
        public ProcedureForkLiftToBuffer(RobotUnity robot, DoorManagementService doorservice, TrafficManagementService traffiicService) : base(robot)
        {
            StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_IDLE;
            resCmd = ResponseCommand.RESPONSE_NONE;
            this.robot = robot;
            // this.points = new DataForkLiftToBuffer();
            door = doorservice.DoorMezzamineUp;
            // this.points.PointFrontLineGate = this.door.config.PointFrontLine;
            // this.points.PointPickPalletIn = this.door.config.PointOfPallet;
            this.Traffic = traffiicService;
            procedureCode = ProcedureCode.PROC_CODE_FORKLIFT_TO_BUFFER;

        }
        public void Start(ForkLiftToBuffer state = ForkLiftToBuffer.FORBUF_ROBOT_GOTO_CHECKIN_GATE)
        {
            // public void Start (ForkLiftToBuffer state = ForkLiftToBuffer.FORBUF_ROBOT_RELEASED) {
            errorCode = ErrorCode.RUN_OK;
            robot.ProcedureAs = ProcedureControlAssign.PRO_FORKLIFT_TO_BUFFER;
            StateForkLiftToBuffer = state;
            ProForkLiftToBuffer = new Thread(this.Procedure);
            ProForkLiftToBuffer.Start(this);
            ProRun = true;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
        }
        public void Destroy()
        {
            // StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_RELEASED;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            ProRun = false;
            UpdateInformationInProc(this, ProcessStatus.F);
        }
        public void Procedure(object ojb)
        {
            ProcedureForkLiftToBuffer FlToBuf = (ProcedureForkLiftToBuffer)ojb;
            RobotUnity rb = FlToBuf.robot;
            // DataForkLiftToBuffer p = FlToBuf.points;
            DoorService ds = FlToBuf.door;
            TrafficManagementService Traffic = FlToBuf.Traffic;
            robot.ShowText(" Start -> " + procedureCode);
            while (ProRun)
            {
                switch (StateForkLiftToBuffer)
                {
                    case ForkLiftToBuffer.FORBUF_IDLE:
                        robot.ShowText("FORBUF_IDLE");
                        break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_GOTO_CHECKIN_GATE: //gui toa do di den khu vuc checkin cong
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
                                        StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_CAME_CHECKIN_GATE;
                                        robot.ShowText("FORBUF_ROBOT_CAME_CHECKIN_GATE");
                                    }
                                    else
                                    {
                                        rb.SendPoseStamped(ds.config.PointCheckInGate);
                                        StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE;
                                        robot.ShowText("FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE");
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
                                StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_CAME_CHECKIN_GATE;
                                robot.ShowText("FORBUF_ROBOT_CAME_CHECKIN_GATE");
                            }
                            else
                            {
                                rb.SendPoseStamped(ds.config.PointCheckInGate);
                                StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE;
                                robot.ShowText("FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE");
                            }
                        }
                        break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE:
                       // if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT && robot.ReachedGoal())
                        if ( robot.ReachedGoal())
                        {
                            robot.SetTrafficAtCheckIn(true);
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            rb.UpdateRiskAraParams(0, rb.properties.L2, rb.properties.WS, rb.properties.DistInter);
                            StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_CAME_CHECKIN_GATE;
                            robot.ShowText("FORBUF_ROBOT_CAME_CHECKIN_GATE");
                        }
                        break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_CAME_CHECKIN_GATE: // đã đến vị trí, kiem tra va cho khu vuc cong san sang de di vao.
                                                                          // robot.ShowText( "FORBUF_ROBOT_WAITTING_GOTO_GATE ===> FLAG " + Traffic.HasRobotUnityinArea(ds.config.PointFrontLine.Position));
                        if (false == Traffic.HasRobotUnityinArea(ds.config.PointFrontLine.Position))
                        {
                            robot.SetTrafficAtCheckIn(false);
                            rb.UpdateRiskAraParams(40, rb.properties.L2, rb.properties.WS, rb.properties.DistInter);
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            rb.SendPoseStamped(ds.config.PointFrontLine);
                            StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOTO_GATE;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_GOTO_GATE");
                        }
                        break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOTO_GATE:
                        //if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT && robot.ReachedGoal())
                        if ( robot.ReachedGoal())
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_CAME_GATE_POSITION;
                            robot.ShowText("FORBUF_ROBOT_CAME_GATE_POSITION");
                        }
                        break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_CAME_GATE_POSITION: // da den khu vuc cong , gui yeu cau mo cong.
                        if (ds.Open(DoorService.DoorType.DOOR_BACK))
                        {
                            StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_OPEN_DOOR;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_OPEN_DOOR");
                        }
                        else
                        {
                            errorCode = ErrorCode.CONNECT_DOOR_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_OPEN_DOOR: //doi mo cong
                        if (true == ds.WaitOpen(DoorService.DoorType.DOOR_BACK, TIME_OUT_OPEN_DOOR))
                        {
                            StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_OPEN_DOOR_SUCCESS;
                            robot.ShowText("FORBUF_ROBOT_OPEN_DOOR_SUCCESS");
                        }
                        else
                        {
                            errorCode = ErrorCode.OPEN_DOOR_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_OPEN_DOOR_SUCCESS: // mo cua thang cong ,gui toa do line de robot di vao gap hang
                        // rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETUP);
                        rb.SendCmdAreaPallet(ds.config.infoPallet);
                        StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_PICKUP_PALLET_IN;
                        robot.ShowText("FORBUF_ROBOT_WAITTING_PICKUP_PALLET_IN");
                        break;
                    // case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOTO_PALLET_IN:
                    //     if (true == rb.CheckPointDetectLine(ds.config.P, rb))
                    //     {
                    //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                    //         StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_PICKUP_PALLET_IN;
                    //     }
                    //     break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_PICKUP_PALLET_IN: // doi robot gap hang
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETUP)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            // FlToBuf.UpdatePalletState(PalletStatus.F);
                            //   rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            if (ds.Close(DoorService.DoorType.DOOR_BACK))
                            {
                                StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_CLOSE_GATE;
                                robot.ShowText("FORBUF_ROBOT_WAITTING_CLOSE_GATE");
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
                    case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_CLOSE_GATE: // doi dong cong.
                        try
                        {
                            if (true == ds.WaitClose(DoorService.DoorType.DOOR_BACK, TIME_OUT_CLOSE_DOOR))
                            {
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                                rb.SendPoseStamped(FlToBuf.GetCheckInBuffer(true));
                                StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER;
                                robot.ShowText("FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER");
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
                    case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER: // doi robot di den khu vuc checkin cua vung buffer
                       // if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT && robot.ReachedGoal())
                            if (robot.ReachedGoal())
                            {
                            robot.SetTrafficAtCheckIn(true);
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_ZONE_BUFFER_READY;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_ZONE_BUFFER_READY");
                        }
                        break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_ZONE_BUFFER_READY: // doi khu vuc buffer san sang de di vao
                        try
                        {
                            if (false == Traffic.HasRobotUnityinArea(FlToBuf.GetAnyPointInBuffer(true).Position))
                            {
                                robot.SetTrafficAtCheckIn(false);
                                // createPlanBuffer();
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                                rb.SendPoseStamped(FlToBuf.GetFrontLineBuffer(true));
                                StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER;
                                robot.ShowText("FORBUF_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER");
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER:
                        try
                        {
                          //  if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT && robot.ReachedGoal())
                                if (robot.ReachedGoal())
                                {
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                rb.SendCmdAreaPallet(FlToBuf.GetInfoOfPalletBuffer(PistonPalletCtrl.PISTON_PALLET_DOWN, true));
                                // rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_FORWARD_DIRECTION);
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_DROPDOWN_PALLET_BUFFER;
                                robot.ShowText("FORBUF_ROBOT_WAITTING_DROPDOWN_PALLET_BUFFER");
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    // case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOTO_POINT_BRANCHING:
                    //     if (true == rb.CheckPointDetectLine(FlToBuf.GetPointDetectBranching().xy, rb))
                    //     {
                    //         if (FlToBuf.GetPointDetectBranching().brDir == BrDirection.DIR_LEFT)
                    //         {
                    //             rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_TURN_LEFT);
                    //         }
                    //         else if (FlToBuf.GetPointDetectBranching().brDir == BrDirection.DIR_RIGHT)
                    //         {
                    //             rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_TURN_RIGHT);
                    //         }
                    //         StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_CAME_POINT_BRANCHING;
                    //     }
                    //     break;
                    // case ForkLiftToBuffer.FORBUF_ROBOT_CAME_POINT_BRANCHING:  //doi bobot re
                    //     if ((resCmd == ResponseCommand.RESPONSE_FINISH_TURN_LEFT) || (resCmd == ResponseCommand.RESPONSE_FINISH_TURN_RIGHT))
                    //     {
                    //         resCmd = ResponseCommand.RESPONSE_NONE;
                    //         rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETDOWN);
                    //         StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_GOTO_DROPDOWN_PALLET_BUFFER;
                    //     }
                    //     break;
                    // case ForkLiftToBuffer.FORBUF_ROBOT_GOTO_DROPDOWN_PALLET_BUFFER:
                    //     if (true == rb.CheckPointDetectLine(FlToBuf.GetPointPallet(), rb))
                    //     {
                    //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                    //         StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_DROPDOWN_PALLET_BUFFER;
                    //     }
                    //     break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_DROPDOWN_PALLET_BUFFER:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETDOWN)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            FlToBuf.UpdatePalletState(PalletStatus.W);
                            //   rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER: // đợi
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_RELEASED;
                            robot.ShowText("FORBUF_ROBOT_RELEASED");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToBuffer.FORBUF_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
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
            StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_IDLE;
        }
        public override void FinishStatesCallBack(Int32 message)
        {
            this.resCmd = (ResponseCommand)message;
        }
    }
}
