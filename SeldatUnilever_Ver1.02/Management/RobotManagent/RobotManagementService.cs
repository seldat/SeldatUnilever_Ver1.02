using Newtonsoft.Json;
using SeldatMRMS.Management.TrafficManager;
using SeldatUnilever_Ver1._02.Management.RobotManagent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SelDatUnilever_Ver1._00.Management.ChargerCtrl.ChargerCtrl;

namespace SeldatMRMS.Management.RobotManagent
{
    public class RobotManagementService
    {
        public const Int32 AmountofRobotUnity = 3;
        public class ResultRobotReady
        {
            public RobotUnity robot;
            public bool onReristryCharge = false;

        }
        public ListCollectionView Grouped_PropertiesRobotUnity { get; private set; }
        public List<PropertiesRobotUnity> PropertiesRobotUnity_List;
        public Dictionary<String,RobotUnity>  RobotUnityRegistedList = new Dictionary<string, RobotUnity>();
        public Dictionary<String, RobotUnity> RobotUnityWaitTaskList = new Dictionary<string, RobotUnity>();
        public Dictionary<String, RobotUnity> RobotUnityReadyList = new Dictionary<string, RobotUnity>();
        public ConfigureRobotUnity configureForm;
        private TrafficManagementService trafficManagementService;
        private Canvas canvas;
        public RobotManagementService(Canvas canvas) {
            this.canvas = canvas;
            PropertiesRobotUnity_List = new List<PropertiesRobotUnity>();
            Grouped_PropertiesRobotUnity = (ListCollectionView)CollectionViewSource.GetDefaultView(PropertiesRobotUnity_List);
            configureForm = new ConfigureRobotUnity(this, Thread.CurrentThread.CurrentCulture.ToString());
            LoadConfigure();
        }
        public void Initialize()
        {
            PropertiesRobotUnity prop1 = new PropertiesRobotUnity();
            prop1.NameId = "RSD"+ RobotUnityRegistedList.Count;
            prop1.L1 = 40;
            prop1.L2 = 40;
            prop1.WS = 60;
            prop1.Label = "Robot1";
            prop1.BatteryLevelRb = 40;
            prop1.Url = "ws://192.168.80.132:9090";
            prop1.ipMcuCtrl = "192.168.1.210";
            prop1.portMcuCtrl = 8081;
            prop1.DistInter = 40;
            prop1.BatteryLowLevel = 20;
            prop1.RequestChargeBattery = false;
            prop1.Width = 1.8;
            prop1.Height = 2.5;
            prop1.Length = 2.2;
            prop1.ChargeID= ChargerId.CHARGER_ID_1;
            prop1.Scale = 10;
            RobotUnity r1 = new RobotUnity();
            r1.Initialize(this.canvas);
            r1.UpdateProperties(prop1);
            r1.ConnectionStatusHandler += ConnectionStatusHandler;
            PropertiesRobotUnity_List.Add (r1.properties);
            RobotUnityRegistedList.Add (r1.properties.NameId, r1);
            r1.Registry (trafficManagementService);
            r1.Start (prop1.Url);
            // đăng ký robot list to many robot quan trong
            AddRobotUnityReadyList (r1);
            r1.RegisteRobotInAvailable (RobotUnityRegistedList);
#if false
            PropertiesRobotUnity prop2 = new PropertiesRobotUnity ();
            prop2.NameId = "RSD" + RobotUnityRegistedList.Count;
            prop2.L1 = 40;
            prop2.L2 = 40;
            prop2.WS = 60;
            prop2.Label = "Robot2";
            prop2.BatteryLevelRb = 40;
            prop2.Url = "ws://192.168.1.200:9090";
            prop2.ipMcuCtrl = "192.168.1.211";
            prop2.portMcuCtrl = 8081;
            prop2.DistInter = 40;
            prop2.BatteryLowLevel = 20;
            prop2.RequestChargeBattery = false;
            prop2.Width = 1.8;
            prop2.Height = 2.5;
            prop2.Length = 2.2;
            prop2.ChargeID = ChargerId.CHARGER_ID_2;
            prop2.Scale = 10;
            RobotUnity r2 = new RobotUnity();
            r2.Initialize(this.canvas);
            r2.UpdateProperties(prop2);
            r2.ConnectionStatusHandler += ConnectionStatusHandler;
            PropertiesRobotUnity_List.Add(r2.properties);
            RobotUnityRegistedList.Add(r2.properties.NameId, r2);
            r2.Registry(trafficManagementService);
            r2.Start (prop2.Url);
            AddRobotUnityReadyList (r2);
            r2.RegisteRobotInAvailable (RobotUnityRegistedList);

            PropertiesRobotUnity prop3 = new PropertiesRobotUnity();
            prop3.NameId = "RSD" + RobotUnityRegistedList.Count;
            prop3.L1 = 40;
            prop3.L2 = 40;
            prop3.WS = 60;
            prop3.Label = "Robot3";
            prop3.BatteryLevelRb = 40;
            prop3.Url = "ws://192.168.1.200:9090";
            prop3.ipMcuCtrl = "192.168.1.212";
            prop3.portMcuCtrl = 8081;
            prop3.DistInter = 40;
            prop3.BatteryLowLevel = 20;
            prop3.RequestChargeBattery = false;
            prop3.Width = 1.8;
            prop3.Height = 2.5;
            prop3.Length = 2.2;
            prop3.ChargeID = ChargerId.CHARGER_ID_3;
            prop3.Scale = 10;

            RobotUnity r3 = new RobotUnity();
            r3.Initialize(this.canvas);
            r3.UpdateProperties(prop3);
            r3.ConnectionStatusHandler += ConnectionStatusHandler;
            PropertiesRobotUnity_List.Add(r2.properties);
            RobotUnityRegistedList.Add(r3.properties.NameId, r3);
            r3.Registry(trafficManagementService);
            r3.Start (prop3.Url);
            AddRobotUnityReadyList (r3);
            r3.RegisteRobotInAvailable (RobotUnityRegistedList);
            Grouped_PropertiesRobotUnity.Refresh ();
#endif
        }
        public void Registry(TrafficManagementService trafficManagementService)
        {
            this.trafficManagementService = trafficManagementService;
        }
        public void ConnectionStatusHandler(Object obj, RosSocket.ConnectionStatus status)
        {
            RobotUnity robot = obj as RobotUnity;
            if (status==RosSocket.ConnectionStatus.CON_OK)
            {
              //  RobotUnityReadyList.Add(robot.properties.NameID,robot);
            }
        }
        public void AddRobotUnity()
        {
                PropertiesRobotUnity_List.Add(new RobotUnity().properties);
                Grouped_PropertiesRobotUnity.Refresh();
        }
        public void SaveConfig(String data)
        {
                String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigRobot.json");
                System.IO.File.WriteAllText(path, data);   
        }
        public bool LoadConfigure()
        {
            String path= Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigRobot.json");
            if(!File.Exists(path))
            {
                Initialize();
                SaveConfig(JsonConvert.SerializeObject(PropertiesRobotUnity_List, Formatting.Indented).ToString());
                return false;
            }
            else
            {
                try
                {
                    String data = File.ReadAllText(path);
                    if (data.Length > 0)
                    {
                        List<PropertiesRobotUnity> tempPropertiestRobotList = JsonConvert.DeserializeObject<List<PropertiesRobotUnity>>(data);
                        foreach(var e in tempPropertiestRobotList)
                        {
                            PropertiesRobotUnity_List.Add(e);
                            RobotUnity robot = new RobotUnity();
                            robot.Initialize(this.canvas);
                            robot.UpdateProperties(e);
                            robot.Registry(trafficManagementService);
                            RobotUnityRegistedList.Add(e.NameId,robot);
                            robot.Start(robot.properties.Url);
                            AddRobotUnityReadyList(robot);
                            robot.RegisteRobotInAvailable(RobotUnityRegistedList);
                        }
                        Grouped_PropertiesRobotUnity.Refresh();
                        return true;
                    }                   
                }
                catch { }
            }
            return false;
        }
        public void AddRobotUnityWaitTaskList(RobotUnity robot)
        {
           RobotUnityWaitTaskList.Add(robot.properties.NameId,robot);
        }
        public void RemoveRobotUnityWaitTaskList(String NameID)
        {
            RobotUnityWaitTaskList.Remove(NameID);
        }
        public void DestroyAllRobotUnity()
        {
            foreach (var item in RobotUnityRegistedList.Values)
            {
                item.Dispose();
            }
            RobotUnityRegistedList.Clear();
        }
        public void DestroyRobotUnity(String NameID)
        {
            if(RobotUnityRegistedList.ContainsKey(NameID))
            {
                RobotUnity robot = RobotUnityRegistedList[NameID];
                robot.Dispose();
                RobotUnityRegistedList.Remove(NameID);
            }
   
        }
        public ResultRobotReady GetRobotUnityWaitTaskItem0()
        {
            ResultRobotReady result = null;
            if (RobotUnityWaitTaskList.Count > 0)
            {
                RobotUnity robot = RobotUnityWaitTaskList.ElementAt(0).Value;
                if (robot.getBattery())
                {
                    RemoveRobotUnityWaitTaskList(robot.properties.NameId);
                }
                result = new ResultRobotReady() { robot = robot, onReristryCharge = robot.getBattery() };
            }
            return result;
        }
        public void AddRobotUnityReadyList(RobotUnity robot)
        {
            RobotUnityReadyList.Add(robot.properties.NameId,robot);
        }
        
        public ResultRobotReady GetRobotUnityReadyItem0()
        {
            ResultRobotReady result = null;
            if (RobotUnityReadyList.Count > 0)
            {
                RobotUnity robot = RobotUnityReadyList.ElementAt(0).Value;
                if(robot.getBattery())
                {
                    RemoveRobotUnityReadyList(robot.properties.NameId);
                }
                result = new ResultRobotReady() {robot=robot, onReristryCharge=robot.getBattery()};
            }
            return result;
        }
        public void RemoveRobotUnityReadyList(String nameID)
        {
            RobotUnityReadyList.Remove(nameID);
        }
        public void StopAt(String nameID)
        {
            if (RobotUnityRegistedList.ContainsKey(nameID))
                RobotUnityRegistedList[nameID].SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
        }
        public void RunAt(String nameID)
        {
            if (RobotUnityRegistedList.ContainsKey(nameID))
                RobotUnityRegistedList[nameID].SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
        }
        public void Stop()
        {
            foreach (RobotUnity r in RobotUnityReadyList.Values)
                r.SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
        }
        public void Run()
        {
            foreach (RobotUnity r in RobotUnityReadyList.Values)
                r.SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
        }
        public void RemoveRobotUnityRegistedList(String nameID)
        {
            if (RobotUnityRegistedList.ContainsKey(nameID))
                RobotUnityRegistedList.Remove(nameID);
        }
        public void FixedPropertiesRobotUnity(String nameID,PropertiesRobotUnity properties)
        {
            DialogResult result = System.Windows.Forms.MessageBox.Show("Bạn chắc chắn Robot đang nằm ở vùng Charge/Ready?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                RobotUnity Rd = RobotUnityRegistedList[nameID];
                Rd.RemoveDraw();
                Rd.Dispose();
                RemoveRobotUnityRegistedList(nameID);
                RemoveRobotUnityWaitTaskList(nameID);
                RemoveRobotUnityReadyList(nameID);
                Rd = null;
                RobotUnity rn = new RobotUnity();
                // cài đặt canvas
                rn.Initialize(this.canvas);
                rn.UpdateProperties(properties);

                // update properties

                // connect ros
                rn.Start(properties.Url);
                // đăng ký giao thông
                rn.Registry(trafficManagementService);
                RobotUnityRegistedList.Add(nameID, rn);
                RobotUnityReadyList.Add(nameID, rn);
                // dăng ký robot list
                rn.RegisteRobotInAvailable(RobotUnityRegistedList);
            }
            else if (result == DialogResult.No)
            {
                //...
            }

        }
    }
}
