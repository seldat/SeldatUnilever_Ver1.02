using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace SeldatMRMS
{
    public class DBProcedureService:CollectionDataService
    {
        public enum ProcessStatus
        {
            F=0, // failed
            S=1 //Sucess
        }

        public DBProcedureService(RobotUnity robot) {
            


        }
        public void SendHttpProcedureDataItem(ProcedureDataItemsDB procedureDataItemsDB)
        {
            String url= Global_Object.url + "/robot/rest/reportRobot/insertUpdateListRobotProcess";
            List<ProcedureDataItemsDB> listproc = new List<ProcedureDataItemsDB>();
            listproc.Add(procedureDataItemsDB);         
            new BridgeClientRequest().PostCallAPI(url, JsonConvert.SerializeObject(listproc).ToString());
        }
        public void SendHttpRobotTaskItem(RobotTaskDB robotTaskDB)
        {
            String url = Global_Object.url+ "/robot/rest/reportRobot/insertUpdateListRobotTask";
            List<RobotTaskDB> listrot = new List<RobotTaskDB>();
            listrot.Add(robotTaskDB);
            new BridgeClientRequest().PostCallAPI(url, JsonConvert.SerializeObject(listrot));
        }
        public class ProcedureDataItemsDB
        {
            ProcedureCode prcode;
            OrderItem order;
            RobotUnity robot;
            public ProcedureDataItemsDB(ProcedureCode prcode,RobotUnity robot) {
                this.prcode = prcode;
                this.order = order;
                this.robot = robot;
                rpBeginDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                creUsrId = Global_Object.userLogin;
                updUsrId = Global_Object.userLogin;
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
            public int operationType { get; set; }
            public string rpBeginDatetime { get; set; }
            public string rpEndDatetime { get; set; }
            public string orderContent { get; set; }
            public String robotProcessStastus { get; set; }
            public int creUsrId { get; set; }
            public int updUsrId { get; set; }
            public void SetOrderItem(OrderItem order)
            {
                this.order = order;
            }
            public void GetParams(String status)
            {
                planId = order.planId;
                deviceId = order.deviceId;
                productDetailId = order.productDetailID;
                productId = order.productId;
                palletId = order.palletId;
                bufferId = order.bufferId;
                operationType = (int)prcode ;
                orderContent = JsonConvert.SerializeObject(order);
                rpEndDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
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
             /* public String detailInfo;
              public String problemContent;
              public String solvedProblemContent;*/

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
