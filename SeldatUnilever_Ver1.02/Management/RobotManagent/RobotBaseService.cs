using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeldatMRMS.Management.RobotManagent
{
    public class RobotBaseService:TrafficRobotUnity
    {
        public enum ProcedureControlAssign 
        {
            PRO_NONE = 0,
            PRO_BUFFER_TO_MACHINE ,
            PRO_BUFFER_TO_RETURN,
            PRO_FORKLIFT_TO_BUFFER,
            PRO_MACHINE_TO_RETURN,
            PRO_RETURN_TO_GATE,
            PRO_CHARGE,
            PRO_READY
        }
        public object ProcedureControl;
        public ProcedureControlAssign  PreProcedureAs;
        public ProcedureControlAssign ProcedureAs;
        public bool SelectedATask { get; set; }
       
        public struct LoadedConfigureInformation
        {
            public bool IsLoadedStatus { get; set; }
            public String ErrorContent { get; set; }
        }
    }
}
