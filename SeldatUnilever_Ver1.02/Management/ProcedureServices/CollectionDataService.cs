using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeldatMRMS;
using SeldatMRMS.Management;
using SelDatUnilever_Ver1._00.Communication.HttpBridge;
using SelDatUnilever_Ver1._00.Management.ProcedureServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SelDatUnilever_Ver1
{


    public class CollectionDataService
    {
        public enum PalletStatus
        {
            F = 200, // Free pallet
            W = 201, // Have Pallet
            P = 202

        }
        //public int planID { get; set; }
        // public int productID { get; set; }
        // public int planID { get; set; }
        protected OrderItem order;
        //public String typeRequest; // FL: ForkLift// BM: BUFFER MACHINE // PR: Pallet return
        //public String activeDate;
        // public int timeWorkID;
        public List<Pose> checkInBuffer = new List<Pose>();
        protected BridgeClientRequest clientRequest;
        protected int palletId { get; set; }
        protected int planId { get; set; }
        public CollectionDataService()
        {
            clientRequest = new BridgeClientRequest();
            clientRequest.ReceiveResponseHandler += ReceiveResponseHandler;
            planId = -1;
        }
        public CollectionDataService(OrderItem order)
        {
            this.order = order;
            clientRequest = new BridgeClientRequest();
            clientRequest.ReceiveResponseHandler += ReceiveResponseHandler;

        }
        public virtual void AssignAnOrder(OrderItem order)
        {
            this.order = order;
        }
        public String createPlanBuffer()
        {
            dynamic product = new JObject();
            product.timeWorkId= 1;
            product.activeDate =order.activeDate;
            product.productId =order.productId;
            product.productDetailId=order.productDetailId;
            product.updUsrId = Global_Object.userLogin;
            product.palletAmount=1;
            String response = RequestDataProcedure(product.ToString(), Global_Object.url + "plan/createPlanPallet");
            return response;
        }
        public String RequestDataProcedure(String dataReq, String url)
        {
            //String url = Global_Object.url+"plan/getListPlanPallet";
            // String url = "http://localhost:8080";
            var data = clientRequest.PostCallAPI(url, dataReq);
            if (data.Result != null)
            {
                return data.Result;
            }
            return null;
        }

        public Pose GetCheckInBuffer(bool onPlandId=false)
        {
            Pose poseTemp = null;
            String collectionData = RequestDataProcedure(order.dataRequest, Global_Object.url + "plan/getListPlanPallet");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
                if (onPlandId)
                {
                    foreach (var result in results)
                    {
                        int temp_planId = (int)result["planId"];

                        if (temp_planId == order.planId)
                        {
                            var bufferResults = result["buffers"][0];
                            String checkinResults = (String)bufferResults["bufferCheckIn"];
                            JObject stuff = JObject.Parse(checkinResults);
                            double x = (double)stuff["checkin"]["x"];
                            double y = (double)stuff["checkin"]["y"];
                            double angle = (double)stuff["checkin"]["angle"];
                            poseTemp = new Pose(x, y, angle);
                            planId = order.planId;
                            break;

                        }
                    }
                }
                else
                {
                    var result = results[0];
                     var bufferResults = result["buffers"][0];
                    String checkinResults = (String)bufferResults["bufferCheckIn"];
                    JObject stuff = JObject.Parse(checkinResults);
                    double x = (double)stuff["checkin"]["x"];
                    double y = (double)stuff["checkin"]["y"];
                    double angle = (double)stuff["checkin"]["angle"];
                    poseTemp = new Pose(x, y, angle);
                    planId = order.planId;
                }
            }
            return poseTemp;
        }
        public Pose GetAnyPointInBuffer(bool onPlandId = false) // đổi 
        {
            Pose poseTemp = null;
            String collectionData = RequestDataProcedure(order.dataRequest, Global_Object.url + "plan/getListPlanPallet");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
                if (onPlandId)
                {
                    foreach (var result in results)
                    {
                        int temp_planId = (int)result["planId"];
                        if (temp_planId == order.planId)
                        {
                            var bufferResults = result["buffers"][0];
                            String checkinResults = (String)bufferResults["bufferCheckIn"];
                            JObject stuff = JObject.Parse(checkinResults);
                            double x = (double)stuff["headpoint"]["x"];
                            double y = (double)stuff["headpoint"]["y"];
                            double angle = (double)stuff["headpoint"]["angle"];
                            poseTemp = new Pose(x, y, angle);
                            break;

                        }
                    }
                }
                else
                {
                    var result = results[0];
                    var bufferResults = result["buffers"][0];
                    String checkinResults = (String)bufferResults["bufferCheckIn"];
                    JObject stuff = JObject.Parse(checkinResults);
                    double x = (double)stuff["headpoint"]["x"];
                    double y = (double)stuff["headpoint"]["y"];
                    double angle = (double)stuff["headpoint"]["angle"];
                    poseTemp = new Pose(x, y, angle);
                }
            }
            return poseTemp;
        }

        public Pose GetFrontLineBuffer(bool onPlandId = false)
        {
            Pose poseTemp = null;
            String collectionData = RequestDataProcedure(order.dataRequest, Global_Object.url + "plan/getListPlanPallet");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
                if (onPlandId)
                {
                    foreach (var result in results)
                    {
                        int temp_planId = (int)result["planId"];
                        if (temp_planId == order.planId)
                        {
                            var bufferResults = result["buffers"][0];
                            var palletInfo = bufferResults["pallets"][0];
                            JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);
                            double x = (double)stuff["line"]["x"];
                            double y = (double)stuff["line"]["y"];
                            double angle = (double)stuff["line"]["angle"];
                            poseTemp = new Pose(x, y, angle);
                            break;

                        }
                    }
                }
                else
                {
                    var result = results[0];
                    var bufferResults = result["buffers"][0];
                    var palletInfo = bufferResults["pallets"][0];
                    JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);
                    double x = (double)stuff["line"]["x"];
                    double y = (double)stuff["line"]["y"];
                    double angle = (double)stuff["line"]["angle"];
                    poseTemp = new Pose(x, y, angle);
                }
            }
            return poseTemp;
        }

        public Pose GetFrontLineMachine()
        {
            return order.palletAtMachine.linePos;
        }

        public Pose GetCheckInReturn()
        {
            dynamic product = new JObject();
            product.palletStatus = order.palletStatus;
            Pose poseTemp = null;
            String collectionData = RequestDataProcedure(product.ToString(), Global_Object.url + "buffer/getListBufferReturn");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
                var bufferResults = results[0];
                String checkinResults = (String)bufferResults["bufferCheckIn"];
                JObject stuff = JObject.Parse(checkinResults);
                double x = (double)stuff["x"];
                double y = (double)stuff["y"];
                double angle = (double)stuff["angle"];
                poseTemp = new Pose(x, y, angle );

            }
            return poseTemp;
        }

        public Pose GetFrontLineReturn()
        {

            Pose poseTemp = null;
            dynamic product = new JObject();
            product.palletStatus = order.palletStatus;
            String collectionData = RequestDataProcedure(product.ToString(), Global_Object.url + "buffer/getListBufferReturn");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
                var bufferResults = results[0];
                var palletInfo = bufferResults["pallets"][0];
                JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);
                double x = (double)stuff["line"]["x"];
                double y = (double)stuff["line"]["y"];
                double angle = (double)stuff["line"]["angle"];
                poseTemp = new Pose(x, y, angle);

            }
            return poseTemp;
        }

        /*
         */
        public String GetInfoOfPalletBuffer(TrafficRobotUnity.PistonPalletCtrl pisCtrl, bool onPlandId = false)
        {
            JInfoPallet infoPallet = new JInfoPallet();
            String collectionData = RequestDataProcedure(order.dataRequest, Global_Object.url + "plan/getListPlanPallet");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
                if (onPlandId)
                {
                    foreach (var result in results)
                    {
                        int temp_planId = (int)result["planId"];
                        if (temp_planId == order.planId)
                        {


                            var bufferResults = result["buffers"][0];
                            var palletInfo = bufferResults["pallets"][0];
                            palletId = (int)palletInfo["palletId"];
                            JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);
                            int row = (int)stuff["pallet"]["row"];
                            int bay = (int)stuff["pallet"]["bay"];
                            int directMain = (int)stuff["pallet"]["dir_main"];
                            int directSub = (int)stuff["pallet"]["dir_sub"];
                            int directOut = (int)stuff["pallet"]["dir_out"];
                            string subline = (string)stuff["pallet"]["hasSubLine"];

                            infoPallet.pallet = pisCtrl; /* dropdown */
                            infoPallet.dir_main = (TrafficRobotUnity.BrDirection)directMain;
                            infoPallet.bay = bay;
                            infoPallet.hasSubLine = subline; /* yes or no */
                            infoPallet.dir_sub = (TrafficRobotUnity.BrDirection)directSub; /* right */
                            infoPallet.dir_out = (TrafficRobotUnity.BrDirection)directOut;
                            infoPallet.row = row;
                            break;
                        }
                    }
                }
                else
                {
                    var result= results[0];
                    var bufferResults = result["buffers"][0];
                    var palletInfo = bufferResults["pallets"][0];
                    palletId = (int)palletInfo["palletId"];
                    JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);
                    int row = (int)stuff["pallet"]["row"];
                    int bay = (int)stuff["pallet"]["bay"];
                    int directMain = (int)stuff["pallet"]["dir_main"];
                    int directSub = (int)stuff["pallet"]["dir_sub"];
                    int directOut = (int)stuff["pallet"]["dir_out"];
                    string subline = (string)stuff["pallet"]["hasSubLine"];

                    infoPallet.pallet = pisCtrl; /* dropdown */
                    infoPallet.dir_main = (TrafficRobotUnity.BrDirection)directMain;
                    infoPallet.bay = bay;
                    infoPallet.hasSubLine = subline; /* yes or no */
                    infoPallet.dir_sub = (TrafficRobotUnity.BrDirection)directSub; /* right */
                    infoPallet.dir_out = (TrafficRobotUnity.BrDirection)directOut;
                    infoPallet.row = row;
                }
            }
            return JsonConvert.SerializeObject(infoPallet);
        }

        public String GetInfoOfPalletMachine(TrafficRobotUnity.PistonPalletCtrl pisCtrl)
        {
            JInfoPallet infoPallet = new JInfoPallet();

            infoPallet.pallet = pisCtrl; /* dropdown */
            infoPallet.bay = order.palletAtMachine.bay;
            infoPallet.hasSubLine = "no"; /* no */
            infoPallet.dir_main = (TrafficRobotUnity.BrDirection)order.palletAtMachine.directMain;
            infoPallet.dir_sub = (TrafficRobotUnity.BrDirection)order.palletAtMachine.directSub;
            infoPallet.dir_out= (TrafficRobotUnity.BrDirection)order.palletAtMachine.directOut;
            infoPallet.row = order.palletAtMachine.row;

            return JsonConvert.SerializeObject(infoPallet);
        }

        public String GetInfoOfPalletReturn(TrafficRobotUnity.PistonPalletCtrl pisCtrl)
        {
            JInfoPallet infoPallet = new JInfoPallet();
            dynamic product = new JObject();
            product.palletStatus = order.palletStatus;
            String collectionData = RequestDataProcedure(product.ToString(), Global_Object.url + "buffer/getListBufferReturn");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
                var bufferResults = results[0];
                var palletInfo = bufferResults["pallets"][0];
                palletId = (int)palletInfo["palletId"];
                JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);
                int row = (int)stuff["pallet"]["row"];
                int bay = (int)stuff["pallet"]["bay"];
                int directMain = (int)stuff["pallet"]["dir_main"];
                int directSub = (int)stuff["pallet"]["dir_sub"];
                int directOut = (int)stuff["pallet"]["dir_out"];
                infoPallet.pallet = pisCtrl; /* dropdown */
                infoPallet.bay = bay;
                infoPallet.hasSubLine = "yes"; /* no */
                infoPallet.dir_main = (TrafficRobotUnity.BrDirection)directMain; 
                infoPallet.dir_sub = (TrafficRobotUnity.BrDirection)directSub;
                infoPallet.dir_out = (TrafficRobotUnity.BrDirection)directOut;
                infoPallet.row = row;

            }
            return JsonConvert.SerializeObject(infoPallet);
        }

        public void UpdatePalletState(PalletStatus palletStatus)
        {
            String url = Global_Object.url + "pallet/updatePalletStatus";
            dynamic product = new JObject();
            product.palletId = palletId;
            product.planId = planId;
            product.palletStatus = palletStatus.ToString();
            product.updUsrId = Global_Object.userLogin;
            var data = clientRequest.PostCallAPI(url, product.ToString());
            if (data.Result == null)
            {
                ErrorHandler(ProcedureMessages.ProcMessage.MESSAGE_ERROR_UPDATE_PALLETSTATUS);
            }
        }

        protected virtual void ReceiveResponseHandler(String msg) { }
        protected virtual void ErrorHandler(ProcedureMessages.ProcMessage procMessage) { }
    }
}
