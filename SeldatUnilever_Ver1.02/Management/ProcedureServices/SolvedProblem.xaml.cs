using Newtonsoft.Json;
using SeldatMRMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SeldatUnilever_Ver1._02.Management.ProcedureServices
{
    /// <summary>
    /// Interaction logic for SolvedProblem.xaml
    /// </summary>
    public partial class SolvedProblem : Window
    {
        Object objProc;
        public SolvedProblem(Object obj, string cultureName = null)
        {
            InitializeComponent();
            ApplyLanguage(cultureName);
            this.objProc = obj;
            ProcedureForkLiftToBuffer proFB = obj as ProcedureForkLiftToBuffer;
            ShowInformation(proFB);
        }

        public void ApplyLanguage(string cultureName = null)
        {
            if (cultureName != null)
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);

            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "vi-VN":
                    dict.Source = new Uri("..\\Lang\\Vietnamese.xaml", UriKind.Relative);
                    break;
                // ...
                default:
                    dict.Source = new Uri("..\\Lang\\English.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }

        public void ShowInformation(ProcedureForkLiftToBuffer proFB)
        {
            FlowDocument myFlowDoc = new FlowDocument();
            // Add paragraphs to the FlowDocument.
            myFlowDoc.Blocks.Add(new Paragraph(new Run("Robot"+proFB.robot.properties.NameID)));
            myFlowDoc.Blocks.Add(new Paragraph(new Run("RLabel" + proFB.robot.properties.Label)));
            myFlowDoc.Blocks.Add(new Paragraph(new Run("Procedure"+ proFB.procedureCode)));
            myFlowDoc.Blocks.Add(new Paragraph(new Run("Error code" + proFB.errorCode)));
            myFlowDoc.Blocks.Add(new Paragraph(new Run("Gate 1" + "192.168.1.2")));
            myFlowDoc.Blocks.Add(new Paragraph(new Run("Gate 2" + "192.168.1.2")));

            detailInfo.Document = myFlowDoc;
           
        }
        public void UpdateInformation(ProcedureForkLiftToBuffer proFB)
        {
            proFB.procedureDataItemsDB.GetParams(DBProcedureService.ProcessStatus.F.ToString());
            //proFB.robotTaskDB.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
            proFB.robot.properties.solvedProblemContent = "";
            proFB.robot.properties.problemContent= "";
            proFB.robot.properties.detailInfo= new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
            proFB.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proFB.robot.properties).ToString();
            MessageBox.Show(JsonConvert.SerializeObject(proFB.procedureDataItemsDB));
            proFB.SendHttpProcedureDataItem(proFB.procedureDataItemsDB);
            proFB.SendHttpRobotTaskItem(proFB.robotTaskDB);
        }

        private void cancelProcBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcedureForkLiftToBuffer proFB = this.objProc as ProcedureForkLiftToBuffer;
            UpdateInformation(proFB);
        }
    }
}
