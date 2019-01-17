using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeldatMRMS.Management.RobotManagent;
using SeldatUnilever_Ver1._02.DTO;
using SelDatUnilever_Ver1._00.Management.ComSocket;

namespace SeldatUnilever_Ver1._02.Management.McuCom {
    public class McuCtrl : TranferData {
        private enum CmdCtrlMcu {
            CMD_TURNON_PC = 0x0B,
            RES_TURNOFF_PC, /*0x0C */
        }

        public McuCtrl (RobotUnity rb) : base (rb.properties.ipMcuCtrl, rb.properties.portMcuCtrl) {

        }

        public bool TurnOnPcRobot () {
            bool ret = false;
            byte[] dataSend = new byte[6];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte) CmdCtrlMcu.CMD_TURNON_PC;
            dataSend[3] = 0x04;
            dataSend[4] = 0x00;
            dataSend[5] = CalChecksum (dataSend, 3);
            ret = this.Tranfer (dataSend);
            return ret;
        }
    }
}