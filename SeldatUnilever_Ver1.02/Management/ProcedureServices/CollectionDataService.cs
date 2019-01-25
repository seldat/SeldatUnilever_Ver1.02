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

        public Pose GetCheckInBuffer()
        {
            Pose poseTemp = null;
            String collectionData = RequestDataProcedure(order.dataRequest, Global_Object.url + "plan/getListPlanPallet");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
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
                        poseTemp = new Pose(x, y, angle * Math.PI / 180.0);
                        planId = order.planId;
                        break;

                    }
                }
            }
            return poseTemp;
        }
        public Pose GetAnyPointInBuffer() // đổi 
        {
            Pose poseTemp = null;
            String collectionData = RequestDataProcedure(order.dataRequest, Global_Object.url + "plan/getListPlanPallet");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
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
                        poseTemp = new Pose(x, y, angle * Math.PI / 180.0);
                        break;

                    }
                }
            }
            return poseTemp;
        }

        public Pose GetFrontLineBuffer()
        {
            Pose poseTemp = null;
            String collectionData = RequestDataProcedure(order.dataRequest, Global_Object.url + "plan/getListPlanPallet");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
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
                        poseTemp = new Pose(x, y, angle * Math.PI / 180.0);
                        break;

                    }
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
                poseTemp = new Pose(x, y, angle * Math.PI / 180.0);

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
                poseTemp = new Pose(x, y, angle * Math.PI / 180.0);

            }
            return poseTemp;
        }

        /*
         */
        public String GetInfoOfPalletBuffer(TrafficRobotUnity.PistonPalletCtrl pisCtrl)
        {
            JInfoPallet infoPallet = new JInfoPallet();
            String collectionData = RequestDataProcedure(order.dataRequest, Global_Object.url + "plan/getListPlanPallet");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
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
                        int direct = (int)stuff["pallet"]["direct"];

                        infoPallet.pallet = pisCtrl; /* dropdown */
                        infoPallet.bay = bay;
                        infoPallet.hasSubLine = "yes"; /* no */
                        infoPallet.direction = (TrafficRobotUnity.BrDirection)direct; /* right */
                        infoPallet.row = row;
                        break;
                    }
                }
            }
            return JsonConvert.SerializeObject(infoPallet);
        }

        public String GetInfoOfPalletMachine(TrafficRobotUnity.PistonPalletCtrl pisCtrl)
        {
            JInfoPallet infoPallet = new JInfoPallet();

            infoPallet.pallet = pisCtrl; /* dropdown */
            infoPallet.bay = order.palletAtMachine.bay;
            infoPallet.hasSubLine = "yes"; /* no */
            infoPallet.direction = (TrafficRobotUnity.BrDirection)order.palletAtMachine.direct; /* right */
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
                int direct = (int)stuff["pallet"]["direct"];

                infoPallet.pallet = pisCtrl; /* dropdown */
                infoPallet.bay = bay;
                infoPallet.hasSubLine = "yes"; /* no */
                infoPallet.direction = (TrafficRobotUnity.BrDirection)direct; /* right */
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
