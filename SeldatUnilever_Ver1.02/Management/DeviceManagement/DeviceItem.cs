﻿using Newtonsoft.Json.Linq;
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
    public class DeviceItem : NotifyUIBase
    {
        public enum StatusOrderCode
        {
            ORDER_STATUS_SUCCESS=200,
            ORDER_STATUS_ERROR_DATA = 201,
            ORDER_STATUS_NOACCEPTED = 202

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

            public TyeRequest typeReq { get; set; } // FL: ForkLift// BM: BUFFER MACHINE // PR: Pallet return
            public String activeDate { get; set; }
            public int timeWorkId;
            public String palletStatus;
            public int palletId;
            public int updUsrId;
            public int lengthPallet;
            public String dataRequest;
            public bool status = false; // chua hoan thanh
            public DataPallet palletAtMachine;
            public String userName;
            public int bufferId;
            public int palletAmount;
        }
        public string userName { get; set; } // dia chi Emei
        public string codeID;
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
                    oneOrderList.Add(order);
                }
                else if (typeReq == (int)TyeRequest.TYPEREQUEST_BUFFER_TO_MACHINE)
                {
                    int len = (int)results["length"];
                    int palletAmountInBuffer = (int)results["palletAmount"];
                    int productDetailId = (int)results["productDetailId"];
                    int cntOrderReg = 0;
                    int orderAmount = 0;
                    foreach (OrderItem ord in oneOrderList)
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
                        statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderCode.ORDER_STATUS_NOACCEPTED, ErrorMessage = "" };
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
                        order.planId = (int)results["planId"];
                        order.deviceId = (int)results["deviceId"];
                        order.timeWorkId = (int)results["timeWorkId"];
                        order.activeDate = (string)results["activeDate"];
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
                        order.palletAtMachine = new DataPallet() { linePos = new Pose(xx, yy, angle), row = row, bay = bay, directMain = directMain, directSub = directSub, directOut = directOut };
                        dynamic product = new JObject();
                        product.timeWorkId = order.timeWorkId;
                        product.activeDate = order.activeDate;
                        product.productId = order.productId;
                        product.productDetailId = order.productDetailId;
                        // chu y sua 
                        product.palletStatus = PalletStatus.W.ToString(); // W
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
                        order.palletAtMachine = new DataPallet() { linePos = new Pose(xx, yy, angle), row = row, bay = bay, directMain = directMain, directSub = directSub };
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
                    oneOrderList.Add(order);
                }
                else if (typeReq == (int)TyeRequest.TYPEREQUEST_CLEAR)
                {
                    String userName = (String)results["userName"];
                    // kiểm tra quy trình và hủy task 

                    oneOrderList.Remove(oneOrderList.Find(e=>e.userName==userName));
                }
                statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderCode.ORDER_STATUS_SUCCESS, ErrorMessage = "" };
            }
            catch(Exception e)
            {
                statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderCode.ORDER_STATUS_ERROR_DATA, ErrorMessage = e.Message };
                return statusOrderResponse;
            }
            return statusOrderResponse;
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
