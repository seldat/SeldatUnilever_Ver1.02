using SeldatMRMS.Management.RobotManagent;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            public Int32 bay;
            public String hasSubLine;
            public BrDirection direction;
            public Int32 row;
        }
        private List<RobotUnity> RobotUnitylist;
        private bool flagSupervisorTraffic;
        private Dictionary<String,RobotUnity> RobotUnityRiskList=new Dictionary<string, RobotUnity>();
        private TrafficBehaviorState TrafficBehaviorStateTracking;
        public TrafficRobotUnity() : base() {
            TurnOnSupervisorTraffic(false);
        }
        public PriorityLevel PrioritLevelRegister;
        public void RegisteRobotInAvailable(List<RobotUnity> RobotUnitylist)
        {
            this.RobotUnitylist = RobotUnitylist;
            TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_NOTOUCH;
            PrioritLevelRegister = new PriorityLevel();
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
                    if(!RobotUnityRiskList.ContainsKey(r.properties.NameID))
                    {
                        RobotUnityRiskList.Add(r.properties.NameID,r);
                    }
                    // reduce speed robot control
                    iscloseDistance = true;
                }
                else
                {
                    // if robot in list is far but before registe in list, must remove in list
                    RemoveRiskList(r.properties.NameID);
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
        public void TrafficBehavior()
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
        protected override void SupervisorTraffic() {
            if (flagSupervisorTraffic)
            {
                if (CheckSafeDistance())
                {
                    RobotUnity robot = CheckIntersection();
                    if (robot != null)
                    {
                        DetectTouchedPosition(robot);
                        TrafficBehavior();
                    }
                }
                else
                {
                    if (RobotUnityRiskList.Count > 0)
                    {
                        RobotUnityRiskList.Clear();
                    }
                    TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_NOTOUCH;
                    TrafficBehavior();
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
