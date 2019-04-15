//#define USE_POSE_CONFIRM
using SeldatMRMS.Management.RobotManagent;
using System;
using SeldatMRMS.Communication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading;

namespace SeldatUnilever_Ver1._02.Management.RobotManagent
{
    public class RobotComServer : RobotUnityControl
    {
        public class JStructDataString
        {
            public Int32 TypeData;
            public string Data;
        }
        public class JStructDataInt
        {
            public Int32 TypeData;
            public Int32 Data;
        }

        private enum Comres {
            RES_SUCCESS = 0,
            RES_ERROR
        }

        private enum str {
            TYPE_STR_LINEDETECTIONCTRL = 1,
            TYPE_STR_POSPALLET,
            TYPE_STR_CMDAREAPALLET,
            TYPE_STR_SET_GOAL
        }

        private enum rts {
            TYPE_RTS_FINISH_STATE = 1,
            TYPE_RTS_POSE_CHECKIN,
            TYPE_RTS_DISPLAY_INFO
        }

        public class PubTopic {
            public int pubServerSendToRb;
            public int pubServerResToRb;
        }
#if USE_POSE_CONFIRM
        private class PCheckIn
        {
            public PCheckIn(bool fNewPCI = false, Pose pointCheckInConfirm = null) {
                this.fNewPCI = fNewPCI;
                this.pointCheckInConfirm = pointCheckInConfirm;
            }
            public bool fNewPCI;
            public Pose pointCheckInConfirm;
        }

        PCheckIn pCI;
#endif
        public event Action<int> FinishStatesCallBack;
        private PubTopic serverPub;
        private Int32 preTypeSend;
        private bool waitRes;
        private const UInt32 TIME_OUT_WAT_RESPONSE = 5000;
        private const UInt32 NUM_TRY_SEND_TO_RB = 1000;
        private UInt32 numResendData;



        public RobotComServer() {
            this.waitRes = false;
            this.serverPub = new PubTopic();
            this.preTypeSend = 0;
            this.numResendData = 0;
#if USE_POSE_CONFIRM
            this.pCI = new PCheckIn();
#endif
        }
#if USE_POSE_CONFIRM
        public bool checkNewPci() {
            return pCI.fNewPCI;
        }
        public Pose getPointCheckInConfirm() {
            return pCI.pointCheckInConfirm;   
        }
#endif
        private void InitTopic() {
            serverPub.pubServerSendToRb = this.Advertise("/ServerSendToRb", "std_msgs/String");
            serverPub.pubServerResToRb = this.Advertise("/ServerResToRb", "std_msgs/String");
            int subRbSendToServer = this.Subscribe("/RbSendToServer", "std_msgs/String", RbSendToServerCallback, 10);
            int subRbResToServer = this.Subscribe("/RbResToServer", "std_msgs/String", RbResToServerCallback, 10);
        }

        private void ResStatus(Int32 typeData, Int32 Status) {
            JStructDataInt res = new JStructDataInt();
            res.TypeData = typeData;
            res.Data = Status;
            String jRes = JsonConvert.SerializeObject(res);
            ServerResToRb(jRes);
        }

        private void ServerPubToRb(String mes) {
            try
            {
                StandardString msg = new StandardString();
                msg.data = mes;
                this.Publish(serverPub.pubServerSendToRb, msg);
            }
            catch
            {
                Console.WriteLine("ServerSendToRb fail");
            }
        }

        private void ServerResToRb(String mes)
        {
            try
            {
                StandardString msg = new StandardString();
                msg.data = mes;
                this.Publish(serverPub.pubServerResToRb, msg);
            }
            catch
            {
                Console.WriteLine("ServerResToRb fail");
            }
        }

        public bool SendPoseStamped(Pose pose)
        {
            bool ret = false;
            try
            {
                if (pose != null)
                {
                    JStructDataString dt = new JStructDataString();

                    dt.TypeData = (Int32)str.TYPE_STR_SET_GOAL;
                    GeometryPoseStamped data = new GeometryPoseStamped();
                    data.header.frame_id = "map";
                    data.pose.position.x = (float)pose.Position.X;
                    data.pose.position.y = (float)pose.Position.Y;
                    data.pose.position.z = 0;
                    double theta = pose.AngleW;
                    data.pose.orientation.z = (float)Math.Sin(theta / 2);
                    data.pose.orientation.w = (float)Math.Cos(theta / 2);

                    robotLogOut.ShowText(this.properties.Label, "Send Pose => " + JsonConvert.SerializeObject(data).ToString());
                    dt.Data = JsonConvert.SerializeObject(data).ToString();
                    Console.WriteLine(dt.Data);
                    preTypeSend = dt.TypeData;

                    String jData = JsonConvert.SerializeObject(dt);

                    ret = this.SendToRb(jData);


                    // lưu vị trí đích đến
                    gx = data.pose.position.x;
                    gy = data.pose.position.y;
#if USE_POSE_CONFIRM
                    this.pCI.fNewPCI = false;
#endif
                    ret = true;
                }
                else
                {
                    Console.WriteLine("Without Data SendPoseStamped");
                    ret = false;
                }
            }
            catch
            {
                Console.WriteLine("Robot Control Error SendPoseStamped");
                ret = false;
            }
            return ret;
        }

        public bool SendCmdLineDetectionCtrl(RequestCommandLineDetect cmd)
        {
            bool ret = false;
            JStructDataInt data = new JStructDataInt();

            data.TypeData = (Int32)str.TYPE_STR_LINEDETECTIONCTRL;
            data.Data = (Int32)cmd;
            preTypeSend = data.TypeData;

            String jData = JsonConvert.SerializeObject(data);

            ret = this.SendToRb(jData);

            robotLogOut.ShowText(this.properties.Label, "SendCmdLineDetectionCtrl => " + jData);

            return ret;
        }

        public bool SendCmdPosPallet(RequestCommandPosPallet cmd)
        {
            bool ret = false;
            JStructDataInt data = new JStructDataInt();

            data.TypeData = (Int32)str.TYPE_STR_POSPALLET;
            data.Data = (Int32)cmd;
            preTypeSend = data.TypeData;

            String jData = JsonConvert.SerializeObject(data);

            ret = this.SendToRb(jData);

            robotLogOut.ShowText(this.properties.Label, "SendCmdPosPallet => " + jData);

            return ret;
        }

        public bool SendCmdAreaPallet(String cmd)
        {
            bool ret = false;
            JStructDataString data = new JStructDataString();

            data.TypeData = (Int32)str.TYPE_STR_CMDAREAPALLET;
            data.Data = cmd;
            preTypeSend = data.TypeData;

            String jData = JsonConvert.SerializeObject(data);

            ret = this.SendToRb(jData);

            robotLogOut.ShowText(this.properties.Label, "SendCmdAreaPallet => " + jData);

            return ret;
        }

        private bool SendToRb(String mes)
        {
            bool ret = true;
            this.ServerPubToRb(mes);
            this.waitRes = true;
            Stopwatch et = new Stopwatch();
            et.Start();
            while (this.waitRes) {
                if (et.ElapsedMilliseconds >= TIME_OUT_WAT_RESPONSE)
                {
                    et.Restart();
                    this.ServerPubToRb(mes);
                    this.numResendData++;
                    if (this.numResendData >= NUM_TRY_SEND_TO_RB)
                    {
                        ret = false;
                        this.waitRes = false;
                    }
                }
                else {
                    Thread.Sleep(20);
                }
            }
            return ret;
        }

        private void RbSendToServerCallback(SeldatMRMS.Communication.Message message)
        {
            /*json = {
                "TypeData": "1",
          "Data": "abcdxyz
            }"*/
            StandardString mesg = (StandardString)message;
            try
            {
                JObject jRet = JObject.Parse(mesg.data);
                rts typeData = (rts)(Int32)jRet["TypeData"];
                switch (typeData)
                {
                    case rts.TYPE_RTS_FINISH_STATE:
                        this.FinishedStatesHandler((Int32)jRet["Data"]);
                        this.ResStatus((Int32)rts.TYPE_RTS_FINISH_STATE, (Int32)Comres.RES_SUCCESS);
                        break;
                    case rts.TYPE_RTS_POSE_CHECKIN:
#if USE_POSE_CONFIRM
                        double x = (double)jRet["Data"]["x"];
                        double y = (double)jRet["Data"]["y"];
                        this.pCI.pointCheckInConfirm = new Pose(x, y, 0);
                        this.pCI.fNewPCI = true;
#endif
                        this.ResStatus((Int32)rts.TYPE_RTS_POSE_CHECKIN, (Int32)Comres.RES_SUCCESS);
                        break;
                    case rts.TYPE_RTS_DISPLAY_INFO:
                        String info = (String)jRet["Data"];
                        this.ResStatus((Int32)rts.TYPE_RTS_DISPLAY_INFO, (Int32)Comres.RES_SUCCESS);
                        break;
                    default:
                        break;
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private void RbResToServerCallback(SeldatMRMS.Communication.Message message)
        {
            /*json = {
                "TypeData": "1",
                "Data": "abcdxyz
            }"*/
            StandardString mesg = (StandardString)message;
            try
            {
                JObject jRet = JObject.Parse(mesg.data);
                JStructDataInt jData = new JStructDataInt();
                jData.TypeData = (Int32)jRet["TypeData"];
                jData.Data = (Int32)jRet["Data"];
                if (jData.Data == (Int32)Comres.RES_SUCCESS)
                {
                    if(preTypeSend == jData.TypeData)
                    {
                        this.waitRes = false;
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void FinishedStatesHandler(Int32 message)
        {
            try
            {
                Console.WriteLine("FinishedStatesHandler :{0}", message);
                robotLogOut.ShowText(this.properties.Label, "Finished State [" + message + "]");
                FinishStatesCallBack(message);
            }
            catch
            {
                Console.WriteLine(" Error FinishedStatesHandler");
            }
        }

        protected override void OnOpenedEvent() {
            this.InitTopic();
            base.OnOpenedEvent();
        }
    }       
}
