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
    public class TrafficRobotUnity : RobotUnityService
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
            HEADER_TOUCH_NOTOUCH,
            MODE_FREE,
            SLOW_DOWN,
            NORMAL_SPEED
        }
        // public enum MvDirection{
        //     INCREASE_X = 0,
        // 	INCREASE_Y,
        // 	DECREASE_X,
        // 	DECREASE_Y
        // // }
        public enum BrDirection
        {
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
            PISTON_PALLET_UP = 0,
            PISTON_PALLET_DOWN
        }
        public class JInfoPallet
        {
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
        public bool onFlagSupervisorTraffic;
        public bool onFlagSelfTraffic;
        private Dictionary<String, RobotUnity> RobotUnityRiskList = new Dictionary<string, RobotUnity>();
        private TrafficBehaviorState TrafficBehaviorStateTracking;
        private TrafficManagementService trafficManagementService;
        private RobotUnity robotModeFree;
        private const double DistanceToSetSlowDown = 80; // sau khi dừng robot phai doi khoan cach len duoc tren 8m thi robot bat dau hoat dong lai bình thuong 8m
        private const double DistanceToSetNormalSpeed = 12; // sau khi dừng robot phai doi khoan cach len duoc tren 8m thi robot bat dau hoat dong lai bình thuong 12m
        public TrafficRobotUnity() : base()
        {
            TurnOnSupervisorTraffic(false);
            TurnOnCtrlSelfTraffic(true);
            RobotUnitylist = new List<RobotUnity>();
            prioritLevel = new PriorityLevel();


        }
        public void StartTraffic()
        {
            new Thread(TrafficUpdate).Start();
        }
        public PriorityLevel prioritLevel;
        public void RegisteRobotInAvailable(Dictionary<String, RobotUnity> RobotUnitylistdc)
        {
            foreach (var r in RobotUnitylistdc.Values)
            {
                if (!r.properties.NameId.Equals(this.properties.NameId))
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
            if (RobotUnityRiskList.Count > 0)
            {
                foreach (RobotUnity r in RobotUnityRiskList.Values)
                {
                    Point thCV = TopHeaderCv();
                    Point mdCV = MiddleHeaderCv();
                    Point bhCV = BottomHeaderCv();
                    // bool onTouch= FindHeaderIntersectsFullRiskArea(this.TopHeader()) | FindHeaderIntersectsFullRiskArea(this.MiddleHeader()) | FindHeaderIntersectsFullRiskArea(this.BottomHeader());
                    // bool onTouch = r.FindHeaderIntersectsFullRiskAreaCv(thCV) | r.FindHeaderIntersectsFullRiskAreaCv(mdCV) | r.FindHeaderIntersectsFullRiskAreaCv(bhCV);

                    bool onTouch = r.FindHeaderIntersectsFullRiskAreaCv(mdCV);

                    if (onTouch)
                    {
                        //  robotLogOut.ShowTextTraffic(r.properties.Label+" => CheckIntersection");
                        robot = r;
                        break;
                    }
                }
            }
            return robot;
        }
        public int CheckSafeDistance() // KIểm tra khoản cách an toàn/ nếu đang trong vùng close với robot khác thì giảm tốc độ, chuyển sang chế độ dò risk area
        {
            int iscloseDistance = 0;
            foreach (RobotUnity r in RobotUnitylist)
            {
                if (r.onFlagSupervisorTraffic)
                {
                    Point rP = MiddleHeaderCv();
                    // bool onFound = r.FindHeaderIsCloseRiskArea(this.properties.pose.Position);
                    bool onFound = r.FindHeaderIsCloseRiskAreaCv(rP);

                    if (onFound)
                    {
                        // if robot in list is near but add in risk list robot
                   //     robotLogOut.ShowTextTraffic(r.properties.Label + "- Intersection");
                       
                        if (!RobotUnityRiskList.ContainsKey(r.properties.NameId))
                        {
                            RobotUnityRiskList.Add(r.properties.NameId, r);
                        }
                        // reduce speed robot control
                        iscloseDistance = 2;
                    }
                    else
                    {
                        // if robot in list is far but before registe in list, must remove in list
                        RemoveRiskList(r.properties.NameId);
                        double rd = ExtensionService.CalDistance(Global_Object.CoorCanvas(this.properties.pose.Position), Global_Object.CoorCanvas(r.properties.pose.Position));
                        if (rd < DistanceToSetSlowDown && rd > 60)
                            iscloseDistance = 1;
                        else
                            iscloseDistance = 0;

                    }
                }
            }
            return iscloseDistance;
        }
        public void RemoveRiskList(String NameID)
        {
            if (RobotUnityRiskList.ContainsKey(NameID))
            {
                RobotUnityRiskList.Remove(NameID);
            }
        }
        public void DetectTouchedPosition(RobotUnity robot) // determine traffic state
        {

            Point thCV = Global_Object.CoorCanvas(TopHeader());
            Point mhCV = Global_Object.CoorCanvas(MiddleHeader());
            Point mhCV1 = MiddleHeaderCv1();
            Point mhCV2 = MiddleHeaderCv2();
            Point mhCV3 = MiddleHeaderCv3();
            Point bhCV = Global_Object.CoorCanvas(BottomHeader());
            //if (robot.FindHeaderIntersectsRiskAreaHeader(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaHeader(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaHeader(this.BottomHeader()))
          /*  if (robot.FindHeaderIntersectsRiskAreaHeaderCv(thCV) || robot.FindHeaderIntersectsRiskAreaHeaderCv(mhCV) || robot.FindHeaderIntersectsRiskAreaHeaderCv(bhCV)
              )
            {
                TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_HEADER;
                robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
            }
            // else if (robot.FindHeaderIntersectsRiskAreaTail(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaTail(this.MiddleHeader()) || robot.FindHeaderIntersectsRiskAreaTail(this.BottomHeader()))


            else if (robot.FindHeaderIntersectsRiskAreaTailCv(thCV) || robot.FindHeaderIntersectsRiskAreaTailCv(mhCV) || robot.FindHeaderIntersectsRiskAreaTailCv(bhCV) )
            {
                TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_TAIL;
                robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
            }
            //   else if (robot.FindHeaderIntersectsRiskAreaRightSide(this.TopHeader())|| robot.FindHeaderIntersectsRiskAreaRightSide(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaRightSide(this.BottomHeader()))
            else if (robot.FindHeaderIntersectsRiskAreaRightSideCv(thCV) || robot.FindHeaderIntersectsRiskAreaRightSideCv(mhCV) || robot.FindHeaderIntersectsRiskAreaRightSideCv(bhCV) 
      
                )

            {
                TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_SIDE;
                robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
            }
            //  else if (robot.FindHeaderIntersectsRiskAreaLeftSide(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaLeftSide(this.MiddleHeader()) || robot.FindHeaderIntersectsRiskAreaLeftSide(this.BottomHeader()))
            else if (robot.FindHeaderIntersectsRiskAreaLeftSideCv(thCV) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(mhCV) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(bhCV) 
                               )
            {
                TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_SIDE;
                robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
            }
            */
            
            //if (robot.FindHeaderIntersectsRiskAreaHeader(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaHeader(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaHeader(this.BottomHeader()))
            if (robot.FindHeaderIntersectsRiskAreaHeaderCv(thCV) || robot.FindHeaderIntersectsRiskAreaHeaderCv(mhCV) || robot.FindHeaderIntersectsRiskAreaHeaderCv(bhCV)
               || robot.FindHeaderIntersectsRiskAreaHeaderCv(mhCV1) || robot.FindHeaderIntersectsRiskAreaHeaderCv(mhCV2) || robot.FindHeaderIntersectsRiskAreaHeaderCv(mhCV3))
            {
                TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_HEADER;
            //  robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
            }
            // else if (robot.FindHeaderIntersectsRiskAreaTail(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaTail(this.MiddleHeader()) || robot.FindHeaderIntersectsRiskAreaTail(this.BottomHeader()))


            else if (robot.FindHeaderIntersectsRiskAreaTailCv(thCV) || robot.FindHeaderIntersectsRiskAreaTailCv(mhCV) || robot.FindHeaderIntersectsRiskAreaTailCv(bhCV)||
                robot.FindHeaderIntersectsRiskAreaTailCv(mhCV1) || robot.FindHeaderIntersectsRiskAreaTailCv(mhCV2)|| robot.FindHeaderIntersectsRiskAreaTailCv(mhCV3))
            {
                TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_TAIL;
             // robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
            }
            //   else if (robot.FindHeaderIntersectsRiskAreaRightSide(this.TopHeader())|| robot.FindHeaderIntersectsRiskAreaRightSide(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaRightSide(this.BottomHeader()))
           /* else if (robot.FindHeaderIntersectsRiskAreaRightSideCv(thCV) || robot.FindHeaderIntersectsRiskAreaRightSideCv(mhCV) || robot.FindHeaderIntersectsRiskAreaRightSideCv(bhCV) ||
                robot.FindHeaderIntersectsRiskAreaRightSideCv(mhCV1) || robot.FindHeaderIntersectsRiskAreaRightSideCv(mhCV2) || robot.FindHeaderIntersectsRiskAreaRightSideCv(mhCV3)
                )

            {
                TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_SIDE;
            //  robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
            }
            //  else if (robot.FindHeaderIntersectsRiskAreaLeftSide(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaLeftSide(this.MiddleHeader()) || robot.FindHeaderIntersectsRiskAreaLeftSide(this.BottomHeader()))
            else if (robot.FindHeaderIntersectsRiskAreaLeftSideCv(thCV) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(mhCV) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(bhCV) ||
                robot.FindHeaderIntersectsRiskAreaLeftSideCv(mhCV1) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(mhCV2) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(mhCV3)
                )
            {
                TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_SIDE;
             //robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
            }*/
       

        }
        public void TrafficBehavior(RobotUnity robot)
        {
            switch (TrafficBehaviorStateTracking)
            {
                case TrafficBehaviorState.HEADER_TOUCH_NOTOUCH:
                    SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                  // robotLogOut.ShowTextTraffic(this.properties.Label + " => NORMAL");
                    // robot speed normal;
                    break;
                case TrafficBehaviorState.HEADER_TOUCH_HEADER:
                    // Find condition priority
                    // index level of road
                    // procedure Flag is set

                    if (prioritLevel.OnAuthorizedPriorityProcedure)
                    {
                        SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                        // SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                        //TrafficBehaviorStateTracking = TrafficBehaviorState.MODE_FREE;
                        // robotModeFree = robot;
                        //  robotLogOut.ShowTextTraffic(this.properties.Label + " => STOP");
                    }
                    else
                    {

                        if (prioritLevel.IndexOnMainRoad < robot.prioritLevel.IndexOnMainRoad)
                        {
                            SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                            //  robotLogOut.ShowTextTraffic(this.properties.Label + " => STOP");
                        }
                        else
                        {
                            SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                           // SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                            // robotLogOut.ShowTextTraffic(this.properties.Label + " => STOP");
                        }
                    }

                    break;
                case TrafficBehaviorState.HEADER_TOUCH_TAIL:
                    SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                    //TrafficBehaviorStateTracking = TrafficBehaviorState.MODE_FREE;
                    //robotModeFree = robot;
                 // robotLogOut.ShowTextTraffic(this.properties.Label+ " => STOP");
                    // robot stop
                    break;
                case TrafficBehaviorState.HEADER_TOUCH_SIDE:
                    SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                    //TrafficBehaviorStateTracking = TrafficBehaviorState.MODE_FREE;
                   // robotModeFree = robot;
              //   robotLogOut.ShowTextTraffic(this.properties.Label+ " => STOP");
                    break;

            }
        }
        public void TurnOnSupervisorTraffic(bool onflagtraffic)
        {
            onFlagSupervisorTraffic = onflagtraffic;
            if(!onflagtraffic)
            { 
                UpdateRiskAraParams(0, 0, 0, 0);
            }
        }
        public void TurnOnCtrlSelfTraffic(bool _onflagSelftraffic)
        {
            this.onFlagSelfTraffic = _onflagSelftraffic;
        }
        public void SetTrafficAtCheckIn(bool onset) // khi robot tai check in
        {
            if (onset)
            {
                onFlagSupervisorTraffic = false;
                UpdateRiskAraParams(0, DfL2, DfWS, DfDistanceInter);
            }
            else
            {
                onFlagSupervisorTraffic = true;
                UpdateRiskAraParams(DfL1, DfL2, DfWS, DfDistanceInter);
            }

        }
        public void TrafficUpdate()
        {
            while (true)
            {
                try
                {
                    prioritLevel.IndexOnMainRoad = trafficManagementService.FindIndexZoneRegister(properties.pose.Position);
                    if (onFlagSupervisorTraffic)
                    {
                       
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
                        SupervisorTraffic();
                    }
                    Draw();

                    // giám sát an toàn
                  
                }
                catch { Console.WriteLine("TrafficRobotUnity Error in TrafficUpdate"); }
                Thread.Sleep(200);
            }

        }
        protected override void SupervisorTraffic()
        {
            if (onFlagSelfTraffic)
            {
                int numMode = CheckSafeDistance();
                if (numMode == 0)
                {
                    if (RobotUnityRiskList.Count > 0)
                    {
                        RobotUnityRiskList.Clear();
                    }
                    TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_NOTOUCH;
                    TrafficBehavior(null);
                }
                else if(numMode==1)
                {
                    SetSpeed(RobotSpeedLevel.ROBOT_SPEED_SLOW);
                   // robotLogOut.ShowTextTraffic("Slow Motion");
                }
                else if (numMode==2)
                {
                    RobotUnity robot = CheckIntersection();
                    if (robot != null)
                    {
                        DetectTouchedPosition(robot);
                        TrafficBehavior(robot);
                    }
                }

            }
            else
            {
                SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
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
