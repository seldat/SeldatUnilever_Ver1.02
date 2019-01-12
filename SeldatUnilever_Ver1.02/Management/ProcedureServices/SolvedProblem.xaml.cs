using Newtonsoft.Json;
using SeldatMRMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static SeldatMRMS.ProcedureControlServices;

namespace SeldatMRMS
{
    /// <summary>
    /// Interaction logic for SolvedProblem.xaml
    /// </summary>
    public partial class SolvedProblem : Window
    {
        Object objProc;
        public SolvedProblem()
        {
            InitializeComponent();


        }
        public void Registry(Object obj)
        {
            this.objProc = obj;
            if (obj.GetType() == typeof(ProcedureForkLiftToBuffer))
            {
                ProcedureForkLiftToBuffer proc = obj as ProcedureForkLiftToBuffer;
                ShowInformation(proc);
            }
            else if (obj.GetType() == typeof(ProcedureBufferToMachine))
            {
                ProcedureBufferToMachine proc = obj as ProcedureBufferToMachine;
                ShowInformation(proc);
            }
            else if (obj.GetType() == typeof(ProcedureMachineToReturn))
            {
                ProcedureMachineToReturn proc = obj as ProcedureMachineToReturn;
                ShowInformation(proc);
            }
            else if (obj.GetType() == typeof(ProcedureBufferToReturn))
            {
                ProcedureBufferToReturn proc = obj as ProcedureBufferToReturn;
                ShowInformation(proc);
            }
            else if (obj.GetType() == typeof(ProcedureRobotToReady))
            {
                ProcedureRobotToReady proc = obj as ProcedureRobotToReady;
                ShowInformation(proc);
            }
            else if (obj.GetType() == typeof(ProcedureRobotToCharger))
            {
                ProcedureRobotToCharger proc = obj as ProcedureRobotToCharger;
                ShowInformation(proc);
            }
        }
        public void ShowInformation(Object obj)
        {
            if (obj.GetType() == typeof(ProcedureForkLiftToBuffer))
            {
                var proc = obj as ProcedureForkLiftToBuffer;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Door]=====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.door.config))));
                detailInfo.Document = myFlowDoc;
            }
            else if (obj.GetType() == typeof(ProcedureBufferToMachine))
            {
                var proc = obj as ProcedureBufferToMachine;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                detailInfo.Document = myFlowDoc;
            }
            else if (obj.GetType() == typeof(ProcedureMachineToReturn))
            {
                var proc = obj as ProcedureMachineToReturn;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                detailInfo.Document = myFlowDoc;
            }
            else if (obj.GetType() == typeof(ProcedureBufferToReturn))
            {
                var proc = obj as ProcedureBufferToReturn;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                detailInfo.Document = myFlowDoc;
            }
            else if (obj.GetType() == typeof(ProcedureRobotToReady))
            {
                var proc = obj as ProcedureRobotToReady;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                detailInfo.Document = myFlowDoc;
            }
            else if (obj.GetType() == typeof(ProcedureRobotToCharger))
            {
                var proc = obj as ProcedureRobotToCharger;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Charger]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.chargerCtrl.cf))));
                detailInfo.Document = myFlowDoc;
            }
            else if (obj.GetType() == typeof(ProcedureReturnToGate))
            {
                var proc = obj as ProcedureForkLiftToBuffer;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Door]=====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.door.config))));
                detailInfo.Document = myFlowDoc;
            }

        }
        public void UpdateInformation(Object obj)
        {
            if (obj.GetType() == typeof(ProcedureForkLiftToBuffer))
            {
                var proc = obj as ProcedureForkLiftToBuffer;
                proc.procedureDataItemsDB.GetParams("F");
                proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robot.properties.problemContent = new TextRange(problemInfo.Document.ContentStart, problemInfo.Document.ContentEnd).Text;
                proc.robot.properties.solvedProblemContent = new TextRange(solvedProblemInfo.Document.ContentStart, solvedProblemInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
              
            }
            else if (obj.GetType() == typeof(ProcedureBufferToMachine))
            {
                var proc = obj as ProcedureBufferToMachine;
                proc.procedureDataItemsDB.GetParams("F");
                proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robot.properties.problemContent = new TextRange(problemInfo.Document.ContentStart, problemInfo.Document.ContentEnd).Text;
                proc.robot.properties.solvedProblemContent = new TextRange(solvedProblemInfo.Document.ContentStart, solvedProblemInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
             
            }
            else if (obj.GetType() == typeof(ProcedureMachineToReturn))
            {
                var proc = obj as ProcedureMachineToReturn;
                proc.procedureDataItemsDB.GetParams("F");
                proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robot.properties.problemContent = new TextRange(problemInfo.Document.ContentStart, problemInfo.Document.ContentEnd).Text;
                proc.robot.properties.solvedProblemContent = new TextRange(solvedProblemInfo.Document.ContentStart, solvedProblemInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
                
            }
            else if (obj.GetType() == typeof(ProcedureBufferToReturn))
            {
                var proc = obj as ProcedureBufferToReturn;
                proc.procedureDataItemsDB.GetParams("F");
                proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robot.properties.problemContent = new TextRange(problemInfo.Document.ContentStart, problemInfo.Document.ContentEnd).Text;
                proc.robot.properties.solvedProblemContent = new TextRange(solvedProblemInfo.Document.ContentStart, solvedProblemInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
                
            }
            else if (obj.GetType() == typeof(ProcedureRobotToReady))
            {
                var proc = obj as ProcedureRobotToReady;
                proc.readyChargerProcedureDB.Registry(proc.robotTaskDB.robotTaskId);
                proc.readyChargerProcedureDB.GetParams("F");
                proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robot.properties.problemContent = new TextRange(problemInfo.Document.ContentStart, problemInfo.Document.ContentEnd).Text;
                proc.robot.properties.solvedProblemContent = new TextRange(solvedProblemInfo.Document.ContentStart, solvedProblemInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                MessageBox.Show(JsonConvert.SerializeObject(proc.readyChargerProcedureDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpReadyChargerProcedureDB(proc.readyChargerProcedureDB);
            }
            else if (obj.GetType() == typeof(ProcedureRobotToCharger))
            {
                var proc = obj as ProcedureRobotToCharger;
                proc.readyChargerProcedureDB.Registry(proc.chargerCtrl, proc.robotTaskDB.robotTaskId);
                proc.readyChargerProcedureDB.GetParams("F");
                proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robot.properties.problemContent = new TextRange(problemInfo.Document.ContentStart, problemInfo.Document.ContentEnd).Text;
                proc.robot.properties.solvedProblemContent = new TextRange(solvedProblemInfo.Document.ContentStart, solvedProblemInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                MessageBox.Show(JsonConvert.SerializeObject(proc.readyChargerProcedureDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpReadyChargerProcedureDB(proc.readyChargerProcedureDB);

            }
        }

        private void cancelProcBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void destroyProcBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcedureForkLiftToBuffer proFB = objProc as ProcedureForkLiftToBuffer;
            UpdateInformation(proFB);
            proFB.selectHandleError = SelectHandleError.CASE_ERROR_EXIT;
        }

        private void contProcBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcedureForkLiftToBuffer proFB = objProc as ProcedureForkLiftToBuffer;
            UpdateInformation(proFB);
            proFB.selectHandleError = SelectHandleError.CASE_ERROR_CONTINUOUS;
        }
    }
}
