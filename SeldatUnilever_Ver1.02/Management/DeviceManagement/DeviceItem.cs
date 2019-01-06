using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;
using static SelDatUnilever_Ver1.CollectionDataService;

namespace SelDatUnilever_Ver1._00.Management.DeviceManagement
{
    public class DeviceItem
    {
        public enum PalletCtrl
        {
            Pallet_CTRL_DOWN =0,
            Pallet_CTRL_UP = 1

        }
        public class DataPallet
        {
            public int row;
            public int bay;
            public int direct;
            public PalletCtrl palletCtrl;
            public Pose linePos;
        }
        public enum TyeRequest
        {
            TYPEREQUEST_FORLIFT_TO_BUFFER=1,
            TYPEREQUEST_BUFFER_TO_MACHINE=2,
            TYPEREQUEST_BUFFER_TO_RETURN=3,
            TYPEREQUEST_MACHINE_TO_RETURN=4,
            TYPEREQUEST_RETURN_TO_GATE = 5,
        }
        public enum TabletConTrol
        {
            TABLET_MACHINE = 10000,
            TABLET_FORKLIFT = 10001
        }
        public enum CommandRequest
        {
            CMD_DATA_ORDER_BUFFERTOMACHINE = 100,
            CMD_DATA_ORDER_RETURN = 100,
            CMD_DATA_ORDER_FORKLIFT = 101,
            CMD_DATA_STATE = 102
        }
        public class OrderItem
        {
            public OrderItem() { }
            public String OrderId;
            public int planId { get; set; }
            public int productId { get; set; }
            public int productDetailID { get; set; }
            public TyeRequest typeReq; // FL: ForkLift// BM: BUFFER MACHINE // PR: Pallet return
            public String activeDate;
            public int timeWorkId;
            public String palletStatus;
            public int palletId;
            public int updUsrId;
            public int lengthPallet;
            public String dataRequest;
            public bool status = false; // chua hoan thanh
            public DataPallet palletAtMachine;
            public String userName;
        }
        public string userName { get; set; } // dia chi Emei
        public string codeID { get; set; }
        public List<OrderItem> oneOrderList { get; set; }
        public int orderedAmount = 0;
        public int doneAmount = 0;
        public DeviceItem()
        {
            oneOrderList = new List<OrderItem>();
        }
        public void state(CommandRequest pCommandRequest, String data)
        {
            switch (pCommandRequest)
            {
                case CommandRequest.CMD_DATA_ORDER_BUFFERTOMACHINE:
                    break;
                case CommandRequest.CMD_DATA_ORDER_FORKLIFT:
                    break;
                case CommandRequest.CMD_DATA_STATE:
                    break;
            }
        }
        public void RemoveFirstOrder()
        {
            if (oneOrderList.Count > 0)
            {
                oneOrderList.RemoveAt(0);
            }
        }
        public void AddOrder(OrderItem hasOrder)
        {
            oneOrderList.Add(hasOrder);
        }
        public OrderItem GetOrder()
        {
            if (oneOrderList.Count > 0)
            {
                return oneOrderList[0];
            }
            return null;
        }
        public void ClearOrderList()
        {
            if (oneOrderList.Count > 0)
            {
                oneOrderList.Clear();
            }
        }
        public void rounter(String data)
        {

        }
        public void ParseData(String dataReq)
        {

            JObject results = JObject.Parse(dataReq);
            int typeReq= (int)results["typeReq"];
            if (typeReq==(int)TyeRequest. TYPEREQUEST_FORLIFT_TO_BUFFER)
            {
                OrderItem order = new OrderItem();
                order.typeReq = (TyeRequest)typeReq;
                order.userName = (String)results["userName"];
                order.productDetailID = (int)results["productDetailId"];
                order.productId = (int)results["productId"];
                order.planId = (int)results["planId"];
                order.timeWorkId = (int)results["timeWorkId"];
                order.activeDate = (string)results["activeDate"];
               // order.palletStatus = (String)results["palletStatus"];
                dynamic product = new JObject();
                product.timeWorkId = order.timeWorkId;
                product.activeDate = order.activeDate;
                product.productId = order.productId;

                // chu y sua 
                product.palletStatus= PalletStatus.P.ToString();
                order.dataRequest = product.ToString();
                oneOrderList.Add(order);
            }
            else if (typeReq == (int)TyeRequest.TYPEREQUEST_BUFFER_TO_MACHINE)
            {
                int len = (int)results["length"];

                for (int i = 0; i < len; i++)
                {
                    OrderItem order = new OrderItem();
                    order.typeReq = (TyeRequest)typeReq;
                    order.userName = (String)results["userName"];
                    order.productDetailID = (int)results["productDetailId"];
                    order.productId = (int)results["productId"];
                    order.planId=(int)results["planId"];
                    order.timeWorkId = (int)results["timeWorkId"];
                    order.activeDate = (string)results["activeDate"];
                   // order.palletStatus = (String)results["palletStatus"];
                    String jsonDPst=(string)results["datapallet"][i];
                    JObject stuffPallet =JObject.Parse(jsonDPst);
                    double xx = (double)stuffPallet["line"]["x"];
                    double yy = (double)stuffPallet["line"]["y"];
                    double angle = (double)stuffPallet["line"]["angle"];
                    int row = (int)stuffPallet["pallet"]["row"];
                    int bay = (int)stuffPallet["pallet"]["bay"];
                    int direct = (int)stuffPallet["pallet"]["direct"];
                    order.palletAtMachine = new DataPallet() {linePos=new Pose(xx,yy,angle), row=row, bay=bay,direct=direct};
                    dynamic product = new JObject();
                    product.timeWorkId = order.timeWorkId;
                    product.activeDate = order.activeDate;
                    product.productId = order.productId;
                    // chu y sua 
                    product.palletStatus = PalletStatus.P.ToString(); // W
                    order.dataRequest = product.ToString();
                    oneOrderList.Add(order);
                }
            }
            else if (typeReq == (int)TyeRequest.TYPEREQUEST_MACHINE_TO_RETURN)
            {
                int len = (int)results["length"];

                for (int i = 0; i < len; i++)
                {
                    OrderItem order = new OrderItem();
                    order.typeReq = (TyeRequest)typeReq;
                    order.userName = (String)results["userName"];
                    order.activeDate = (string)results["activeDate"];
                    //order.palletStatus = (String)results["palletStatus"];
                    String jsonDPst = (string)results["datapallet"][i];
                    JObject stuffPallet = JObject.Parse(jsonDPst);
                    double xx = (double)stuffPallet["line"]["x"];
                    double yy = (double)stuffPallet["line"]["y"];
                    double angle = (double)stuffPallet["line"]["angle"];
                    int row = (int)stuffPallet["pallet"]["row"];
                    int bay = (int)stuffPallet["pallet"]["bay"];
                    int direct = (int)stuffPallet["pallet"]["direct"];
                    order.palletAtMachine = new DataPallet() { linePos = new Pose(xx, yy, angle), row = row, bay = bay, direct = direct };
                    dynamic product = new JObject();
                    product.timeWorkId = order.timeWorkId;
                    product.activeDate = order.activeDate;
                    product.productId = order.productId;
                    // chu y sua 
                    product.palletStatus = PalletStatus.F.ToString();
                    order.dataRequest = product.ToString();
                    oneOrderList.Add(order);
                }
            }
            else if (typeReq == (int)TyeRequest.TYPEREQUEST_BUFFER_TO_RETURN)
            {
                OrderItem order = new OrderItem();
                order.typeReq = (TyeRequest)typeReq;
                order.userName = (String)results["userName"];
                order.productDetailID = (int)results["productDetailId"];
                order.productId = (int)results["productId"];
                order.timeWorkId = (int)results["timeWorkId"];
                order.activeDate = (string)results["activeDate"];
               // order.palletStatus = (String)results["palletStatus"];
                dynamic product = new JObject();
                product.timeWorkId = order.timeWorkId;
                product.activeDate = order.activeDate;
                product.productId = order.productId;
                // chu y sua 
                product.palletStatus = PalletStatus.W.ToString();
                order.dataRequest = product.ToString();
                oneOrderList.Add(order);
            }
        }

    }
}
/*
 * {
  "deviceId": "1",
  "productId": "4",
  "productDetailId": "16",
  "typeReq": "2",
  "userName": "tab1",
  "planId": 1,
  "activeDate": "2018-12-25",
  "length":3,
  "datapallet": [
    {
      "line":{"X":1,"X":1,Angle:""},
      "pallet": {"row":1,"bay":2,"direct":1}
    },
    {
      "line":{"X":1,"X":1,Angle:""},
      "pallet": {"row":1,"bay":2,"direct":1}
    },
    {
      "line":{"X":1,"X":1,Angle:""},
      "pallet": {"row":1,"bay":2,"direct":1}
    }
  ]
}
 */
