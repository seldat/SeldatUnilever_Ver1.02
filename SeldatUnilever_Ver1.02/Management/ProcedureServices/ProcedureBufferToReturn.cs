﻿using System;
using System.Diagnostics;
using System.Threading;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;

namespace SeldatMRMS {
    public class ProcedureBufferToReturn : ProcedureControlServices {
        public struct DataForkBufferToReturn {
            // public Pose PointCheckInBuffer;
            // public Pose PointFrontLineBuffer;
            // public PointDetectBranching PointDetectLineBranching;
            // public PointDetect PointPickPallet;
            // public Pose PointCheckInReturn;
            // public Pose PointFrontLineReturn;
            // public PointDetect PointDropPallet;
        }
        // DataForkBufferToReturn points;
        BufferToReturn StateBufferToReturn;
        Thread ProBuferToReturn;
        public RobotUnity robot;
        ResponseCommand resCmd;
        TrafficManagementService Traffic;
        public override event Action<Object> ReleaseProcedureHandler;
        // public override event Action<Object> ErrorProcedureHandler;
        public ProcedureBufferToReturn (RobotUnity robot, TrafficManagementService traffiicService) : base (robot) {
            StateBufferToReturn = BufferToReturn.BUFRET_IDLE;
            this.robot = robot;
            // this.points = new DataForkBufferToReturn();
            this.Traffic = traffiicService;
            procedureCode = ProcedureCode.PROC_CODE_BUFFER_TO_RETURN;
        }

        public void Start (BufferToReturn state = BufferToReturn.BUFRET_ROBOT_GOTO_CHECKIN_BUFFER) {
            errorCode = ErrorCode.RUN_OK;
            robot.ProcedureAs = ProcedureControlAssign.PRO_BUFFER_TO_RETURN;
            StateBufferToReturn = state;
            ProBuferToReturn = new Thread (this.Procedure);
            ProBuferToReturn.Start (this);
            ProRun = true;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
        }
        public void Destroy () {
            // StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_RELEASED;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            ProRun = false;
        }
        public void Procedure (object ojb) {
            ProcedureBufferToReturn BfToRe = (ProcedureBufferToReturn) ojb;
            RobotUnity rb = BfToRe.robot;
            TrafficManagementService Traffic = BfToRe.Traffic;
            Debug(this,"Start");
            while (ProRun) {
                switch (StateBufferToReturn) {
                    case BufferToReturn.BUFRET_IDLE:
                        Debug(this,"BUFRET_IDLE");
                        break;
                    case BufferToReturn.BUFRET_ROBOT_GOTO_CHECKIN_BUFFER: // bắt đầu rời khỏi vùng GATE đi đến check in/ đảm bảo check out vùng cổng để robot kế tiếp vào làm việc
                        try {
                            if (rb.PreProcedureAs == ProcedureControlAssign.PRO_READY) {
                                rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                                Stopwatch sw = new Stopwatch ();
                                sw.Start ();
                                do {
                                    if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE) {
                                        resCmd = ResponseCommand.RESPONSE_NONE;
                                        rb.SendPoseStamped (BfToRe.GetCheckInBuffer ());
                                        StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER;
                                        Debug(this,"BUFRET_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER");
                                        break;
                                    } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                                        errorCode = ErrorCode.DETECT_LINE_ERROR;
                                        CheckUserHandleError(this);
                                        break;
                                    }
                                    if (sw.ElapsedMilliseconds > TIME_OUT_WAIT_GOTO_FRONTLINE) {
                                        errorCode = ErrorCode.DETECT_LINE_ERROR;
                                        CheckUserHandleError(this);
                                        break;
                                    }
                                    Thread.Sleep (100);
                                } while (true);
                                sw.Stop ();
                            } else {
                                rb.SendPoseStamped (BfToRe.GetCheckInBuffer ());
                                StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER;
                                Debug(this,"BUFRET_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER");
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }

                        break;
                    case BufferToReturn.BUFRET_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER: // doi robot di den khu vuc checkin cua vung buffer
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_WAITTING_ZONE_BUFFER_READY;
                            Debug(this,"BUFRET_ROBOT_WAITTING_ZONE_BUFFER_READY");
                        }
                        break;
                    case BufferToReturn.BUFRET_ROBOT_WAITTING_ZONE_BUFFER_READY: // doi khu vuc buffer san sang de di vao
                        try {
                            if (false == Traffic.HasRobotUnityinArea (BfToRe.GetFrontLineBuffer ().Position)) {
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                                rb.SendPoseStamped (BfToRe.GetFrontLineBuffer ());
                                StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER;
                                Debug(this,"BUFRET_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER");
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    case BufferToReturn.BUFRET_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER:
                        try {
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT) {
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                rb.SendCmdAreaPallet (BfToRe.GetInfoOfPalletBuffer (PistonPalletCtrl.PISTON_PALLET_UP));
                                StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_WAITTING_PICKUP_PALLET_BUFFER;
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                Debug(this,"BUFRET_ROBOT_WAITTING_PICKUP_PALLET_BUFFER");
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                        // case BufferToReturn.BUFRET_ROBOT_WAITTING_GOTO_POINT_BRANCHING:
                        //     if (true == rb.CheckPointDetectLine(BfToRe.GetPointDetectBranching().xy, rb))
                        //     {
                        //         if (BfToRe.GetPointDetectBranching().brDir == BrDirection.DIR_LEFT)
                        //         {
                        //             rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_TURN_LEFT);
                        //         }
                        //         else if (BfToRe.GetPointDetectBranching().brDir == BrDirection.DIR_RIGHT)
                        //         {
                        //             rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_TURN_RIGHT);
                        //         }
                        //         StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_WAITTING_GOTO_POINT_BRANCHING;
                        //     }
                        //     break;
                        // case BufferToReturn.BUFRET_ROBOT_CAME_POINT_BRANCHING:  //doi bobot re
                        //     if ((resCmd == ResponseCommand.RESPONSE_FINISH_TURN_LEFT) || (resCmd == ResponseCommand.RESPONSE_FINISH_TURN_RIGHT))
                        //     {
                        //         resCmd = ResponseCommand.RESPONSE_NONE;
                        //         rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETUP);
                        //         StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_GOTO_PICKUP_PALLET_BUFFER;
                        //     }
                        //     break;
                        // case BufferToReturn.BUFRET_ROBOT_GOTO_PICKUP_PALLET_BUFFER:
                        //     if (true == rb.CheckPointDetectLine(BfToRe.GetPointPallet(), rb))
                        //     {
                        //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                        //         StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_WAITTING_PICKUP_PALLET_BUFFER;
                        //     }
                        // break;
                    case BufferToReturn.BUFRET_ROBOT_WAITTING_PICKUP_PALLET_BUFFER:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETUP) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            BfToRe.UpdatePalletState (PalletStatus.F);
                           // rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER;
                            Debug(this,"BUFRET_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER");
                        } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case BufferToReturn.BUFRET_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER: // đợi
                        try {
                            if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE) {
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                                rb.SendPoseStamped (BfToRe.GetCheckInReturn ());
                                StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_GOTO_CHECKIN_RETURN;
                                Debug(this,"BUFRET_ROBOT_GOTO_CHECKIN_RETURN");
                            } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                                errorCode = ErrorCode.DETECT_LINE_ERROR;
                                CheckUserHandleError(this);
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    case BufferToReturn.BUFRET_ROBOT_GOTO_CHECKIN_RETURN: // dang di
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            rb.UpdateRiskAraParams(0,rb.properties.L2,rb.properties.WS,rb.properties.DistInter);
                            StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_CAME_CHECKIN_RETURN;
                            Debug(this,"BUFRET_ROBOT_CAME_CHECKIN_RETURN");
                        }
                        break;
                    case BufferToReturn.BUFRET_ROBOT_CAME_CHECKIN_RETURN: // đã đến vị trí
                        try {
                            if (false == Traffic.HasRobotUnityinArea (BfToRe.GetFrontLineReturn ().Position)) {
                                rb.UpdateRiskAraParams(40,rb.properties.L2,rb.properties.WS,rb.properties.DistInter);
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                                rb.SendPoseStamped (BfToRe.GetFrontLineReturn ());
                                StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET;
                                Debug(this,"BUFRET_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET");
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    case BufferToReturn.BUFRET_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET:
                        try {
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT) {
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                rb.SendCmdAreaPallet (BfToRe.GetInfoOfPalletReturn (PistonPalletCtrl.PISTON_PALLET_DOWN));
                                StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_WAITTING_DROPDOWN_PALLET;
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                Debug(this,"BUFRET_ROBOT_WAITTING_DROPDOWN_PALLET");
                            }
                        } catch (System.Exception) {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                        // case BufferToReturn.BUFRET_ROBOT_CAME_FRONTLINE_DROPDOWN_PALLET:  // đang trong tiến trình dò line và thả pallet
                        //     rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETDOWN);
                        //     StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_WAITTING_GOTO_POINT_DROP_PALLET;
                        //     break;
                        // case BufferToReturn.BUFRET_ROBOT_WAITTING_GOTO_POINT_DROP_PALLET:
                        //     if (true == rb.CheckPointDetectLine(BfToRe.GetPointPallet(), rb))
                        //     {
                        //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                        //         StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_WAITTING_DROPDOWN_PALLET;
                        //     }
                        //     break;
                    case BufferToReturn.BUFRET_ROBOT_WAITTING_DROPDOWN_PALLET:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETDOWN) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            BfToRe.UpdatePalletState (PalletStatus.W);
                          //  rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_WAITTING_GOTO_FRONTLINE;
                            Debug(this,"BUFRET_ROBOT_WAITTING_GOTO_FRONTLINE");
                        } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case BufferToReturn.BUFRET_ROBOT_WAITTING_GOTO_FRONTLINE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE) {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            StateBufferToReturn = BufferToReturn.BUFRET_ROBOT_RELEASED;
                            Debug(this,"BUFRET_ROBOT_RELEASED");
                        } else if (resCmd == ResponseCommand.RESPONSE_ERROR) {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case BufferToReturn.BUFRET_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_BUFFER_TO_RETURN;
                        // if (errorCode == ErrorCode.RUN_OK) {
                            ReleaseProcedureHandler (this);
                        // } else {
                        //     ErrorProcedureHandler (this);
                        // }
                        ProRun = false;
                        Debug(this,"RELEASED");
                        break;
                    default:
                        break;
                }
                Thread.Sleep (5);
            }
            StateBufferToReturn = BufferToReturn.BUFRET_IDLE;
        }
        public override void FinishStatesCallBack (Int32 message) {
            this.resCmd = (ResponseCommand) message;
        }
    }
}
