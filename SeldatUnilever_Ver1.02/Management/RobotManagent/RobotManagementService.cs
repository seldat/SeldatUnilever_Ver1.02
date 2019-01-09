﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SelDatUnilever_Ver1._00.Management.ChargerCtrl.ChargerCtrl;

namespace SeldatMRMS.Management.RobotManagent
{
    public class RobotManagementService
    {
        public const Int32 AmountofRobotUnity = 3;
        public class ResultRobot
        {
            public RobotUnity robot;
            public bool onReristryCharge = false;

        }
        public ListCollectionView Grouped_PropertiesRobotUnity { get; private set; }
        public List<PropertiesRobotUnity> PropertiesRobotUnity_List;
        public Dictionary<String,RobotUnity>  RobotUnityRegistedList = new Dictionary<string, RobotUnity>();
        public Dictionary<String, RobotUnity> RobotUnityWaitTaskList = new Dictionary<string, RobotUnity>();
        public Dictionary<String, RobotUnity> RobotUnityReadyList = new Dictionary<string, RobotUnity>();
        public RobotManagementService(Canvas canvas) {
           LoadRobotUnityConfigure();
            PropertiesRobotUnity_List = new List<PropertiesRobotUnity>();
            Grouped_PropertiesRobotUnity = (ListCollectionView)CollectionViewSource.GetDefaultView(PropertiesRobotUnity_List);
         //   LoadConfigure();
        }
        public void Initialize()
        {
            RobotUnity r1 = new RobotUnity();
            r1.ConnectionStatusHandler += ConnectionStatusHandler;
            PropertiesRobotUnity_List.Add(r1.properties);
            RobotUnityRegistedList.Add(r1.properties.NameID, r1);
            RobotUnity r2 = new RobotUnity();
            r2.ConnectionStatusHandler += ConnectionStatusHandler;
            PropertiesRobotUnity_List.Add(r2.properties);
            RobotUnityRegistedList.Add(r2.properties.NameID, r2);
            RobotUnity r3 = new RobotUnity();
            r3.ConnectionStatusHandler += ConnectionStatusHandler;
            PropertiesRobotUnity_List.Add(new RobotUnity().properties);
            RobotUnityRegistedList.Add(r1.properties.NameID, r1);
            Grouped_PropertiesRobotUnity.Refresh();
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
        public void SaveConfig(DataGrid datagrid)
        {
                String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigRobot.json");
                System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(datagrid.ItemsSource, Formatting.Indented));   
        }
        public bool LoadConfigure()
        {
            String path= Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigRobot.json");
            if(!File.Exists(path))
            {
                File.Create(path);
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
                        tempPropertiestRobotList.ForEach(e => PropertiesRobotUnity_List.Add(e));
                        Grouped_PropertiesRobotUnity.Refresh();
                        return true;
                    }                   
                }
                catch { }
            }
            return false;
        }
        public void LoadRobotUnityConfigure()
        {
            string name = "Robot";
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Configure.xlsx");
            string constr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                            path +
                            ";Extended Properties='Excel 12.0 XML;HDR=YES;';";
            OleDbConnection con = new OleDbConnection(constr);
            OleDbCommand oconn = new OleDbCommand("Select * From [" + name + "$]", con);
            con.Open();

            OleDbDataAdapter sda = new OleDbDataAdapter(oconn);
            DataTable data = new DataTable();
            sda.Fill(data);
           // foreach (DataRow row in data.Rows)
            {
                RobotUnity robot1 = new RobotUnity();
                //robot.Initialize(row);
                robot1.properties.NameID = "1";
                robot1.properties.chargeID = ChargerId.CHARGER_ID_1;
                robot1.Start("ws://192.168.80.131:9090");
                RobotUnityRegistedList.Add(robot1.properties.NameID, robot1);
                robot1.ConnectionStatusHandler += ConnectionStatusHandler;
                AddRobotUnityReadyList(robot1);


                //RobotUnity robot2 = new RobotUnity();
                //robot2.ConnectionStatusHandler += ConnectionStatusHandler;
                //robot.Initialize(row);
                //robot2.properties.NameID = "Robot2";
                //robot2.Start("ws://192.168.80.131:9090");
                //RobotUnityRegistedList.Add(robot2.properties.NameID, robot2);
                //AddRobotUnityReadyList(robot2);

                //RobotUnity robot3 = new RobotUnity();
                //robot.Initialize(row);
                //robot3.properties.NameID = "Robot3";
                //robot3.Start("ws://192.168.80.131:9090");
                //RobotUnityRegistedList.Add(robot3.properties.NameID, robot3);
                //AddRobotUnityReadyList(robot3);
                //robot3.ConnectionStatusHandler += ConnectionStatusHandler;
            }
            con.Close();
        }
        public void AddRobotUnityWaitTaskList(RobotUnity robot)
        {
           RobotUnityWaitTaskList.Add(robot.properties.NameID,robot);
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
        public ResultRobot GetRobotUnityWaitTaskItem0()
        {
            ResultRobot result = null;
            if (RobotUnityWaitTaskList.Count > 0)
            {
                RobotUnity robot = RobotUnityWaitTaskList.ElementAt(0).Value;
                if (robot.getBattery())
                {
                    RemoveRobotUnityWaitTaskList(robot.properties.NameID);
                }
                result = new ResultRobot() { robot = robot, onReristryCharge = robot.getBattery() };
            }
            return result;
        }
        public void AddRobotUnityReadyList(RobotUnity robot)
        {
            RobotUnityReadyList.Add(robot.properties.NameID,robot);
        }
        
        public ResultRobot GetRobotUnityReadyItem0()
        {
            ResultRobot result = null;
            if (RobotUnityReadyList.Count > 0)
            {
                RobotUnity robot = RobotUnityReadyList.ElementAt(0).Value;
                if(robot.getBattery())
                {
                    RemoveRobotUnityReadyList(robot.properties.NameID);
                }
                result = new ResultRobot() {robot=robot, onReristryCharge=robot.getBattery()};
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
    }
}
