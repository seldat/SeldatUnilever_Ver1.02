﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DoorControllerService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeldatMRMS.Management;
using SeldatMRMS.Management.DoorServices;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using SelDatUnilever_Ver1._00.Management.DeviceManagement;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

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
        ForkLift StateForkLift;
        Thread ProForkLift;
        public RobotUnity robot;
        public DoorService door;
        ResponseCommand resCmd;
        TrafficManagementService Traffic;
        private DeviceRegistrationService deviceService;

        public override event Action<Object> ReleaseProcedureHandler;

        public void Registry(DeviceRegistrationService deviceService)
        {
            this.deviceService = deviceService;
        }
        // public override event Action<Object> ErrorProcedureHandler;
        public ProcedureForkLiftToBuffer(RobotUnity robot, DoorManagementService doorservice, TrafficManagementService trafficService) : base(robot)
        {
            StateForkLift = ForkLift.FORBUF_IDLE;
            resCmd = ResponseCommand.RESPONSE_NONE;
            this.robot = robot;
            // this.points = new DataForkLiftToBuffer();
            door = doorservice.DoorMezzamineUp;
            // this.points.PointFrontLineGate = this.door.config.PointFrontLine;
            // this.points.PointPickPalletIn = this.door.config.PointOfPallet;
            this.Traffic = trafficService;
            procedureCode = ProcedureCode.PROC_CODE_FORKLIFT_TO_BUFFER;

        }
        public void Start(ForkLift state = ForkLift.FORBUF_ROBOT_GOTO_CHECKIN_GATE)
        {
            // public void Start (ForkLiftToBuffer state = ForkLiftToBuffer.FORBUF_ROBOT_RELEASED) {
            errorCode = ErrorCode.RUN_OK;
            robot.ProcedureAs = ProcedureControlAssign.PRO_FORKLIFT_TO_BUFFER;
            StateForkLift = state;
            ProForkLift = new Thread(this.Procedure);
            ProForkLift.Start(this);
            ProRun = true;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
        }
        public void Destroy()
        {
            
          //  FreePlanedBuffer();
            // StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_RELEASED;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            ProRun = false;
            UpdateInformationInProc(this, ProcessStatus.F);
            order.status = StatusOrderResponseCode.ROBOT_ERROR;
            selectHandleError = SelectHandleError.CASE_ERROR_EXIT;
            
            this.robot.DestroyRegistrySolvedForm();
        }


        public void FreePlanedBuffer()
        {


            String url = Global_Object.url + "pallet/updatePalletStatus";

            int _palletId = GetPalletId(order.planId);
            if (_palletId > 0)
            {
                dynamic product = new JObject();
                product.palletId = _palletId;
                product.planId = order.planId;
                product.palletStatus = PalletStatus.F.ToString();
                product.updUsrId = Global_Object.userLogin;
                var data = RequestDataProcedure(product.ToString(), url);
                if (data.Result != "")
                {
                  //  ErrorHandler(ProcedureMessages.ProcMessage.MESSAGE_ERROR_UPDATE_PALLETSTATUS);
                }
            }

        }
        // lấy pallet info cho viec free the plan planed
        public int GetPalletId(int planId)
        {
            int palletId = -1;
            String collectionData = RequestDataProcedure(order.dataRequest, Global_Object.url + "plan/getListPlanPallet");
            if (collectionData.Length > 0)
            {
                try
                {
                    JArray results = JArray.Parse(collectionData);
                    foreach (var result in results)
                    {
                        int temp_planId = (int)result["planId"];
                        if (temp_planId == planId)
                        {
                            var bufferResults = result["buffers"][0];
                            var palletInfo = bufferResults["pallets"][0];
                            palletId = (int)palletInfo["palletId"];
                            break;
                        }
                    }
                }
                catch { }

            }
            return palletId;
        }
        public void Procedure(object ojb)
        {
            ProcedureForkLiftToBuffer FlToBuf = (ProcedureForkLiftToBuffer)ojb;
            RobotUnity rb = FlToBuf.robot;
            // DataForkLiftToBuffer p = FlToBuf.points;
            DoorService ds = FlToBuf.door;
            TrafficManagementService Traffic = FlToBuf.Traffic;
            ForkLiftToMachineInfo flToMachineInfo = new ForkLiftToMachineInfo();
            robot.ShowText(" Start -> " + procedureCode);
            while (ProRun)
            {
                switch (StateForkLift)
                {
                    case ForkLift.FORBUF_IDLE:
                        robot.ShowText("FORBUF_IDLE");
                        break;
                    case ForkLift.FORBUF_ROBOT_GOTO_CHECKIN_GATE: //gui toa do di den khu vuc checkin cong
                        if (rb.PreProcedureAs == ProcedureControlAssign.PRO_READY)
                        {
                            if (rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE))
                            {
                                Stopwatch sw = new Stopwatch();
                                sw.Start();
                                do
                                {
                                    if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                                    {
                                        resCmd = ResponseCommand.RESPONSE_NONE;
                                        if (Traffic.RobotIsInArea("OPA4", rb.properties.pose.Position))
                                        {
                                            if (rb.SendPoseStamped(ds.config.PointFrontLine))
                                            {
                                                StateForkLift = ForkLift.FORBUF_ROBOT_CAME_CHECKIN_GATE;
                                                robot.ShowText("FORBUF_ROBOT_CAME_CHECKIN_GATE");
                                            }
                                        }
                                        else
                                        {
                                            if (rb.SendPoseStamped(ds.config.PointCheckInGate))
                                            {
                                                StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE;
                                                robot.ShowText("FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE");
                                            }
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
                        }
                        else
                        {
                            if (Traffic.RobotIsInArea("OPA4", rb.properties.pose.Position))
                            {
                                if (rb.SendPoseStamped(ds.config.PointFrontLine))
                                {
                                    StateForkLift = ForkLift.FORBUF_ROBOT_CAME_CHECKIN_GATE;
                                    robot.ShowText("FORBUF_ROBOT_CAME_CHECKIN_GATE");
                                }
                            }
                            else
                            {

                                if (rb.SendPoseStamped(ds.config.PointCheckInGate))
                                {
                                    StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE;
                                    robot.ShowText("FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE");
                                }
                            }
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE:
                       // if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT && robot.ReachedGoal())
                        if ( robot.ReachedGoal())
                        {
                            robot.SetTrafficAtCheckIn(true);
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            rb.UpdateRiskAraParams(0, rb.properties.L2, rb.properties.WS, rb.properties.DistInter);
                            StateForkLift = ForkLift.FORBUF_ROBOT_CAME_CHECKIN_GATE;
                            robot.ShowText("FORBUF_ROBOT_CAME_CHECKIN_GATE");
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_CAME_CHECKIN_GATE: // đã đến vị trí, kiem tra va cho khu vuc cong san sang de di vao.
                                                                          // robot.ShowText( "FORBUF_ROBOT_WAITTING_GOTO_GATE ===> FLAG " + Traffic.HasRobotUnityinArea(ds.config.PointFrontLine.Position));
                        if (false == Traffic.HasRobotUnityinArea(ds.config.PointFrontLine.Position))
                        {
                            robot.SetTrafficAtCheckIn(false);
                            rb.UpdateRiskAraParams(40, rb.properties.L2, rb.properties.WS, rb.properties.DistInter);
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            if (rb.SendPoseStamped(ds.config.PointFrontLine))
                            {
                                StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOTO_GATE;
                                robot.ShowText("FORBUF_ROBOT_WAITTING_GOTO_GATE");
                            }
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_GOTO_GATE:
                        //if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT && robot.ReachedGoal())
                        if ( robot.ReachedGoal())
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            StateForkLift = ForkLift.FORBUF_ROBOT_CAME_GATE_POSITION;
                            robot.ShowText("FORBUF_ROBOT_CAME_GATE_POSITION");
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_CAME_GATE_POSITION: // da den khu vuc cong , gui yeu cau mo cong.
                        if (ds.Open(DoorService.DoorType.DOOR_BACK))
                        {
                            StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_OPEN_DOOR;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_OPEN_DOOR");
                        }
                        else
                        {
                            errorCode = ErrorCode.CONNECT_DOOR_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_OPEN_DOOR: //doi mo cong
                        if (true == ds.WaitOpen(DoorService.DoorType.DOOR_BACK, TIME_OUT_OPEN_DOOR))
                        {
                            StateForkLift = ForkLift.FORBUF_ROBOT_OPEN_DOOR_SUCCESS;
                            robot.ShowText("FORBUF_ROBOT_OPEN_DOOR_SUCCESS");
                        }
                        else
                        {
                            errorCode = ErrorCode.OPEN_DOOR_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_OPEN_DOOR_SUCCESS: // mo cua thang cong ,gui toa do line de robot di vao gap hang
                        // rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETUP);
                        if (rb.SendCmdAreaPallet(ds.config.infoPallet))
                        {
                            StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_PICKUP_PALLET_IN;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_PICKUP_PALLET_IN");
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_PICKUP_PALLET_IN: // doi robot gap hang
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETUP)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            // FlToBuf.UpdatePalletState(PalletStatus.F);
                            //   rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            if (ds.Close(DoorService.DoorType.DOOR_BACK))
                            {
                                StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_CLOSE_GATE;
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
                    case ForkLift.FORBUF_ROBOT_WAITTING_CLOSE_GATE: // doi dong cong.
                        try
                        {
                            if (true == ds.WaitClose(DoorService.DoorType.DOOR_BACK, TIME_OUT_CLOSE_DOOR))
                            {
                                flToMachineInfo = GetPriorityTaskForkLiftToMachine(order.productId);
                                if (flToMachineInfo == null)
                                {
                                    rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                                    rb.SendPoseStamped(FlToBuf.GetCheckInBuffer(true));
                                    StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER;
                                    robot.ShowText("FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER");
                                }
                                else {

                                    FreePlanedBuffer();
                                    StateForkLift = ForkLift.FORMAC_ROBOT_GOTO_FRONTLINE_MACHINE;
                                    robot.ShowText("FORMAC_ROBOT_GOTO_FRONTLINE_MACHINE");
                                }
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
                    case ForkLift.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER: // doi robot di den khu vuc checkin cua vung buffer
                       // if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT && robot.ReachedGoal())
                            if (robot.ReachedGoal())
                            {
                            robot.SetTrafficAtCheckIn(true);
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_ZONE_BUFFER_READY;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_ZONE_BUFFER_READY");
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_ZONE_BUFFER_READY: // doi khu vuc buffer san sang de di vao
                        try
                        {
                            if (false == Traffic.HasRobotUnityinArea(FlToBuf.GetAnyPointInBuffer(true).Position))
                            {
                                robot.SetTrafficAtCheckIn(false);
                                // createPlanBuffer();
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = false;

                                if (rb.SendPoseStamped(FlToBuf.GetFrontLineBuffer(true)))
                                {
                                    StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER;
                                    robot.ShowText("FORBUF_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER:
                        try
                        {
                          //  if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT && robot.ReachedGoal())
                                if (robot.ReachedGoal())
                                {
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                if (rb.SendCmdAreaPallet(FlToBuf.GetInfoOfPalletBuffer(PistonPalletCtrl.PISTON_PALLET_DOWN, true)))
                                {
                                    // rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_FORWARD_DIRECTION);
                                    rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                    StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_DROPDOWN_PALLET_BUFFER;
                                    robot.ShowText("FORBUF_ROBOT_WAITTING_DROPDOWN_PALLET_BUFFER");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;

                    case ForkLift.FORBUF_ROBOT_WAITTING_DROPDOWN_PALLET_BUFFER:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETDOWN)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            FlToBuf.UpdatePalletState(PalletStatus.W);
                            //   rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER: // đợi
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            StateForkLift = ForkLift.FORBUF_ROBOT_RELEASED;
                            robot.ShowText("FORBUF_ROBOT_RELEASED");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_FORKLIFT_TO_BUFFER;
                        // if (errorCode == ErrorCode.RUN_OK) {
                        ReleaseProcedureHandler(this);
                        // } else {
                        //     ErrorProcedureHandler (this);
                        // }
                        ProRun = false;
                        robot.ShowText("RELEASED");
                        UpdateInformationInProc(this, ProcessStatus.S);
                        order.status = StatusOrderResponseCode.FINISHED;
                        break;


                    ///////////////////////////////////////////////////////
                    case ForkLift.FORMAC_ROBOT_GOTO_FRONTLINE_MACHINE:
                        try
                        {
                            robot.TurnOnCtrlSelfTraffic(true);
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            if (rb.SendPoseStamped(flToMachineInfo.frontLinePose))
                            {
                                StateForkLift = ForkLift.FORMAC_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE;
                                robot.ShowText("FORMAC_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE");
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;

                    case ForkLift.FORMAC_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE:
                        try
                        {
                            //  if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT && robot.ReachedGoal())
                            if (robot.ReachedGoal())
                            {
                                robot.TurnOnCtrlSelfTraffic(false);
                                if (rb.SendCmdAreaPallet(flToMachineInfo.infoPallet))
                                {
                                    rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                    StateForkLift = ForkLift.FORMAC_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE;
                                    robot.ShowText("FORMAC_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;

                    case ForkLift.FORMAC_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETDOWN)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            // FlToBuf.UpdatePalletState(PalletStatus.W);
                            //   rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateForkLift = ForkLift.FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE;
                            robot.ShowText("FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE: // đợi
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            StateForkLift = ForkLift.FORMAC_ROBOT_RELEASED;
                            robot.ShowText("FORMAC_ROBOT_RELEASED");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORMAC_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
                        robot.TurnOnCtrlSelfTraffic(true);
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_FORKLIFT_TO_MACHINE;
                        // if (errorCode == ErrorCode.RUN_OK) {
                        ReleaseProcedureHandler(this);
                        // } else {
                        //     ErrorProcedureHandler (this);
                        // }
                        ProRun = false;
                        robot.ShowText("RELEASED");
                        UpdateInformationInProc(this, ProcessStatus.S);
                        order.status = StatusOrderResponseCode.FINISHED;
                        break;
                    //////////////////////////////////////////////////////
                    default:
                        break;
                }
                Thread.Sleep(5);
            }
            StateForkLift = ForkLift.FORBUF_IDLE;
        }
        protected override void CheckUserHandleError(object obj)
        {
            CheckUserHandleError(this);
        }
        public override void FinishStatesCallBack(Int32 message)
        {
            this.resCmd = (ResponseCommand)message;
        }
        public class ForkLiftToMachineInfo
        {
            public Pose frontLinePose;
            public String infoPallet;
        }
        public ForkLiftToMachineInfo GetPriorityTaskForkLiftToMachine(int productId)
        {
            ForkLiftToMachineInfo forkLiftToMachineInfo = null;
            bool onHasOrder = false;
            foreach (DeviceItem deviceItem in deviceService.GetDeviceItemList())
            {
                foreach (OrderItem order in deviceItem.PendingOrderList)
                {
                    if (order.typeReq == TyeRequest.TYPEREQUEST_BUFFER_TO_MACHINE)
                    {
                        if (order.productId == productId)
                        {
                            forkLiftToMachineInfo = new ForkLiftToMachineInfo();
                            forkLiftToMachineInfo.frontLinePose = order.palletAtMachine.linePos;
                            JInfoPallet infoPallet = new JInfoPallet();

                            infoPallet.pallet = PistonPalletCtrl.PISTON_PALLET_DOWN; /* dropdown */
                            infoPallet.bay = order.palletAtMachine.bay;
                            infoPallet.hasSubLine = "no"; /* no */
                            infoPallet.dir_main = (TrafficRobotUnity.BrDirection)order.palletAtMachine.directMain;
                            infoPallet.dir_sub = (TrafficRobotUnity.BrDirection)order.palletAtMachine.directSub;
                            infoPallet.dir_out = (TrafficRobotUnity.BrDirection)order.palletAtMachine.directOut;
                            infoPallet.line_ord = order.palletAtMachine.line_ord;
                            infoPallet.row = order.palletAtMachine.row;

                            forkLiftToMachineInfo.infoPallet = JsonConvert.SerializeObject(infoPallet);
                            order.status = StatusOrderResponseCode.CHANGED_FORKLIFT;
                            onHasOrder = true;
                            deviceItem.PendingOrderList.Remove(order);
                            break;
                        }

                    }
                }
                if (onHasOrder)
                    break;
            }
            return forkLiftToMachineInfo;
        }
    }
}
