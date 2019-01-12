﻿using Newtonsoft.Json;
using SeldatMRMS;
using SeldatMRMS.Management.RobotManagent;
using SeldatUnilever_Ver1._02.Management.TrafficManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SelDatUnilever_Ver1._00.Management.TrafficManager
{
    public class TrafficRounterService
    {
        public ListCollectionView Grouped_PropertiesTrafficZoneList { get; private set; }
        public List<ZoneRegister> PropertiesTrafficZoneList;
        protected List<RobotUnity> RobotUnityListOnTraffic = new List<RobotUnity>();
        public class ZoneRegister : NotifyUIBase
        {
            private String _NameId;
            public String NameId { get => _NameId; set { _NameId = value; RaisePropertyChanged("NameId"); } }
            private String _TypeZone;
            public String TypeZone { get => _TypeZone; set { _TypeZone = value; RaisePropertyChanged("TypeZone"); } }
            private int _Index;
            public int Index { get => _Index; set { _Index = value; RaisePropertyChanged("Index"); } }
            private Point _Point1;
            public Point Point1 { get => _Point1; set { _Point1 = value; RaisePropertyChanged("Point1"); } }
            private Point _Point2;
            public Point Point2 { get => _Point2; set { _Point2 = value; RaisePropertyChanged("Point2"); } }
            private Point _Point3;
            public Point Point3 { get => _Point3; set { _Point3 = value; RaisePropertyChanged("Point3"); } }
            private Point _Point4;
            public Point Point4 { get => _Point4; set { _Point4 = value; RaisePropertyChanged("Point4"); } }
            public String _Detail;
            public String Detail { get => _Detail; set { _Detail = value; RaisePropertyChanged("Detail"); } }
            public Point[] GetZone()
            {
                return new Point[4] { Point1, Point2, Point3, Point4 };
            }
        }
        public Dictionary<String, ZoneRegister> ZoneRegisterList = new Dictionary<string, ZoneRegister>();
        public ConfigureArea configureArea;
        public TrafficRounterService()
        {
            PropertiesTrafficZoneList = new List<ZoneRegister>();
            Grouped_PropertiesTrafficZoneList = (ListCollectionView)CollectionViewSource.GetDefaultView(PropertiesTrafficZoneList);
            LoadConfigure();
            configureArea = new ConfigureArea(this);
        }
        public void Initialize()
        {
            ZoneRegister ptemp = new ZoneRegister();
            ptemp.NameId = "OPA" + ZoneRegisterList.Count;
            ptemp.Point1 = new Point(0, 0);
            ptemp.Point2 = new Point(10, 10);
            ptemp.Point3 = new Point(3, 4);
            ptemp.Point4 = new Point(5, 5);
            PropertiesTrafficZoneList.Add(ptemp);
            Grouped_PropertiesTrafficZoneList.Refresh();
            ZoneRegisterList.Add(ptemp.NameId, ptemp);
        }
        public void AddConfig()
        {
            ZoneRegister ptemp = new ZoneRegister();
            ptemp.NameId = "OPA" + ZoneRegisterList.Count;
            PropertiesTrafficZoneList.Add(ptemp);
            Grouped_PropertiesTrafficZoneList.Refresh();
            ZoneRegisterList.Add(ptemp.NameId, ptemp);
            SaveConfig(JsonConvert.SerializeObject(PropertiesTrafficZoneList, Formatting.Indented).ToString());
        }
        public void SaveConfig(String data)
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigTrafficZone.json");
            System.IO.File.WriteAllText(path, data);
        }
        public bool LoadConfigure()
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigTrafficZone.json");
            if (!File.Exists(path))
            {
                Initialize();
                SaveConfig(JsonConvert.SerializeObject(PropertiesTrafficZoneList, Formatting.Indented).ToString());
                return false;
            }
            else
            {
                try
                {
                    String data = File.ReadAllText(path);
                    if (data.Length > 0)
                    {
                        List<ZoneRegister> tempPropertiestcharge = JsonConvert.DeserializeObject<List<ZoneRegister>>(data);
                        tempPropertiestcharge.ForEach(e => { PropertiesTrafficZoneList.Add(e); ZoneRegisterList.Add(e.NameId, e); });
                        Grouped_PropertiesTrafficZoneList.Refresh();
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }
        public void LoadConfigureZone()
        {
            string name = "Area";
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Configure.xlsx");
            MessageBox.Show(path);
            string constr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                            path +
                            ";Extended Properties='Excel 12.0 XML;HDR=YES;';";
            OleDbConnection con = new OleDbConnection(constr);
            OleDbCommand oconn = new OleDbCommand("Select * From [" + name + "$]", con);
            con.Open();

            OleDbDataAdapter sda = new OleDbDataAdapter(oconn);
            DataTable data = new DataTable();
            sda.Fill(data);
            double x, y;
            foreach (DataRow row in data.Rows)
            {
                ZoneRegister zone = new ZoneRegister();
                zone.NameId = row.Field<string>("Name");
                zone.Index = int.Parse(row.Field<string>("Index"));
                x = double.Parse(row.Field<string>("Point1").Split(',')[0]);
                y = double.Parse(row.Field<string>("Point1").Split(',')[1]);
                zone.Point1 = new Point(x, y);
                x = double.Parse(row.Field<string>("Point2").Split(',')[0]);
                y = double.Parse(row.Field<string>("Point2").Split(',')[1]);
                zone.Point2 = new Point(x, y);
                x = double.Parse(row.Field<string>("Point3").Split(',')[0]);
                y = double.Parse(row.Field<string>("Point3").Split(',')[1]);
                zone.Point3 = new Point(x, y);
                x = double.Parse(row.Field<string>("Point4").Split(',')[0]);
                y = double.Parse(row.Field<string>("Point4").Split(',')[1]);
                zone.Point4 = new Point(x, y);
                zone.Detail = row.Field<string>("Detail_vn");
                ZoneRegisterList.Add(zone.NameId, zone);
            }
            con.Close();
        }
        public int FindIndexZoneRegister(Point p)
        {
            int index = -1;
            foreach (ZoneRegister z in ZoneRegisterList.Values)
            {
                if (ExtensionService.IsInPolygon(z.GetZone(), p))
                {
                    index = z.Index;
                    break;
                }
            }
            return index;
        }
        public int FindAmoutOfRobotUnityinArea(String areaName)
        {
            int amout = 0;
            foreach (RobotUnity r in RobotUnityListOnTraffic)
            {

                if (ExtensionService.IsInPolygon(ZoneRegisterList["areaName"].GetZone(), r.properties.pose.Position))
                {
                    amout++;
                }
            }
            return amout;
        }
        public String DetermineRobotUnityinArea(Point position)
        {
            String zoneName = "";
            foreach (var r in ZoneRegisterList.Values) // xác định khu vực đến
            {

                if (ExtensionService.IsInPolygon(r.GetZone(), position))
                {
                    zoneName = r.NameId;
                    break;
                }
            }
            return zoneName;
        }
        public bool HasRobotUnityinArea(Point goal)
        {
            String zoneName = "";
            bool hasRobot = false;
            foreach (var r in ZoneRegisterList.Values) // xác định khu vực đến
            {

                if (ExtensionService.IsInPolygon(r.GetZone(), goal))
                {
                    zoneName = r.NameId;
                    break;
                }
            }
            foreach (RobotUnity r in RobotUnityListOnTraffic) // xác định robot có trong khu vực
            {

                if (ExtensionService.IsInPolygon(ZoneRegisterList[zoneName].GetZone(), r.properties.pose.Position))
                {
                    hasRobot = true;
                }
            }
            return hasRobot;
        }
        public bool HasRobotUnityinArea(String AreaName)
        {
            bool hasRobot = false;
            foreach (RobotUnity r in RobotUnityListOnTraffic) // xác định robot có trong khu vực
            {

                if (ExtensionService.IsInPolygon(ZoneRegisterList[AreaName].GetZone(), r.properties.pose.Position))
                {
                    hasRobot = true;
                }
            }
            return hasRobot;
        }

        public bool RobotIsInArea(String AreaName, Point position)
        {
            bool ret = false;
            foreach (var r in ZoneRegisterList.Values) // xác định khu vực đến
            {
                if (r.NameId.Equals(AreaName))
                {
                    if (ExtensionService.IsInPolygon(r.GetZone(), position))
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }
        public void FixedPropertiesZoneRegister(String nameID)
        {
       
        }
        public void ClearZoneRegister(String nameID)
        {
            ZoneRegisterList.Remove(nameID);
            int index=PropertiesTrafficZoneList.FindIndex(e => e.NameId.Equals(nameID));
            PropertiesTrafficZoneList.RemoveAt(index);
            Grouped_PropertiesTrafficZoneList.Refresh();
        }
    }
}
