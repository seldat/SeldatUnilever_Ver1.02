﻿using SeldatMRMS.Management.DoorServices;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using SeldatUnilever_Ver1._02;
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
        private MainWindow mainWindow;
        public UnityManagementService(MainWindow mainWindow ) { this.mainWindow = mainWindow; }
        public void Initialize()
        {
            robotManagementService = new RobotManagementService(this.mainWindow.map);
            doorManagementService = new DoorManagementService();
            procedureManagementService = new ProcedureManagementService();
            chargerService = new ChargerManagementService();
            trafficService = new TrafficManagementService();
            deviceRegistrationService = new DeviceRegistrationService(12000);

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
