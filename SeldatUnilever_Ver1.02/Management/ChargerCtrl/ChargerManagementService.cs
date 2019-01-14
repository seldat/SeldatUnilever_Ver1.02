using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeldatUnilever_Ver1._02.Management.ChargerCtrl;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;
using static SelDatUnilever_Ver1._00.Management.ChargerCtrl.ChargerCtrl;

namespace SelDatUnilever_Ver1._00.Management.ChargerCtrl
{
    public class ChargerManagementService
    {

        public Dictionary<ChargerId, ChargerCtrl> ChargerStationList;
        public const Int32 AmountofCharger = 3;
        public ListCollectionView Grouped_PropertiesCharge { get; private set; }
        public List<ChargerInfoConfig> PropertiesCharge_List;
        private List<ChargerInfoConfig> CfChargerStationList;
        public ConfigureCharger configureForm;
        public ChargerManagementService()
        {
            //LoadChargerConfigure();
          
            PropertiesCharge_List = new List<ChargerInfoConfig>();
            Grouped_PropertiesCharge = (ListCollectionView)CollectionViewSource.GetDefaultView(PropertiesCharge_List);
            ChargerStationList = new Dictionary<ChargerId, ChargerCtrl>();
            LoadConfigure();

            configureForm = new ConfigureCharger(this, Thread.CurrentThread.CurrentCulture.ToString());
        }

        public void Initialize()
        {
            ChargerInfoConfig pchr1 = new ChargerInfoConfig();
            pchr1.Id = ChargerId.CHARGER_ID_1;
            pchr1.Ip = "192.168.1.2";
            pchr1.Port = 8081;
            pchr1.PointFrontLineStr = "1,2,3";
            pchr1.ParsePointFrontLineValue(pchr1.PointFrontLineStr);
            pchr1.PointOfPallet = "{\"pallet\":2,\"bay\":1,\"hasSubLine\":\"no\",\"direction\":0,\"row\":0}";
            PropertiesCharge_List.Add(pchr1);
            ChargerCtrl chargerStation1 = new ChargerCtrl(pchr1);
            ChargerStationList.Add(chargerStation1.cf.Id, chargerStation1);

            ChargerInfoConfig pchr2 = new ChargerInfoConfig();
            pchr2.Id = ChargerId.CHARGER_ID_2;
            pchr2.Ip = "192.168.1.2";
            pchr2.Port = 8081;
            pchr2.PointFrontLineStr = "1,2,3";
            pchr2.ParsePointFrontLineValue(pchr2.PointFrontLineStr);
            pchr2.PointOfPallet = "{\"pallet\":2,\"bay\":1,\"hasSubLine\":\"no\",\"direction\":0,\"row\":0}";
            PropertiesCharge_List.Add(pchr2);
            ChargerCtrl chargerStation2 = new ChargerCtrl(pchr2);
            ChargerStationList.Add(chargerStation2.cf.Id, chargerStation2);

            ChargerInfoConfig pchr3 = new ChargerInfoConfig();
            pchr3.Id = ChargerId.CHARGER_ID_3;
            pchr3.Ip = "192.168.1.2";
            pchr3.Port = 8081;
            pchr3.PointFrontLineStr = "1,2,3";
            pchr3.ParsePointFrontLineValue(pchr3.PointFrontLineStr);
            pchr3.PointOfPallet = "{\"pallet\":2,\"bay\":1,\"hasSubLine\":\"no\",\"direction\":0,\"row\":0}";
            PropertiesCharge_List.Add(pchr3);
            ChargerCtrl chargerStation3 = new ChargerCtrl(pchr3);
            ChargerStationList.Add(chargerStation3.cf.Id, chargerStation3);

            Grouped_PropertiesCharge.Refresh();
        }
        public void AddCharger()
        {
            PropertiesCharge_List.Add(new ChargerInfoConfig());
            Grouped_PropertiesCharge.Refresh();
        }
        public void SaveConfig(String data)
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigCharge.json");
            System.IO.File.WriteAllText(path, data);
        }
        public bool LoadConfigure()
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigCharge.json");
            if (!File.Exists(path))
            {
                Initialize();
                SaveConfig(JsonConvert.SerializeObject(PropertiesCharge_List, Formatting.Indented).ToString());
                return false;
            }
            else
            {
                try
                {
                    String data = File.ReadAllText(path);
                    if (data.Length > 0)
                    {
                        // List<ChargerInfoConfig> tempPropertiestcharge = JsonConvert.DeserializeObject<List<ChargerInfoConfig>>(data);
                        //tempPropertiestcharge.ForEach(e => PropertiesCharge_List.Add(e));
                        JArray results = JArray.Parse(data);
                        foreach(var result in results)
                        {
                            ChargerInfoConfig pchr = new ChargerInfoConfig();
                            pchr.Id = (ChargerId)((int)result["Id"]);
                            pchr.Ip = (String)result["Ip"];
                            pchr.Port = (int)result["Port"];
                            pchr.PointFrontLineStr = (String)result["PointFrontLineStr"];
                            pchr.ParsePointFrontLineValue(pchr.PointFrontLineStr);
                            pchr.PointOfPallet = (String)result["PointOfPallet"];
                            ChargerCtrl chargerStation = new ChargerCtrl(pchr);
                            ChargerStationList.Add(chargerStation.cf.Id, chargerStation);
                            PropertiesCharge_List.Add(pchr);
                        }
                        Grouped_PropertiesCharge.Refresh();
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }
        public void LoadChargerConfigure()
        {
            string name = "Charger";
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
            ChargerStationList = new Dictionary<ChargerId, ChargerCtrl>();
            foreach (DataRow row in data.Rows)
            {
                ChargerInfoConfig ptemp = new ChargerInfoConfig();
                ptemp.Id = (ChargerId)int.Parse(row.Field<String>("ID"));
                ptemp.Ip = row.Field<String>("IP");
                ptemp.Port = int.Parse(row.Field<String>("Port"));
                ptemp.PointFrontLine = new Pose(double.Parse(row.Field<String>("PointFrontLine").Split(',')[0]),
                                                double.Parse(row.Field<String>("PointFrontLine").Split(',')[1]),
                                                double.Parse(row.Field<String>("PointFrontLine").Split(',')[2]));
                ptemp.PointOfPallet = row.Field<String>("PointOfCharger");
                ChargerCtrl chargerStation = new ChargerCtrl(ptemp);
                ChargerStationList.Add(chargerStation.cf.Id,chargerStation);
            }
            con.Close();
        }
        public void FixedConfigure(ChargerId id, ChargerInfoConfig chcf)
        {
            if(ChargerStationList.ContainsKey(id))
            {
                ChargerStationList[(ChargerId)id].UpdateConfigure(chcf);
            }
        }
    }
}
