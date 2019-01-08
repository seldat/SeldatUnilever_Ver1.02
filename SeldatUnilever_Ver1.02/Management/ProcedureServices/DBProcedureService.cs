using Newtonsoft.Json;
using SeldatMRMS;
using SeldatMRMS.Management.RobotManagent;
using SelDatUnilever_Ver1;
using SelDatUnilever_Ver1._00.Communication.HttpBridge;
using SelDatUnilever_Ver1._00.Management.ChargerCtrl;
using System;
using System.Collections.Generic;
using System.Windows;
using static SeldatMRMS.ProcedureControlServices;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SelDatUnilever_Ver1
{
    public class DBProcedureService:CollectionDataService
    {
        public ProcedureDataItemsDB procedureDataItemsDB;
        public GateTaskDB gateTaskDB;
        public RobotTaskDB robotTaskDB;
        public ChargerProcedureDB chargerProcedureDB;
        public DBProcedureService(RobotUnity robot) {

        }
        public void SendHttpProcedureDataItem(ProcedureDataItemsDB procedureDataItemsDB)
        {
            String url= Global_Object.url +"/ robot/rest/reportRobot/getListRobotProcess";
            new BridgeClientRequest().PostCallAPI(url,JsonConvert.SerializeObject(procedureDataItemsDB));
        }
        public void SendHttpRobotTaskItem(RobotTaskDB robotTaskDB)
        {
            String url = Global_Object.url+"/robot/rest/reportRobot/insertUpdateListRobotTask";
            new BridgeClientRequest().PostCallAPI(url, JsonConvert.SerializeObject(robotTaskDB));
        }
        public class ProcedureDataItemsDB
        {
            ProcedureCode prcode;
            OrderItem order;
            RobotUnity robot;
            public ProcedureDataItemsDB(ProcedureCode prcode,OrderItem order,RobotUnity robot) {
                this.prcode = prcode;
                this.order = order;
                this.robot = robot;
                rpBeginDatetime = DateTime.Now.ToString("yy//MM/dd hh:mm tt");
                creUsrId = Global_Object.userLogin;
                updUsrId = Global_Object.userLogin;
                robotProcessId = robot.properties.NameID;
                robotTaskId = robot.properties.NameID;
            }
            public String robotProcessId { get; set; }
            public string robotTaskId { get; set; }
            public int gateKey { get; set; }
            public int planId { get; set; }
            public int deviceId { get; set; }
            public int productId { get; set; }
            public int productDetailId { get; set; }
            public int bufferId { get; set; }
            public int palletId { get; set; }
            public string operationType { get; set; }
            public string rpBeginDatetime { get; set; }
            public string rpEndDatetime { get; set; }
            public string orderContent { get; set; }
            public int robotProcessStastus { get; set; }
            public int creUsrId { get; set; }
            public int updUsrId { get; set; }
            public void GetParams(int status)
            {
                planId = order.planId;
                deviceId = order.deviceId;
                productDetailId = order.productDetailID;
                productId = order.productId;
                palletId = order.palletId;
                bufferId = order.bufferId;
                operationType = prcode.ToString() ;
                orderContent = JsonConvert.SerializeObject(order);
                rpEndDatetime = DateTime.Now.ToString("yy//MM/dd hh:mm tt");
                robotProcessStastus = status;
            }
        }
        public struct GateTaskDB
        {
            public int gateKey;
            public string gateTaskStastus;
            public string gtProcedureContent;
            public int creUsrId;
            public int updUsrId;
        }
        public class RobotTaskDB
        {
              RobotUnity robot;
              public RobotTaskDB(RobotUnity robot) {
                robotTaskId = robot.properties.NameID;
                creUsrId = Global_Object.userLogin;
                updUsrId = Global_Object.userLogin;
            }
              public String robotTaskId { get; set; }
              public String robotId { get; set; }
              public String procedureContent{ get; set; }
              public int creUsrId { get; set; }
              public int updUsrId { get ; set; }
              
        }
        public class ProcedureDataItems
        {
            public DateTime StartTaskTime { get; set; }
            public DateTime EndTime { get; set; }
            public String StatusProcedureDelivered { get; set; }
            public String ErrorStatusID { get; set; } // if have
        }

        public class ChargerProcedureDB
        {
            ChargerCtrl chargerCtrl;
            public ChargerProcedureDB(ChargerCtrl chargerCtrl)
            {
                this.chargerCtrl=chargerCtrl;
                rcBeginDatetime = DateTime.Now.ToString("yy//MM/dd hh:mm tt");
            }
            public int robotChargeId;
            public string robotTaskId;
            public int chargeId;
            public int timeWorkId;
            public String rcBeginDatetime;// ": "2018-12-29 13:23:05" //Không có hoặc chuôi rỗng lây ngày hệ thống
            public String rcEndDatetime;//":  "2018-12-29 13:23:05" //Không có hoặc chuôi rỗng lây ngày hệ thống 
            public double currentBattery;
            public int robotChargeStatus;
            public void GetParams(int status)
            {
                rcEndDatetime = DateTime.Now.ToString("yy//MM/dd hh:mm tt");
                robotChargeStatus=status;
            }
        }
    }
}
