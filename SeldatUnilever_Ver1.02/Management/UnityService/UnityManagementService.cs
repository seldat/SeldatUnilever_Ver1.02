using SeldatMRMS.Management.DoorServices;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using SelDatUnilever_Ver1._00.Management.ChargerCtrl;
using SelDatUnilever_Ver1._00.Management.DeviceManagement;
using SelDatUnilever_Ver1._00.Management.UnityService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeldatMRMS.Management.UnityService
{
    public class UnityManagementService
    {
        public RobotManagementService robotManagementService { get; set; }
        public DoorManagementService doorManagementService { get; set; }
        ProcedureManagementService procedureManagementService { get; set; }
        public TrafficManagementService trafficService { get; set; }
        AssigmentTaskService assigmentTaskService { get; set; }
        DeviceRegistrationService deviceRegistrationService { get; set; }
        public ChargerManagementService chargerService;
        public UnityManagementService() { }
        public void Initialize()
        {
            robotManagementService = new RobotManagementService();
            doorManagementService = new DoorManagementService();
            procedureManagementService = new ProcedureManagementService();
            chargerService = new ChargerManagementService();
            trafficService = new TrafficManagementService();
            deviceRegistrationService = new DeviceRegistrationService(8080);

            assigmentTaskService = new AssigmentTaskService();
            trafficService = new TrafficManagementService();
            assigmentTaskService.RegistryService(robotManagementService);
            assigmentTaskService.RegistryService(procedureManagementService);
            assigmentTaskService.RegistryService(deviceRegistrationService.GetDeviceItemList());
            assigmentTaskService.RegistryService(trafficService);
            procedureManagementService.RegistryService(trafficService);
            procedureManagementService.RegistryService(robotManagementService);
            procedureManagementService.RegistryService(doorManagementService);
            procedureManagementService.RegistryService(chargerService);
            procedureManagementService.RegistryService(deviceRegistrationService);
            deviceRegistrationService.listen();
            assigmentTaskService.Start();
        }
        public void Dispose()
        {

        }
    }
}
