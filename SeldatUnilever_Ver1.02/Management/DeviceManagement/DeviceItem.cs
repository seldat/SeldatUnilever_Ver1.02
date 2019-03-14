using Newtonsoft.Json.Linq;
using SeldatMRMS;
using SeldatMRMS.Management.DoorServices;
using SelDatUnilever_Ver1._00.Communication.HttpBridge;
using System; 
using System.Collections.Generic;
using static DoorControllerService.DoorService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SelDatUnilever_Ver1.CollectionDataService;

namespace SelDatUnilever_Ver1._00.Management.DeviceManagement
{
    public class DeviceItem : NotifyUIBase
    {
        public enum StatusOrderResponseCode
        {
            ORDER_STATUS_RESPONSE_SUCCESS=200,
            ORDER_STATUS_RESPONSE_ERROR_DATA = 201,
            ORDER_STATUS_RESPONSE_NOACCEPTED = 202,
            ORDER_WAITING=300,
            ORDER_DELIVERING = 301,
            ORDER_FINISH = 302,


        }

        public class StatusOrderResponse
        {
            public int status;
            public String ErrorMessage;
        }
        public enum PalletCtrl
        {
            Pallet_CTRL_DOWN = 0,
            Pallet_CTRL_UP = 1

        }
        public class DataPallet
        {
            public int row;
            public int bay;
            public int directMain;
            public int directSub;
            public int directOut;
            public int line_ord;
            public PalletCtrl palletCtrl;
            public Pose linePos;
        }
        public enum TyeRequest
        {
            TYPEREQUEST_FORLIFT_TO_BUFFER = 1,
            TYPEREQUEST_BUFFER_TO_MACHINE = 2,
            TYPEREQUEST_BUFFER_TO_RETURN = 3,
            TYPEREQUEST_MACHINE_TO_RETURN = 4,
            TYPEREQUEST_RETURN_TO_GATE = 5,
            TYPEREQUEST_CLEAR = 6,
            TYPEREQUEST_OPEN_FRONTDOOR_DELIVERY_PALLET = 7,
            TYPEREQUEST_CLOSE_FRONTDOOR_DELIVERY_PALLET = 8,
            TYPEREQUEST_OPEN_FRONTDOOR_RETURN_PALLET = 9,
            TYPEREQUEST_CLOSE_FRONTDOOR_RETURN_PALLET = 10,
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
            private String OrderId { get; set; }
            public int planId { get; set; }
            public int deviceId;
            public String productDetailName { get; set; }
            public int productId { get; set; }
            public int productDetailId { get; set; }
            public StatusOrderResponse status { get; set; }

            public TyeRequest typeReq { get; set; } // FL: ForkLift// BM: BUFFER MACHINE // PR: Pallet return
            public String activeDate { get; set; }
            public int timeWorkId;
            public String palletStatus;
            public int palletId;
            public int updUsrId;
            public int lengthPallet;
            public String dataRequest;
           // public bool status = false; // chua hoan thanh
            public DataPallet palletAtMachine;
            public String userName;
            public int bufferId;
            public int palletAmount;
            
        }
        public string userName { get; set; } // dia chi Emei
        public string codeID;
        public List<OrderItem> PendingOrderList { get; set; }
        public List<OrderItem> OrderedItemList { get; set; }
        public int orderedAmount = 0;
        public int doneAmount = 0;

        public DeviceItem()
        {
            PendingOrderList = new List<OrderItem>();
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
        public void StatusOrderItem(OrderItem item, StatusOrderResponse statusOrderResponse)
        {

        }
        public void RemoveFirstOrder()
        {
            if (PendingOrderList.Count > 0)
            {
                PendingOrderList.RemoveAt(0);
            }
        }
        public void AddOrder(OrderItem hasOrder)
        {
            PendingOrderList.Add(hasOrder);
        }
        public OrderItem GetOrder()
        {
            if (PendingOrderList.Count > 0)
            {
                return PendingOrderList[0];
            }
            return null;
        }
        public void ClearOrderList()
        {
            if (PendingOrderList.Count > 0)
            {
                PendingOrderList.Clear();
            }
        }
        public void rounter(String data)
        {

        }
        public StatusOrderResponse ParseData(String dataReq)
        {
            StatusOrderResponse statusOrderResponse = null;
            try
            {
                JObject results = JObject.Parse(dataReq);
                int typeReq = (int)results["typeReq"];
                if (typeReq == (int)TyeRequest.TYPEREQUEST_FORLIFT_TO_BUFFER)
                {
                    OrderItem order = new OrderItem();
                    order.typeReq = (TyeRequest)typeReq;
                    order.userName = (String)results["userName"];
                    order.productDetailId = (int)results["productDetailId"];
                    order.productId = (int)results["productId"];
                    order.planId = (int)results["planId"];
                    order.deviceId = (int)results["deviceId"];
                    order.timeWorkId = (int)results["timeWorkId"];
                    order.activeDate = (string)results["activeDate"];
                    // order.palletStatus = (String)results["palletStatus"];
                    dynamic product = new JObject();
                    product.timeWorkId = order.timeWorkId;
                    product.activeDate = order.activeDate;
                    product.productId = order.productId;
                    product.productDetailId = order.productDetailId;

                    // chu y sua 
                    product.palletStatus = PalletStatus.P.ToString();
                    order.dataRequest = product.ToString();

                    if (Convert.ToInt32(CreatePlanBuffer(order)) > 0)
                    {
                        PendingOrderList.Add(order);
                    }
                    else
                    {
                        statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_NOACCEPTED, ErrorMessage = "" };
                        return statusOrderResponse;
                    }
                }
                else if (typeReq == (int)TyeRequest.TYPEREQUEST_BUFFER_TO_MACHINE)
                {
                    Console.WriteLine("----------------------------------------------------");
                    Console.WriteLine(dataReq);
                    int len = (int)results["length"];
                    int palletAmountInBuffer = (int)results["palletAmount"];
                    int productDetailId = (int)results["productDetailId"];
                    int cntOrderReg = 0;
                    int orderAmount = 0;
                    foreach (OrderItem ord in PendingOrderList)
                    {
                        if (productDetailId == ord.productDetailId)
                        {
                            cntOrderReg++;
                        }
                    }

                    if (cntOrderReg == 0) // chưa có order với productdetailID nào uoc đăng ký. thêm vào đúng số lượng trong orderlist
                    {
                        if (len <= palletAmountInBuffer) // nếu số lượn yêu cầu nhỏ hơn bằng số pallet có trong buffer add vào orderlist 
                            orderAmount = len;
                        else // nếu số lượng yêu cầu nhiều hơn số pallet có trong buffer thì add vào orderlist
                            orderAmount = palletAmountInBuffer;
                    }
                    else if (cntOrderReg >= palletAmountInBuffer) // số lượng yêu cầu trước đó bằng hoặc hơn số lượng yêu cầu hiện tại. không duoc phép đưa vào thêm
                    {
                        statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_NOACCEPTED, ErrorMessage = "" };
                        return statusOrderResponse;
                    }
                    else if (cntOrderReg < palletAmountInBuffer) // số lượng yêu cầu hiện tại nhỏ hơn thì phải tính lại số lượng để bổ sung vào thêm
                    {
                        int availableOrder = palletAmountInBuffer - cntOrderReg; // tính số lượng pallet có thể 
                        int willOrder = availableOrder - len; // số lượng pallet sẽ duoc add thêm vào orederlist
                        if (willOrder > 0)
                        {
                            orderAmount = willOrder;
                        }
                        else if (willOrder == 0)
                        {
                            orderAmount = len;
                        }
                        else
                        {
                            orderAmount = availableOrder;
                        }

                    }
                    for (int i = 0; i < orderAmount; i++)
                    {

                        OrderItem order = new OrderItem();
                        order.typeReq = (TyeRequest)typeReq;
                        order.userName = (String)results["userName"];
                        order.productDetailId = (int)results["productDetailId"];
                        order.productDetailName = (String)results["productDetailName"];
                        order.productId = (int)results["productId"];
                       // order.planId = (int)results["planId"];
                        order.deviceId = (int)results["deviceId"];
                        order.timeWorkId = 1;
                       // order.activeDate = (string)DateTime.Now.ToString("yyyy-MM-dd");
                        // order.palletStatus = (String)results["palletStatus"];
                        String jsonDPst = (string)results["datapallet"][i];
                        JObject stuffPallet = JObject.Parse(jsonDPst);
                        double xx = (double)stuffPallet["line"]["x"];
                        double yy = (double)stuffPallet["line"]["y"];
                        double angle = (double)stuffPallet["line"]["angle"];
                        int row = (int)stuffPallet["pallet"]["row"];
                        int bay = (int)stuffPallet["pallet"]["bay"];
                        int directMain = (int)stuffPallet["pallet"]["dir_main"];
                        int directSub = (int)stuffPallet["pallet"]["dir_sub"];
                        int directOut = (int)stuffPallet["pallet"]["dir_out"];
                        int line_ord = (int)stuffPallet["pallet"]["line_ord"];
                        order.palletAtMachine = new DataPallet() { linePos = new Pose(xx, yy, angle), row = row, bay = bay, directMain = directMain, directSub = directSub, directOut = directOut,line_ord=line_ord };
                        dynamic product = new JObject();
                        product.timeWorkId = order.timeWorkId;
                        product.activeDate = order.activeDate;
                        product.productId = order.productId;
                        product.productDetailId = order.productDetailId;
                        // chu y sua 
                        product.palletStatus = PalletStatus.W.ToString(); // W
                        order.dataRequest = product.ToString();
                        PendingOrderList.Add(order);


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
                        order.deviceId = (int)results["deviceId"];
                        //order.palletStatus = (String)results["palletStatus"];
                        String jsonDPst = (string)results["datapallet"][i];
                        JObject stuffPallet = JObject.Parse(jsonDPst);
                        double xx = (double)stuffPallet["line"]["x"];
                        double yy = (double)stuffPallet["line"]["y"];
                        double angle = (double)stuffPallet["line"]["angle"];
                        int row = (int)stuffPallet["pallet"]["row"];
                        int bay = (int)stuffPallet["pallet"]["bay"];
                        int directMain = (int)stuffPallet["pallet"]["dir_main"];
                        int directSub = (int)stuffPallet["pallet"]["dir_sub"];
                        int dir_out = (int)stuffPallet["pallet"]["dir_out"];
                        int line_ord = (int)stuffPallet["pallet"]["line_ord"];
                        
                        order.palletAtMachine = new DataPallet() { linePos = new Pose(xx, yy, angle), row = row, bay = bay, directMain = directMain, directSub = directSub,directOut=dir_out ,line_ord=line_ord};
                        dynamic product = new JObject();
                     //   product.timeWorkId = order.timeWorkId;
                     //   product.activeDate = order.activeDate;
                     //   product.productId = order.productId;

                        // chu y sua 
                        product.palletStatus = PalletStatus.F.ToString();
                        order.dataRequest = product.ToString();
                        PendingOrderList.Add(order);
                    }
                }
                else if (typeReq == (int)TyeRequest.TYPEREQUEST_BUFFER_TO_RETURN)
                {
                    OrderItem order = new OrderItem();
                    order.typeReq = (TyeRequest)typeReq;
                    order.userName = (String)results["userName"];
                    order.deviceId = (int)results["deviceId"];
                    order.productDetailId = (int)results["productDetailId"];
                    order.productId = (int)results["productId"];
                    order.timeWorkId = (int)results["timeWorkId"];
                    order.activeDate = (string)results["activeDate"];
                    // order.palletStatus = (String)results["palletStatus"];
                    dynamic product = new JObject();
                    product.timeWorkId = order.timeWorkId;
                    product.activeDate = order.activeDate;
                    product.productId = order.productId;
                    product.productDetailId = order.productDetailId;
                    // chu y sua 
                    product.palletStatus = PalletStatus.W.ToString();
                    order.dataRequest = product.ToString();
                    PendingOrderList.Add(order);
                }
                else if (typeReq == (int)TyeRequest.TYPEREQUEST_CLEAR)
                {
                    String userName = (String)results["userName"];
                    // kiểm tra quy trình và hủy task 

                    PendingOrderList.Remove(PendingOrderList.Find(e=>e.userName==userName));
                }
                else if(typeReq == (int)TyeRequest.TYPEREQUEST_OPEN_FRONTDOOR_DELIVERY_PALLET)
                {
                    // same deviceID forklift
                    try
                    {
                        new DoorManagementService().DoorMezzamineUp.Open(DoorType.DOOR_FRONT);                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("control door failed");
                        statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_ERROR_DATA, ErrorMessage = e.Message };
                        return statusOrderResponse;
                    }
                }
                else if (typeReq == (int)TyeRequest.TYPEREQUEST_CLOSE_FRONTDOOR_DELIVERY_PALLET)
                {
                    // same deviceID forklift
                    try
                    {
                        new DoorManagementService().DoorMezzamineUp.Close(DoorType.DOOR_FRONT);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("control door failed");
                        statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_ERROR_DATA, ErrorMessage = e.Message };
                        return statusOrderResponse;
                    }
                }
                statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_SUCCESS, ErrorMessage = "" };
            }
            catch(Exception e)
            {
                statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_ERROR_DATA, ErrorMessage = e.Message };
                return statusOrderResponse;
            }
            return statusOrderResponse;
        }
        public String RequestDataProcedure(String dataReq, String url)
        {
            //String url = Global_Object.url+"plan/getListPlanPallet";
            BridgeClientRequest clientRequest=new BridgeClientRequest();
            // String url = "http://localhost:8080";
            var data = clientRequest.PostCallAPI(url, dataReq);
            if (data.Result != null)
            {
                return data.Result;
            }
            return null;
        }
        public String CreatePlanBuffer(OrderItem order)
        {
            dynamic product = new JObject();
            product.timeWorkId = 1;
            product.activeDate = order.activeDate;
            product.productId = order.productId;
            product.productDetailId = order.productDetailId;
            product.updUsrId = Global_Object.userLogin;
            product.deviceId = order.deviceId;
            product.palletAmount = 1;
            String response = RequestDataProcedure(product.ToString(), Global_Object.url + "plan/createPlanPallet");
            return response;
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
