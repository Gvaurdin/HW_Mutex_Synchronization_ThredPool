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

namespace WpfAppWindowAPPMutex
{
    /// <summary>
    /// Логика взаимодействия для WindowReport.xaml
    /// </summary>
    public partial class WindowReport : Window
    {
        NumberThreadWorker worker;
        public WindowReport()
        {
            InitializeComponent();
            NumberThreadWorker.CreateReport += Worker_CreateReport;
        }

        private void Worker_CreateReport(List<NumberThreadWorker.Report> obj)
        {
            Dispatcher.Invoke(() =>
            {
                FillControls(labelNumberGeneration, listboxNumberGeneration, obj[0]);
                FillControls(labelSearchPrime, listboxSearchPrime, obj[1]);
                FillControls(labelSearchEndSeven, listboxSearchEndSeven, obj[2]);
            });
        }

        private void FillControls(Label label, ListBox listBox, NumberThreadWorker.Report report)
        {
            label.Content = report.Name;
            listBox.Items.Add("Count numbers : " + report.countNumber);
            listBox.Items.Add("Size file = " + report.sizeFile + " bytes");
            foreach (string item in report.fileContent)
            {
                listBox.Items.Add(item);
            }
        }
    }
}
