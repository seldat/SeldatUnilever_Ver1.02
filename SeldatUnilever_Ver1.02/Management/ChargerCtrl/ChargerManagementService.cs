using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
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
        public ChargerManagementService()
        {
            LoadChargerConfigure();
         /* CfChargerStationList = new List<ChargerInfoConfig>();
            ChargerStation_1 = new ChargerCtrl(CfChargerStationList[0]);
            ChargerStation_2 = new ChargerCtrl(CfChargerStationList[1]);
            ChargerStation_3 = new ChargerCtrl(CfChargerStationList[2]);
            PropertiesCharge_List = new List<ChargerInfoConfig>();
            Grouped_PropertiesCharge = (ListCollectionView)CollectionViewSource.GetDefaultView(PropertiesCharge_List);
            LoadConfigure();*/
        }
        public void Initialize()
        { 
            PropertiesCharge_List.Add(new ChargerInfoConfig() {
            IdStr = (PropertiesCharge_List.Count + 1),
            Ip = "192.168.1.2",
            Port = 10001,
            PointOfPallet = "{ \"pallet\":\"null\",\"bay\":1,\"hasSubLine\":\"no\",\"direction\":\"null\",\"row\":0}",
            });
            PropertiesCharge_List.Add(new ChargerInfoConfig()
            {
                IdStr = (PropertiesCharge_List.Count + 1),
                Ip = "192.168.1.2",
                Port = 10001,
                PointOfPallet = "{ \"pallet\":\"null\",\"bay\":1,\"hasSubLine\":\"no\",\"direction\":\"null\",\"row\":0}",
            });
            PropertiesCharge_List.Add(new ChargerInfoConfig()
            {
                IdStr = (PropertiesCharge_List.Count + 1),
                Ip = "192.168.1.2",
                Port = 10001,
                PointOfPallet = "{ \"pallet\":\"null\",\"bay\":1,\"hasSubLine\":\"no\",\"direction\":\"null\",\"row\":0}",
            });
            Grouped_PropertiesCharge.Refresh();
        }
        public void AddCharger()
        {
            PropertiesCharge_List.Add(new ChargerInfoConfig());
            Grouped_PropertiesCharge.Refresh();
        }
        public void SaveConfig(DataGrid datagrid)
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigCharge.json");
            System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(datagrid.ItemsSource, Formatting.Indented));
        }
        public bool LoadConfigure()
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigCharge.json");
            if (!File.Exists(path))
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
                        List<ChargerInfoConfig> tempPropertiestcharge = JsonConvert.DeserializeObject<List<ChargerInfoConfig>>(data);
                        tempPropertiestcharge.ForEach(e => PropertiesCharge_List.Add(e));
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
    }
}
