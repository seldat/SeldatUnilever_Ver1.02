using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
            public int line_ord;
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

            new Thread(TrafficUpdate).Start(); 
        }
        public PriorityLevel prioritLevel;
        public void RegisteRobotInAvailable(Dictionary<String,RobotUnity> RobotUnitylistdc)
        {
            foreach (var r in RobotUnitylistdc.Values)
            {
                if(!r.properties.NameId.Equals(this.properties.NameId))
                    this.RobotUnitylist.Add(r);
            }
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
                    Point thCV = Global_Object.CoorCanvas(this.TopHeader());
                    Point mdCV = Global_Object.CoorCanvas(this.MiddleHeader());
                    Point bhCV = Global_Object.CoorCanvas(this.BottomHeader());
                    // bool onTouch= FindHeaderIntersectsFullRiskArea(this.TopHeader()) | FindHeaderIntersectsFullRiskArea(this.MiddleHeader()) | FindHeaderIntersectsFullRiskArea(this.BottomHeader());
                    bool onTouch = FindHeaderIntersectsFullRiskAreaCv(thCV) | FindHeaderIntersectsFullRiskAreaCv(mdCV) | FindHeaderIntersectsFullRiskAreaCv(bhCV);

                    if (onTouch)
                    {
                        Console.WriteLine(r.properties.NameId+" => CheckIntersection");
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
                Point rP =Global_Object.CoorCanvas( MiddleHeader());
               // bool onFound = r.FindHeaderIsCloseRiskArea(this.properties.pose.Position);
                bool onFound = r.FindHeaderIsCloseRiskAreaCv(rP);
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

            Point thCV = Global_Object.CoorCanvas(this.TopHeader());
            Point mhCV = Global_Object.CoorCanvas(this.MiddleHeader());
            Point bhCV = Global_Object.CoorCanvas(this.BottomHeader());



            //if (robot.FindHeaderIntersectsRiskAreaHeader(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaHeader(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaHeader(this.BottomHeader()))
                if (robot.FindHeaderIntersectsRiskAreaHeaderCv(thCV) || robot.FindHeaderIntersectsRiskAreaHeaderCv(mhCV) || robot.FindHeaderIntersectsRiskAreaHeaderCv(bhCV))

                {
                    TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_HEADER;
                Console.WriteLine(robot.properties.NameId + " =>"+ TrafficBehaviorStateTracking);
            }
           // else if (robot.FindHeaderIntersectsRiskAreaTail(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaTail(this.MiddleHeader()) || robot.FindHeaderIntersectsRiskAreaTail(this.BottomHeader()))


                else if (robot.FindHeaderIntersectsRiskAreaTailCv(thCV)|| robot.FindHeaderIntersectsRiskAreaTailCv(mhCV)|| robot.FindHeaderIntersectsRiskAreaTailCv(bhCV))
                {
                    TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_TAIL;
                Console.WriteLine(robot.properties.NameId + " =>"+ TrafficBehaviorStateTracking);
            }
             //   else if (robot.FindHeaderIntersectsRiskAreaRightSide(this.TopHeader())|| robot.FindHeaderIntersectsRiskAreaRightSide(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaRightSide(this.BottomHeader()))
                 else if (robot.FindHeaderIntersectsRiskAreaRightSideCv(thCV) || robot.FindHeaderIntersectsRiskAreaRightSideCv(mhCV) || robot.FindHeaderIntersectsRiskAreaRightSideCv(bhCV))

            {
                TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_SIDE;
                Console.WriteLine(robot.properties.NameId + " =>"+ TrafficBehaviorStateTracking);
            }
              //  else if (robot.FindHeaderIntersectsRiskAreaLeftSide(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaLeftSide(this.MiddleHeader()) || robot.FindHeaderIntersectsRiskAreaLeftSide(this.BottomHeader()))
                 else if (robot.FindHeaderIntersectsRiskAreaLeftSideCv(thCV) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(mhCV) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(bhCV))

            {
                TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_SIDE;
                Console.WriteLine(robot.properties.NameId + " =>"+ TrafficBehaviorStateTracking);
            }
          
        }
        public void TrafficBehavior(RobotUnity robot)
        {
            switch(TrafficBehaviorStateTracking)
            {
                case TrafficBehaviorState.HEADER_TOUCH_NOTOUCH:
                    SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                    Console.WriteLine(this.properties.Label + " => NORMAL");
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
                            Console.WriteLine(this.properties.Label + " => STOP");
                        }
                        else
                        {
                            SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                            Console.WriteLine(this.properties.Label + " => NORMAL");
                        }
                    }
                    else if(prioritLevel.IndexOnMainRoad < robot.prioritLevel.IndexOnMainRoad)
                    {
                        SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                        Console.WriteLine(this.properties.Label + " => STOP");
                    }
                    break;
                case TrafficBehaviorState.HEADER_TOUCH_TAIL:
                    SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                    Console.WriteLine(this.properties.Label+ " => STOP");
                    // robot stop
                    break;
                case TrafficBehaviorState.HEADER_TOUCH_SIDE:
                    SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                    Console.WriteLine(this.properties.Label+ " => STOP");
                    break;

            }
        }
        public void TurnOnSupervisorTraffic(bool flagtraffic)
        {
            flagSupervisorTraffic = flagtraffic;
        }
        public  void TrafficUpdate() {
            while (true)
            {
                try
                {
                    prioritLevel.IndexOnMainRoad = trafficManagementService.FindIndexZoneRegister(properties.pose.Position);
                    // cập nhật vùng riskzone // update vùng risk area cho robot
                    RiskZoneRegister rZR = trafficManagementService.FindRiskZone(properties.pose.Position, IndexZoneDefaultFind.ZONE_OP, true);
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
                catch { }
                Thread.Sleep(500);
            }
        
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
                    Console.WriteLine(this.properties.Label+"=> Normal No touch");
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
