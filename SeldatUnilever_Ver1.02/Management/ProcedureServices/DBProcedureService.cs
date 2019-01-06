using SelDatUnilever_Ver1;
using System;
using System.Collections.Generic;
using System.Windows;

namespace SeldatMRMS
{
    public class DBProcedureService:CollectionDataService
    {
        public DBProcedureService() { }
        public struct RobotSensor
        {
            public bool sensor1 { get; set; }
            public bool sensor2 { get; set; }
            public bool sensor3 { get; set; }
        }
        public class RobotTrackingInform
        {
            public String CurrentProcedure;
            public Point location;
            public String status; // Finish/ Error
            public RobotSensor CurrentRobotSensor;
        }
        public struct ProcedureDataItems
        {

            public DateTime StartTaskTime { get; set; }
            public DateTime EndTime { get; set; }
            public String StatusProcedureDelivered { get; set; }
            public String ErrorStatusID { get; set; } // if have
        }
        public struct RobotDataItems
        {
            public String RobotTaskID;
            public RobotSensor RobotSensorStatus;
            public double LevelBatteryStart;
            public double LevelBatteryEnd;
            public List <RobotTrackingInform> RobotTrackingInformList;
        }
    }
}
