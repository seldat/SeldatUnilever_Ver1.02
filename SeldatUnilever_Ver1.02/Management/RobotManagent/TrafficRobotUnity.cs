using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SelDatUnilever_Ver1._00.Management.TrafficManager.TrafficRounterService;

namespace SeldatMRMS.Management
{
    public class TrafficRobotUnity:RobotUnityService
    {
        public class PriorityLevel
        {
            public PriorityLevel()
            {
                this.IndexOnMainRoad = 0;
                this.OnAuthorizedPriorityProcedure = false;
            }
            public int IndexOnMainRoad { get; set; } //  Index on Road;
            public bool OnAuthorizedPriorityProcedure { get; set; }

        }
        public enum TrafficBehaviorState
        {
            HEADER_TOUCH_TAIL,
            HEADER_TOUCH_HEADER,
            HEADER_TOUCH_SIDE,
            HEADER_TOUCH_NOTOUCH
        }
        // public enum MvDirection{
        //     INCREASE_X = 0,
		// 	INCREASE_Y,
		// 	DECREASE_X,
		// 	DECREASE_Y
		// // }
         public enum BrDirection{
            FORWARD = 0,
             DIR_LEFT,
             DIR_RIGHT
		 }
        // public class PointDetect {
        //     public Point p;
        //     public MvDirection mvDir;
        //     public PointDetect(Point p, MvDirection mv)
        //     {
        //         this.p = p;
        //         mvDir = mv;
        //     }
        // }

        // public class PointDetectBranching{
        //     public PointDetect xy;
        //     public BrDirection brDir;
        // }
        public enum PistonPalletCtrl
        {
            PISTON_PALLET_UP=0,
            PISTON_PALLET_DOWN
        }
        public class JInfoPallet{
            public PistonPalletCtrl pallet;
            public BrDirection dir_main;
            public Int32 bay;
            public String hasSubLine;
            public BrDirection dir_sub;
            public BrDirection dir_out;
            public Int32 row;
            // public Int32 palletId;
        }
        private List<RobotUnity> RobotUnitylist;
        private bool flagSupervisorTraffic;
        private Dictionary<String,RobotUnity> RobotUnityRiskList=new Dictionary<string, RobotUnity>();
        private TrafficBehaviorState TrafficBehaviorStateTracking;
        private TrafficManagementService trafficManagementService;
        public TrafficRobotUnity() : base() {
            TurnOnSupervisorTraffic(false);
            RobotUnitylist = new List<RobotUnity>();
            prioritLevel = new PriorityLevel();
        }
        public PriorityLevel prioritLevel;
        public void RegisteRobotInAvailable(Dictionary<String,RobotUnity> RobotUnitylistdc)
        {
            foreach(var r in RobotUnitylistdc.Values)
              this.RobotUnitylist.Add(r);
            TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_NOTOUCH;
        }
        public void Registry(TrafficManagementService trafficManagementService)
        {
            this.trafficManagementService = trafficManagementService;
        }
        public RobotUnity CheckIntersection()
        {
            RobotUnity robot = null;
            if(RobotUnityRiskList.Count>0)
            {
                foreach(RobotUnity r in RobotUnityRiskList.Values)
                {
                   bool onTouch= FindHeaderIntersectsFullRiskArea(this.TopHeader()) | FindHeaderIntersectsFullRiskArea(this.MiddleHeader()) | FindHeaderIntersectsFullRiskArea(this.BottomHeader());
                    if(onTouch)
                    {
                        robot = r;
                        break;
                    }
                }
            }
            return robot;
        }
        public bool CheckSafeDistance() // KIểm tra khoản cách an toàn/ nếu đang trong vùng close với robot khác thì giảm tốc độ, chuyển sang chế độ dò risk area
        {
            bool iscloseDistance = false;
            foreach(RobotUnity r in RobotUnitylist)
            {
                bool onFound = r.FindHeaderIsCloseRiskArea(this.properties.pose.Position);
                if (onFound)
                {
                    // if robot in list is near but add in risk list robot

                    SetSpeed(RobotSpeedLevel.ROBOT_SPEED_SLOW);
                    if(!RobotUnityRiskList.ContainsKey(r.properties.NameId))
                    {
                        RobotUnityRiskList.Add(r.properties.NameId,r);
                    }
                    // reduce speed robot control
                    iscloseDistance = true;
                }
                else
                {
                    // if robot in list is far but before registe in list, must remove in list
                    RemoveRiskList(r.properties.NameId);
                }
            }
            return iscloseDistance;
        }
        public void RemoveRiskList(String NameID)
        {
            if(RobotUnityRiskList.ContainsKey(NameID))
            {
                RobotUnityRiskList.Remove(NameID);
            }
        }
        public void DetectTouchedPosition(RobotUnity robot) // determine traffic state
        {
                if (robot.FindHeaderIntersectsRiskAreaHeader(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaHeader(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaHeader(this.BottomHeader()))
                {
                    TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_HEADER;
                }
                else if (robot.FindHeaderIntersectsRiskAreaTail(this.TopHeader())|| robot.FindHeaderIntersectsRiskAreaTail(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaTail(this.BottomHeader()))
                {
                    TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_TAIL;
                }
                else if (robot.FindHeaderIntersectsRiskAreaRightSide(this.TopHeader())|| robot.FindHeaderIntersectsRiskAreaRightSide(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaRightSide(this.BottomHeader()))
                {
                    TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_SIDE;
                }
                else if (robot.FindHeaderIntersectsRiskAreaLeftSide(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaLeftSide(this.MiddleHeader()) || robot.FindHeaderIntersectsRiskAreaLeftSide(this.BottomHeader()))
                {
                    TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_SIDE;
                }
          
        }
        public void TrafficBehavior(RobotUnity robot)
        {
            switch(TrafficBehaviorStateTracking)
            {
                case TrafficBehaviorState.HEADER_TOUCH_NOTOUCH:
                    SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                    // robot speed normal;
                    break;
                case TrafficBehaviorState.HEADER_TOUCH_HEADER:
                    // Find condition priority
                    // index level of road
                    // procedure Flag is set
                    if(prioritLevel.IndexOnMainRoad==robot.prioritLevel.IndexOnMainRoad)
                    {
                       if(robot.prioritLevel.OnAuthorizedPriorityProcedure)
                        {
                            SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                        }
                        else
                        {
                            SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                        }
                    }
                    else if(prioritLevel.IndexOnMainRoad < robot.prioritLevel.IndexOnMainRoad)
                    {
                        SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                    }
                    break;
                case TrafficBehaviorState.HEADER_TOUCH_TAIL:
                    SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                    // robot stop
                    break;
                case TrafficBehaviorState.HEADER_TOUCH_SIDE:
                    SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                    break;

            }
        }
        public void TurnOnSupervisorTraffic(bool flagtraffic)
        {
            flagSupervisorTraffic = flagtraffic;
        }
        public override void TrafficUpdate() {

                prioritLevel.IndexOnMainRoad = trafficManagementService.FindIndexZoneRegister(properties.pose.Position);
                // cập nhật vùng riskzone // update vùng risk area cho robot
                RiskZoneRegister rZR = trafficManagementService.FindRiskZone(properties.pose.Position, IndexZoneDefaultFind.ZONE_OP,true);
                if (rZR != null)
                {
                    UpdateRiskAraParams(rZR.L1, rZR.L2, rZR.WS, rZR.distance);
                }
                else
                {
                    UpdateRiskAraParams(DfL1, DfL2, DfWS, DfDistanceInter);
                }
                // giám sát an toàn
                SupervisorTraffic();
        
        }
        protected override void SupervisorTraffic() {
            if (flagSupervisorTraffic)
            {
                if (CheckSafeDistance())
                {
                    RobotUnity robot = CheckIntersection();
                    if (robot != null)
                    {
                        DetectTouchedPosition(robot);
                        TrafficBehavior(robot);
                    }
                }
                else
                {
                    if (RobotUnityRiskList.Count > 0)
                    {
                        RobotUnityRiskList.Clear();
                    }
                    TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_NOTOUCH;
                    TrafficBehavior(null);
                }
            }
        }

        // public Boolean CheckZoneReady(Pose point){
		// 	/*hien thuc giup em*/
		// 	return true;
        // }

        // public Boolean CheckPointDetectLine(PointDetect p, RobotUnity rb)
		// {
        //     Boolean ret = false;
        //     switch (p.mvDir)
        //     {
        //         case MvDirection.INCREASE_X:
        //             if(rb.properties.pose.Position.X > p.p.X){
        //                 ret = true;
        //             }
        //             else{
        //                 ret = false;
        //             }
        //             break;
        //         case MvDirection.INCREASE_Y:
        //             if(rb.properties.pose.Position.Y > p.p.Y){
        //                 ret = true;
        //             }
        //             else{
        //                 ret = false;
        //             }
        //             break;
        //         case MvDirection.DECREASE_X:
        //             if(rb.properties.pose.Position.X < p.p.X){
        //                 ret = true;
        //             }
        //             else{
        //                 ret = false;
        //             }
        //             break;
        //         case MvDirection.DECREASE_Y:
        //             if(rb.properties.pose.Position.Y < p.p.Y){
        //                 ret = true;
        //             }
        //             else{
        //                 ret = false;
        //             }
        //             break;
        //         default:
        //             break;
        //     }
        //     return ret;
        // }
    }
}
