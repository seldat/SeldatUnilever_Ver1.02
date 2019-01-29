using DoorControllerService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeldatUnilever_Ver1._02.Management.DoorServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static DoorControllerService.DoorService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;

namespace SeldatMRMS.Management.DoorServices
{
    public class DoorManagementService
    {
        public const Int32 AmountofDoor = 4;
        public ListCollectionView Grouped_PropertiesDoor { get; private set; }
        public List<DoorInfoConfig> PropertiesDoor_List;
        private List<DoorInfoConfig> DoorInfoConfigList;
        public DoorService DoorMezzamineUpBack;
        public DoorService DoorMezzamineUpFront;
        public DoorService DoorMezzamineReturnBack;
        public DoorService DoorMezzamineReturnFront;
        public DoorElevator DoorElevator;
        public DoorConfigure doorConfigure;
        public DoorManagementService(){
            // LoadDoorConfigure();
            DoorInfoConfigList = new List<DoorInfoConfig>();
            PropertiesDoor_List = new List<DoorInfoConfig>();
            Grouped_PropertiesDoor = (ListCollectionView)CollectionViewSource.GetDefaultView(PropertiesDoor_List);

            LoadConfigure();
            
            DoorMezzamineUpBack = new DoorService(DoorInfoConfigList[0]);
            DoorMezzamineUpFront = new DoorService(DoorInfoConfigList[1]);
            DoorMezzamineReturnBack = new DoorService(DoorInfoConfigList[2]);
            DoorMezzamineReturnFront = new DoorService(DoorInfoConfigList[3]);
            doorConfigure = new DoorConfigure(this);
        }
    
        public void AddDoor()
        {
                PropertiesDoor_List.Add(new DoorInfoConfig());
                Grouped_PropertiesDoor.Refresh();
        }
        public void Initialize()
        {
            DoorInfoConfig doorICF_MUB = new DoorInfoConfig()
            {
                Id = DoorId.DOOR_MEZZAMINE_UP_FRONT,
                Ip = "192.168.1.1",
                Port = 8081,
                infoPallet = "{\"pallet\":0,\"dir_main\":1,\"bay\":0,\"hasSubLine\":\"no\",\"dir_sub\":0,\"row\":0}",
                PointFrontLineStr = "1,2,3",
                PointCheckInGateStr = "0.0,0.0,0.0"
            };
            doorICF_MUB.ParsePointCheckInGateValue(doorICF_MUB.PointCheckInGateStr);
            doorICF_MUB.ParsePointFrontLineValue(doorICF_MUB.PointFrontLineStr);
            PropertiesDoor_List.Add(doorICF_MUB);
            DoorInfoConfigList.Add(doorICF_MUB);

            DoorInfoConfig doorICF_MUF = new DoorInfoConfig()
            {
                Id = DoorId.DOOR_MEZZAMINE_UP_BACK,
                Ip = "192.168.1.2",
                Port = 8081,
                infoPallet = "{\"pallet\":0,\"dir_main\":1,\"bay\":0,\"hasSubLine\":\"no\",\"dir_sub\":0,\"row\":0}",
                PointFrontLineStr = "1,2,3",
                PointCheckInGateStr = "0.0,0.0,0.0"
            };
            doorICF_MUF.ParsePointCheckInGateValue(doorICF_MUF.PointCheckInGateStr);
            doorICF_MUF.ParsePointFrontLineValue(doorICF_MUF.PointFrontLineStr);
            PropertiesDoor_List.Add(doorICF_MUF);
            DoorInfoConfigList.Add(doorICF_MUF);

            DoorInfoConfig doorICF_MRB = new DoorInfoConfig()
            {
                Id = DoorId.DOOR_MEZZAMINE_RETURN_FRONT,
                Ip = "192.168.1.3",
                Port = 8081,
                infoPallet = "{\"pallet\":1,\"dir_main\":1,\"bay\":0,\"hasSubLine\":\"no\",\"dir_sub\":0,\"row\":0}",
                PointFrontLineStr = "1,2,3",
                PointCheckInGateStr = "0.0,0.0,0.0"
            };
            doorICF_MRB.ParsePointCheckInGateValue(doorICF_MRB.PointCheckInGateStr);
            doorICF_MRB.ParsePointFrontLineValue(doorICF_MRB.PointFrontLineStr);
            PropertiesDoor_List.Add(doorICF_MRB);
            DoorInfoConfigList.Add(doorICF_MRB);

            DoorInfoConfig doorICF_MRF = new DoorInfoConfig()
            {
                Id = DoorId.DOOR_MEZZAMINE_RETURN_BACK,
                Ip = "192.168.1.4",
                Port = 8081,
                infoPallet = "{\"pallet\":1,\"dir_main\":1,\"bay\":0,\"hasSubLine\":\"no\",\"dir_sub\":0,\"row\":0}",
                PointFrontLineStr = "1,2,3",
                PointCheckInGateStr = "0.0,0.0,0.0"
            };
            doorICF_MRF.ParsePointCheckInGateValue(doorICF_MRF.PointCheckInGateStr);
            doorICF_MRF.ParsePointFrontLineValue(doorICF_MRF.PointFrontLineStr);
            PropertiesDoor_List.Add(doorICF_MRF);
            DoorInfoConfigList.Add(doorICF_MRF);

            Grouped_PropertiesDoor.Refresh();
        }
        public void SaveConfig(String data)
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigDoor.json");
            System.IO.File.WriteAllText(path,data);
        }
        public bool LoadConfigure()
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigDoor.json");
            if (!File.Exists(path))
            {
                Initialize();
                SaveConfig(JsonConvert.SerializeObject(PropertiesDoor_List, Formatting.Indented).ToString());
                return false;
            }
            else
            {
                try
                {
                    String data = File.ReadAllText(path);
                    if (data.Length > 0)
                    {
                        JArray results = JArray.Parse(data);
                        foreach (var result in results)
                        {
                            DoorInfoConfig doorICF = new DoorInfoConfig()
                            {
                                Id = (DoorId)((int)result["Id"]),
                                Ip = (String)result["Ip"],
                                Port = (int)result["Port"],
                                infoPallet =(String)result["infoPallet"],
                                PointFrontLineStr = (String)result["PointFrontLineStr"],
                                PointCheckInGateStr = (String)result["PointCheckInGateStr"]
                            };
                            doorICF.ParsePointCheckInGateValue(doorICF.PointCheckInGateStr);
                            doorICF.ParsePointFrontLineValue(doorICF.PointFrontLineStr);
                            PropertiesDoor_List.Add(doorICF);
                            DoorInfoConfigList.Add(doorICF);
                        }
                        Grouped_PropertiesDoor.Refresh();
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }
       
        public void LoadDoorConfigure()
        {
            string name = "Door";
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
            
            foreach (DataRow row in data.Rows)
            {
                DoorInfoConfig ptemp = new DoorInfoConfig();
                ptemp.Id = (DoorId)double.Parse(row.Field<String>("ID"));
                ptemp.Ip = row.Field<String>("IP");
                ptemp.Port = int.Parse(row.Field<String>("Port"));
                ptemp.PointCheckInGate = new Pose(double.Parse(row.Field<String>("PointCheckInGate").Split(',')[0]),
                                                double.Parse(row.Field<String>("PointCheckInGate").Split(',')[1]),
                                                double.Parse(row.Field<String>("PointCheckInGate").Split(',')[2]));
                ptemp.PointFrontLine = new Pose(double.Parse(row.Field<String>("PointFrontLine").Split(',')[0]),
                                                double.Parse(row.Field<String>("PointFrontLine").Split(',')[1]),
                                                double.Parse(row.Field<String>("PointFrontLine").Split(',')[2]));
                ptemp.infoPallet = row.Field<String>("InfoPallet").ToString();
 
                DoorInfoConfigList.Add(ptemp);
            }
            con.Close();
        }
        public void ResetAllDoors()
        {

        }
        public void DisposeAllDoors()
        {

        }
        public void FixedConfigure(DoorId id, DoorInfoConfig dcf)
        {
           /* if (ChargerStationList.ContainsKey(id))
            {
                ChargerStationList[(ChargerId)id].UpdateConfigure(chcf);
            }*/
        }
    }
}
