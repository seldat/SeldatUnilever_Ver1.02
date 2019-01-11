using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
using SelDatUnilever_Ver1._00.Communication;
using SelDatUnilever_Ver1._00.Communication.HttpServerRounter;

namespace SelDatUnilever_Ver1._00.Management.DeviceManagement
{
    public class DeviceRegistrationService : HttpServer
    {
        private List<DeviceItem> deviceItemList { get; set; }

        public DeviceRegistrationService(int port) : base(port)
        {
            deviceItemList = new List<DeviceItem>();
            CreateFolder();
        }
        public void RemoveDeviceItem(String userName)
        {
            if (deviceItemList.Count > 0)
            {
                deviceItemList.RemoveAt(deviceItemList.FindIndex(e => e.userName == userName));
            }
        }
        public void CreateFolder()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // Combine the base folder with your specific folder....
            string specificFolder = Path.Combine(folder, "SelDat\\DeviceItems");
            // CreateDirectory will check if folder exists and, if not, create it.
            // If folder exists then CreateDirectory will do nothing.
            Directory.CreateDirectory(specificFolder);
            if (!System.IO.Directory.Exists(specificFolder))
            {
                System.IO.Directory.CreateDirectory(specificFolder);
            }
        }
        public int HasDeviceItemAt(String userName)
        {
            return deviceItemList.FindIndex(e => e.userName.Equals(userName));
            //
        }
        public DeviceItem FindDeviceItem(String userName)
        {
            return deviceItemList.Find(e => e.userName == userName);
        }
        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            String data = inputData.ReadToEnd();
            JObject results = JObject.Parse(data);
            String userName = (String)results["userName"];
            if (HasDeviceItemAt(userName) >= 0)
            {
                FindDeviceItem(userName).ParseData(data);
            }
            else
            {
                DeviceItem deviceItem = new DeviceItem();
                deviceItem.userName = userName;
                deviceItem.ParseData(data);
                deviceItemList.Add(deviceItem);
            }
        }
        public List<DeviceItem> GetDeviceItemList()
        {
            return deviceItemList;
        }

    }
}