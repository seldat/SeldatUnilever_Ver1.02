using System;
using System.Diagnostics;
using System.Threading;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;

namespace SeldatMRMS {
    public class ProcedureBufferToMachine : ProcedureControlServices {
        public class DataBufferToMachine {
            // public Pose PointCheckInBuffer;
            // public Pose PointFrontLineBuffer;
            // public PointDetectBranching PointDetectLineBranching;
            // public PointDetect PointPickPallet;
            // public Pose PointFrontLineMachine;
            // public PointDetect PointDropPallet;
        }
        // DataBufferToMachine points;
        BufferToMachine StateBufferToMachine;
        Thread ProBuferToMachine;
        RobotUnity robot;
        ResponseCommand resCmd;
        TrafficManagementService Traffic;

        public override event Action<Object> ReleaseProcedureHandler;
        public override event Action<Object> ErrorProcedureHandler;
        public ProcedureBufferToMachine (RobotUnity robot, TrafficManagementService traffiicService) : base (robot) {
            StateBufferToMachine = BufferToMachine.BUFMAC_IDLE;
            this.robot = robot;
            // this.points = new DataBufferToMachine();
            this.Traffic = traffiicService;
            procedureCode = ProcedureCode.PROC_CODE_BUFFER_TO_MACHINE;
        }

        public void Start (BufferToMachine state = BufferToMachine.BUFMAC_ROBOT_GOTO_CHECKIN_BUFFER) {
            errorCode = ErrorCode.RUN_OK;
            robot.ProcedureAs = ProcedureControlAssign.PRO_BUFFER_TO_MACHINE;
            StateBufferToMachine = state;
            ProBuferToMachine = new Thread (this.Procedure);
            ProBuferToMachine.Start (this);
            ProRun = true;
        }
        public void Destroy () {
            // StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
            ProRun = false;
        }
        public void Procedure (object ojb) {
            ProcedureBufferToMachine BfToMa = (ProcedureBufferToMachine) ojb;
            RobotUnity rb = BfToMa.robot;
            // DataBufferToMachine p = BfToMa.points;
            TrafficManagementService Traffic = BfToMa.Traffic;
            while (ProRun) {
                switch (StateBufferToMachine) {
                    case BufferToMachine.BUFMAC_IDLE:
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_GOTO_CHECKIN_BUFFER: // bắt đầu rời khỏi vùng GATE đi đến check in/ đảm bảo check out vùng cổng để robot kế tiếp vào làm việc
                        try {
                            if (rb.PreProcedureAs == ProcedureControlAssign.PRO_READY) {
                                rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                                Stopwatch sw = new Stopwatch ();
                                sw.Start ();
                                do {
                                    if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE) {
                                        resCmd = ResponseCommand.RESPONSE_NONE;
                                        rb.SendPoseStamped (BfToMa.GetCheckInBuffer ());
                                        StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER;
                                        break;
                                    } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                                        errorCode = ErrorCode.DETECT_LINE_ERROR;
                                        StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                                        break;
                                    }
                                    if (sw.ElapsedMilliseconds > TIME_OUT_WAIT_GOTO_FRONTLINE) {
                                        errorCode = ErrorCode.DETECT_LINE_ERROR;
                                        StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                                        break;
                                    }
                                    Thread.Sleep (100);
                                } while (true);
                                sw.Stop ();
                            } else {
                                rb.SendPoseStamped (BfToMa.GetCheckInBuffer ());
                                StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER;
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                        }
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER: // doi robot di den khu vuc checkin cua vung buffer
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_ZONE_BUFFER_READY;
                        }
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_ZONE_BUFFER_READY: // doi khu vuc buffer san sang de di vao
                        try {
                            if (false == Traffic.HasRobotUnityinArea (BfToMa.GetFrontLineBuffer ().Position)) {
                                rb.SendPoseStamped (BfToMa.GetFrontLineBuffer ());
                                StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER;
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                        }
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER:
                        try {
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT) {
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                rb.SendCmdAreaPallet (BfToMa.GetInfoOfPalletBuffer (PistonPalletCtrl.PISTON_PALLET_UP));
                                StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_PICKUP_PALLET_BUFFER;
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                        }
                        break;
                        // case BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_POINT_BRANCHING:
                        //     if (true == rb.CheckPointDetectLine(BfToMa.GetPointDetectBranching().xy, rb))
                        //     {
                        //         if (BfToMa.GetPointDetectBranching().brDir == BrDirection.DIR_LEFT)
                        //         {
                        //             rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_TURN_LEFT);
                        //         }
                        //         else if (BfToMa.GetPointDetectBranching().brDir == BrDirection.DIR_RIGHT)
                        //         {
                        //             rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_TURN_RIGHT);
                        //         }
                        //         StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_POINT_BRANCHING;
                        //     }
                        //     break;
                        // case BufferToMachine.BUFMAC_ROBOT_CAME_POINT_BRANCHING:  //doi bobot re
                        //     if ((resCmd == ResponseCommand.RESPONSE_FINISH_TURN_LEFT) || (resCmd == ResponseCommand.RESPONSE_FINISH_TURN_RIGHT))
                        //     {
                        //         resCmd = ResponseCommand.RESPONSE_NONE;
                        //         rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETUP);
                        //         StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_GOTO_PICKUP_PALLET_BUFFER;
                        //     }
                        //     break;
                        // case BufferToMachine.BUFMAC_ROBOT_GOTO_PICKUP_PALLET_BUFFER:
                        //     if (true == rb.CheckPointDetectLine(BfToMa.GetPointPallet(), rb))
                        //     {
                        //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                        //         StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_PICKUP_PALLET_BUFFER;
                        //     }
                        //     break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_PICKUP_PALLET_BUFFER:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETUP) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            BfToMa.UpdatePalletState (PalletStatus.F);
                            rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER;
                        } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                        }
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER: // đợi
                        try {
                            if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE) {
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                rb.SendPoseStamped (BfToMa.GetFrontLineMachine ());
                                StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET;
                            } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                                errorCode = ErrorCode.DETECT_LINE_ERROR;
                                StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                        }
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET:
                        try {
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT) {
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                rb.SendCmdAreaPallet (BfToMa.GetInfoOfPalletMachine (PistonPalletCtrl.PISTON_PALLET_DOWN));
                                StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_DROPDOWN_PALLET;
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;     
                        }
                        break;
                        // case BufferToMachine.BUFMAC_ROBOT_CAME_FRONTLINE_DROPDOWN_PALLET:  // đang trong tiến trình dò line và thả pallet
                        //     rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETDOWN);
                        //     StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_POINT_DROP_PALLET;
                        //     break;
                        // case BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_POINT_DROP_PALLET:
                        //     if (true == rb.CheckPointDetectLine(BfToMa.GetPointPallet(), rb))
                        //     {
                        //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                        //         StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_DROPDOWN_PALLET;
                        //     }
                        //     break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_DROPDOWN_PALLET:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETDOWN) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            BfToMa.UpdatePalletState (PalletStatus.W);
                            rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_FRONTLINE;
                        } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                        }
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_FRONTLINE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                        } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                        }
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_BUFFER_TO_MACHINE;
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
            StateBufferToMachine = BufferToMachine.BUFMAC_IDLE;
        }
        public override void FinishStatesCallBack (Int32 message) {
            this.resCmd = (ResponseCommand) message;
        }
    }
}