using SeldatMRMS.Management.RobotManagent;
using SelDatUnilever_Ver1._00.Management.TrafficManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;

namespace SeldatMRMS.Management.TrafficManager
{
    public class TrafficManagementService:TrafficRounterService
    {
      
        public TrafficManagementService() { }
        public void AddRobotUnityToTraffic(List<RobotUnity> elements)
        {
            RobotUnityListOnTraffic = elements;
            RobotUnityListOnTraffic.ForEach(e => e.PoseHandler += PoseRobotHandler);
        }
        public void ReleaseRobotUnityToTraffic(List<RobotUnity> elements)
        {           
            RobotUnityListOnTraffic.ForEach(e => e.PoseHandler -= PoseRobotHandler);
            RobotUnityListOnTraffic.Clear();
        }
        public void PoseRobotHandler(Pose p, Object obj)
        {
            var robot = obj as RobotUnity;
            robot.PrioritLevelRegister.IndexOnMainRoad = (int)FindIndexZoneRegister(p.Position);
        }
    }
}
