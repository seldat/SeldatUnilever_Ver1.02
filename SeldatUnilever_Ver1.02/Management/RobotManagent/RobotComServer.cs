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
            TYPE_STR_CMDAREAPALLET
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

        private PubTopic serverPub;
        private Int32 preTypeSend;
        private bool waitRes;
        private const UInt32 TIME_OUT_WAT_RESPONSE = 5000;
        private const UInt32 NUM_TRY_SEND_TO_RB = 1000;
        private UInt32 numResendData;
        private Pose pointCheckInConfirm;

        public RobotComServer() {
            this.waitRes = false;
            this.serverPub = new PubTopic();
            this.preTypeSend = 0;
            this.numResendData = 0;
        }

        public Pose getPointCheckInConfirm() {
            return pointCheckInConfirm;   
        }

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
                        double x = (double)jRet["Data"]["x"];
                        double y = (double)jRet["Data"]["y"];
                        this.pointCheckInConfirm = new Pose(x, y, 0);
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

        protected override void OnOpenedEvent() {
            this.InitTopic();
            base.OnOpenedEvent();
        }
    }       
}
