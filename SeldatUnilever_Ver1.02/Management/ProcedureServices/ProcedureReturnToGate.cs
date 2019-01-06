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

namespace SeldatMRMS {

    public class ProcedureReturnToGate : ProcedureControlServices {
        public struct DataReturnToGate {
            // public Pose PointCheckInReturn;
            // public Pose PointFrontLineReturn;
            // public Pose PointFrontLineGate;
            // public PointDetect PointPickPallet;
            // public Pose PointCheckInGate;
            // public Pose PointOfGate;
            // public PointDetect PointDropPallet;
        }
        // DataReturnToGate points;
        ReturnToGate StateReturnToGate;
        Thread ProReturnToGate;
        RobotUnity robot;
        DoorManagementService door;
        ResponseCommand resCmd;
        TrafficManagementService Traffic;
        const UInt32 TIME_OUT_OPEN_DOOR = 600000; /* ms */
        const UInt32 TIME_OUT_CLOSE_DOOR = 600000; /* ms */

        public override event Action<Object> ReleaseProcedureHandler;
        public override event Action<Object> ErrorProcedureHandler;
        public ProcedureReturnToGate (RobotUnity robot, DoorManagementService doorservice, TrafficManagementService traffiicService) : base (robot) {
            StateReturnToGate = ReturnToGate.RETGATE_IDLE;
            resCmd = ResponseCommand.RESPONSE_NONE;
            this.robot = robot;
            // this.points = new DataReturnToGate();
            this.door = doorservice;
            this.Traffic = traffiicService;
            errorCode = ErrorCode.RUN_OK;
            procedureCode = ProcedureCode.PROC_CODE_RETURN_TO_GATE;
        }
        public void Start (ReturnToGate state = ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_RETURN) {
            errorCode = ErrorCode.RUN_OK;
            robot.ProcedureAs = ProcedureControlAssign.PRO_RETURN_TO_GATE;
            StateReturnToGate = state;
            ProReturnToGate = new Thread (this.Procedure);
            ProReturnToGate.Start (this);
            ProRun = true;
        }
        public void Destroy () {
            // StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
            ProRun = false;
        }
        public void Procedure (object ojb) {
            ProcedureReturnToGate ReToGate = (ProcedureReturnToGate) ojb;
            RobotUnity rb = ReToGate.robot;
            // DataReturnToGate p = ReToGate.points;
            DoorService ds = ReToGate.door.DoorMezzamineReturnBack;
            TrafficManagementService Traffic = ReToGate.Traffic;
            while (ProRun) {
                switch (StateReturnToGate) {
                    case ReturnToGate.RETGATE_IDLE:
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_RETURN: // doi robot di den khu vuc checkin cua vung buffer
                        try {
                            if (rb.PreProcedureAs == ProcedureControlAssign.PRO_READY) {
                                rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                                Stopwatch sw = new Stopwatch ();
                                sw.Start ();
                                do {
                                    if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE) {
                                        resCmd = ResponseCommand.RESPONSE_NONE;
                                        rb.SendPoseStamped (ReToGate.GetCheckInBuffer ());
                                        StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_ZONE_RETURN_READY;
                                        break;
                                    } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                                        errorCode = ErrorCode.DETECT_LINE_ERROR;
                                        StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                                        break;
                                    }
                                    if (sw.ElapsedMilliseconds > TIME_OUT_WAIT_GOTO_FRONTLINE) {
                                        errorCode = ErrorCode.DETECT_LINE_ERROR;
                                        StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                                        break;
                                    }
                                    Thread.Sleep (100);
                                } while (true);
                                sw.Stop ();
                            } else {
                                rb.SendPoseStamped (ReToGate.GetCheckInBuffer ());
                                StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_ZONE_RETURN_READY;
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_ZONE_RETURN_READY: // doi khu vuc buffer san sang de di vao
                        try {
                            if (false == Traffic.HasRobotUnityinArea (ReToGate.GetFrontLineReturn ().Position)) {
                                rb.SendPoseStamped (ReToGate.GetFrontLineReturn ());
                                StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_CAME_FRONTLINE_RETURN;
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_CAME_FRONTLINE_RETURN:
                        try {
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT) {
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                rb.SendCmdAreaPallet (ReToGate.GetInfoOfPalletReturn (PistonPalletCtrl.PISTON_PALLET_UP));
                                // rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETUP);
                                StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_PICKUP_PALLET_RETURN;
                            } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                                errorCode = ErrorCode.DETECT_LINE_ERROR;
                                StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                        }
                        break;
                        // case ReturnToGate.RETGATE_ROBOT_GOTO_PICKUP_PALLET_RETURN:
                        //     if (true == rb.CheckPointDetectLine(ReToGate.GetPointPallet(), rb))
                        //     {
                        //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                        //         StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_PICKUP_PALLET_RETURN;
                        //     }
                        //     break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_PICKUP_PALLET_RETURN:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETUP) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            ReToGate.UpdatePalletState (PalletStatus.F);
                            rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_GOBACK_FRONTLINE_RETURN;
                        } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_GOBACK_FRONTLINE_RETURN: // đợi
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.SendPoseStamped (ds.config.PointCheckInGate);
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_GATE;
                        } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                        }
                        break;
                        // case ReturnToGate.RETGATE_ROBOT_GOTO_CHECKIN_GATE: //gui toa do di den khu vuc checkin cong
                        //     rb.SendPoseStamped(ds.config.PointCheckInGate);
                        //     StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_GATE;
                        //     break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_CAME_CHECKIN_GATE;
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_CAME_CHECKIN_GATE: // đã đến vị trí, kiem tra va cho khu vuc cong san sang de di vao.
                        if (false == Traffic.HasRobotUnityinArea (ds.config.PointFrontLine.Position)) {
                            rb.SendPoseStamped (ds.config.PointFrontLine);
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_GATE;
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_CAME_GATE_POSITION;
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_CAME_GATE_POSITION: // da den khu vuc cong , gui yeu cau mo cong.
                        if (true == ds.Open (DoorService.DoorId.DOOR_MEZZAMINE_RETURN_BACK)) {
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_OPEN_DOOR;
                        } else {
                            errorCode = ErrorCode.CONNECT_DOOR_ERROR;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_OPEN_DOOR: //doi mo cong
                        if (true == ds.WaitOpen (DoorService.DoorId.DOOR_MEZZAMINE_RETURN_BACK, TIME_OUT_OPEN_DOOR)) {
                            rb.SendCmdAreaPallet (ds.config.infoPallet);
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_DROPDOWN_PALLET_RETURN;
                        } else {
                            errorCode = ErrorCode.OPEN_DOOR_ERROR;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                        }
                        break;
                        // case ReturnToGate.RETGATE_ROBOT_OPEN_DOOR_SUCCESS: // mo cua thang cong ,gui toa do line de robot di vao
                        //     rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETDOWN);
                        //     StateReturnToGate = ReturnToGate.RETGATE_ROBOT_GOTO_POSITION_PALLET_RETURN;
                        //     break;
                        // case ReturnToGate.RETGATE_ROBOT_GOTO_POSITION_PALLET_RETURN:
                        //     if (true == rb.CheckPointDetectLine(ds.config.PointOfPallet, rb))
                        //     {
                        //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                        //         StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_DROPDOWN_PALLET_RETURN;
                        //     }
                        //     break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_DROPDOWN_PALLET_RETURN: // doi robot gap hang
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETDOWN) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            // ReToGate.UpdatePalletState(PalletStatus.W);
                            rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE;
                        } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            if (ds.Close (DoorService.DoorId.DOOR_MEZZAMINE_RETURN_BACK)) {
                                StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_CLOSE_GATE;
                            } else {
                                errorCode = ErrorCode.CONNECT_DOOR_ERROR;
                                StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                            }
                        } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_CLOSE_GATE: // doi dong cong.
                        if (true == ds.WaitClose (DoorService.DoorId.DOOR_MEZZAMINE_RETURN_BACK, TIME_OUT_CLOSE_DOOR)) {
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                        } else {
                            errorCode = ErrorCode.CLOSE_DOOR_ERROR;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                        }
                        break;

                    case ReturnToGate.RETGATE_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_RETURN_TO_GATE;
                        if (errorCode == ErrorCode.RUN_OK) {
                            ReleaseProcedureHandler (this);
                        } else {
                            ErrorProcedureHandler (this);
                        }
                        ProRun = false;
                        break;
                    default:
                        break;
                }
                Thread.Sleep (5);
            }
            StateReturnToGate = ReturnToGate.RETGATE_IDLE;
        }

        public override void FinishStatesCallBack (Int32 message) {
            this.resCmd = (ResponseCommand) message;
        }
    }
}