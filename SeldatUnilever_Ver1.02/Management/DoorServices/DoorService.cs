using SelDatUnilever_Ver1._00.Management;
using SelDatUnilever_Ver1._00.Management.ComSocket;
using System;
using System.Diagnostics;
using System.Threading;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;

namespace DoorControllerService
{
    public class DoorService : TranferData
    {
        private enum CmdDoor
        {
            //CMD_GET_ID_DOOR = 0x61,
            //RES_GET_ID_DOOR, /*0x62 */
            //CMD_SET_ID_DOOR, /*0x63 */
            //RES_SET_ID_DOOR, /*0x64 */
            CMD_GET_STATUS_DOOR = 0x65, /*0x65 */
            RES_GET_STATUS_DOOR, /*0x66 */
            CMD_OPEN_DOOR, /*0x67 */
            RES_OPEN_DOOR, /*0x68 */
            CMD_CLOSE_DOOR, /*0x69 */
            RES_CLOSE_DOOR, /*0x6A */

            CMD_ON_LAMP, /*0x6B */
            RES_ON_LAMP, /*0x6C */
            CMD_OFF_LAMP, /*0x6D */
            RES_OFF_LAMP, /*0x6E */
        }
        public enum DoorId
        {
            DOOR_MEZZAMINE_UP = 0x01,
            DOOR_MEZZAMINE_RETURN, /* 0x02 */
            DOOR_ELEVATOR, /* 0x03 */
        }
        public enum DoorType
        {
            DOOR_FRONT = 0x01,
            DOOR_BACK, /* 0x02 */
        };
        public enum DoorStatus
        {
            DOOR_UNKNOW = 0x00,
            DOOR_CLOSE,
            DOOR_OPEN, /* 0x02 */
            DOOR_CLOSING, /* 0x03 */
            DOOR_OPENING, /* 0x04 */
            DOOR_ERROR, /* 0x05 */
            DOOR_START_CLOSE,
            DOOR_START_OPEN
        }

        public class DoorInfoConfig:NotifyUIBase
        {

            private String _Ip;
            public String Ip { get => _Ip; set { _Ip = value; RaisePropertyChanged("Ip"); } }
            private Int32 _Port;
            public Int32 Port { get => _Port; set { _Port = value; RaisePropertyChanged("Port"); } }
            private DoorId _Id;
            public DoorId Id { get => _Id; set { _Id = value; RaisePropertyChanged("Id"); } }
            public Pose PointCheckInGate;
            private String _PointCheckInGateStr;
            public String PointCheckInGateStr { get => _PointCheckInGateStr; set { _PointCheckInGateStr = value; RaisePropertyChanged("PointCheckInGateStr"); } }
            public Pose PointFrontLine;
            private String _PointFrontLineStr;
            public String PointFrontLineStr { get => _PointFrontLineStr; set { _PointFrontLineStr = value; RaisePropertyChanged("PointFrontLineStr"); } }

            private String _infoPallet;
            public String infoPallet { get => _infoPallet; set { _infoPallet = value; RaisePropertyChanged("infoPallet"); } }

            public void ParsePointCheckInGateValue(String value)
            {
                try
                {
                    double xx = double.Parse(value.Split(',')[0]);
                    double yy = double.Parse(value.Split(',')[1]);
                    double angle = double.Parse(value.Split(',')[2]);
                    PointCheckInGate = new Pose(xx, yy, angle);
                }
                catch { }
            }
            public void ParsePointFrontLineValue(String value)
            {
                try
                {
                    double xx = double.Parse(value.Split(',')[0]);
                    double yy = double.Parse(value.Split(',')[1]);
                    double angle = double.Parse(value.Split(',')[2]);
                    PointFrontLine = new Pose(xx, yy, angle);
                }
                catch { }
            }
        }

        public DoorInfoConfig config;
        public DoorService(DoorInfoConfig cf):base(cf.Ip,cf.Port)
        {
            config = cf;
            //SetId(cf.id);
        }

//        public bool GetId(ref DataReceive data)
//        {
//#if true
//            bool ret = true;
//#else
//            bool ret = false;
//            byte[] dataSend = new byte[6];

//            dataSend[0] = 0xFA;
//            dataSend[1] = 0x55;
//            dataSend[2] = (byte)CmdDoor.CMD_GET_ID_DOOR;
//            dataSend[3] = 0x04;
//            dataSend[4] = 0x00;
//            dataSend[5] = CalChecksum(dataSend,3);
//            ret = this.Tranfer(dataSend,ref data);
//#endif
//            return ret;
//        }
//        public bool SetId(DoorId id)
//        {
//#if true
//            bool ret = true;
//#else
//            bool ret = false;
//            byte[] dataSend = new byte[7];

//            dataSend[0] = 0xFA;
//            dataSend[1] = 0x55;
//            dataSend[2] = (byte)CmdDoor.CMD_SET_ID_DOOR;
//            dataSend[3] = 0x05;
//            dataSend[4] = 0x00;
//            dataSend[5] = (byte)id;
//            dataSend[6] = CalChecksum(dataSend,4);
//            ret = this.Tranfer(dataSend);
//#endif
//            return ret;
//        }
        public bool GetStatus(ref DataReceive data,DoorType id)
        {
#if true
            bool ret = true;
#else
            bool ret = false;
            byte[] dataSend = new byte[7];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdDoor.CMD_GET_STATUS_DOOR;
            dataSend[3] = 0x05;
            dataSend[4] = 0x00;
            dataSend[5] = (byte)id;
            dataSend[6] = CalChecksum(dataSend,4);
            ret = this.Tranfer(dataSend,ref data);
#endif
            return ret;
        }
        public bool Open(DoorType id)
        {
#if true
            bool ret = true;
#else
            bool ret = false;
            byte[] dataSend = new byte[7];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdDoor.CMD_OPEN_DOOR;
            dataSend[3] = 0x05;
            dataSend[4] = 0x00;
            dataSend[5] = (byte)id;
            dataSend[6] = CalChecksum(dataSend,4);
            ret = this.Tranfer(dataSend);
#endif
            return ret;
        }
        public bool Close(DoorType id)
        {
#if true
            bool ret = true;
#else
            bool ret = false;
            byte[] dataSend = new byte[7];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdDoor.CMD_CLOSE_DOOR;
            dataSend[3] = 0x05;
            dataSend[4] = 0x00;
            dataSend[5] = (byte)id;
            dataSend[6] = CalChecksum(dataSend,4);
            ret = this.Tranfer(dataSend);
#endif
            return ret;
        }

        public bool WaitOpen(DoorType id, UInt32 timeOut)
        {
            bool result = true;
#if false
            Stopwatch sw = new Stopwatch();
            DataReceive status = new DataReceive();
            //this.Open(id);
            sw.Start();
            do 
            {
                Thread.Sleep(100);
                if (sw.ElapsedMilliseconds > timeOut)
                {
                    result = false;
                    break;
                }
                this.GetStatus(ref status,id);
                if (status.data[0] == (byte)DoorStatus.DOOR_ERROR) {
                    result = false;
                    break;
                }
            } while (status.data[0] != (byte)DoorStatus.DOOR_OPEN);
            sw.Stop();
#endif
            return result;
        }

        public bool WaitClose(DoorType id, UInt32 timeOut)
        {
            bool result = true;
#if false
            Stopwatch sw = new Stopwatch();
            DataReceive status = new DataReceive();
            //this.Close(id);
            sw.Start();
            do 
            {
                Thread.Sleep(100);
                if (sw.ElapsedMilliseconds > timeOut)
                {
                    result = false;
                    break;
                }
                this.GetStatus(ref status,id);
                if (status.data[0] == (byte)DoorStatus.DOOR_ERROR)
                {
                    result = false;
                    break;
                }
            } while (status.data[0] != (byte)DoorStatus.DOOR_CLOSE);
            sw.Stop();
#endif
            return result;
        }

        public bool LampOn(DoorType id)
        {
            bool ret = false;
            byte[] dataSend = new byte[7];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdDoor.CMD_ON_LAMP;
            dataSend[3] = 0x05;
            dataSend[4] = 0x00;
            dataSend[5] = (byte)id;
            dataSend[6] = CalChecksum(dataSend,4);
            ret = this.Tranfer(dataSend);
            return ret;
        }

        public bool LampOff(DoorType id)
        {
            bool ret = false;
            byte[] dataSend = new byte[7];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdDoor.CMD_OFF_LAMP;
            dataSend[3] = 0x05;
            dataSend[4] = 0x00;
            dataSend[5] = (byte)id;
            dataSend[6] = CalChecksum(dataSend, 4);
            ret = this.Tranfer(dataSend);
            return ret;
        }
    }
}
