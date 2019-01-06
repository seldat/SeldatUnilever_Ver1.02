using SeldatMRMS;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using SelDatUnilever_Ver1._00.Management.DeviceManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SelDatUnilever_Ver1._00.Management.UnityService
{
   public class TaskRounterService
    {
        public enum ProcessAssignAnTaskWait
        {
            PROC_ANY_IDLE = 0,
            PROC_ANY_CHECK_HAS_ANTASK,
            PROC_ANY_ASSIGN_ANTASK,
            PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST,
            PROC_ANY_CHECK_ROBOT_BATTERYLEVEL,
            PROC_ANY_SET_TRAFFIC_RISKAREA_ON,
            PROC_ANY_CHECK_ROBOT_OUTSIDEREADY,
        }
        protected enum ProcessAssignTaskReady
        {
            PROC_READY_IDLE = 0,
            PROC_READY_CHECK_HAS_ANTASK,
            PROC_READY_ASSIGN_ANTASK,
            PROC_READY_GET_ANROBOT_INREADYLIST,
            PROC_READY_CHECK_ROBOT_BATTERYLEVEL,
            PROC_READY_SET_TRAFFIC_RISKAREA_ON,
            PROC_READY_CHECK_ROBOT_OUTSIDEREADY,
        }
        protected ProcessAssignTaskReady processAssignTaskReady;
        
        protected ProcessAssignAnTaskWait processAssignAnTaskWait;
        protected ProcedureManagementService procedureService;
        public RobotManagementService robotManageService;
        public TrafficManagementService trafficService;
        public List<DeviceItem> deviceItemsList;
        public bool Alive = false;
        public void RegistryService(RobotManagementService robotManageService)
        {
            this.robotManageService = robotManageService;
        }
        public void RegistryService(TrafficManagementService trafficService)
        {
            this.trafficService = trafficService;
        }
        public void RegistryService(ProcedureManagementService procedureService)
        {
            this.procedureService = procedureService;
        }
        public void RegistryService(List<DeviceItem> deviceItemsList)
        {
            this.deviceItemsList = deviceItemsList;
        }
        public TaskRounterService() {
            //processAssignAnTaskState = ProcessAssignAnTask.PROC_IDLE;
        }
        public void MoveElementToEnd()
        {
            var element = deviceItemsList[0];
            deviceItemsList.RemoveAt(0);
            deviceItemsList.Add(element);
        }
        public OrderItem Gettask()
        {
            OrderItem item = null;
            if (deviceItemsList.Count > 0)
            {
                item = deviceItemsList[0].GetOrder();
            }
            return item;
        }
    }
}
