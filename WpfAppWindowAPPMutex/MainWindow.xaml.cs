using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace WpfAppWindowAPPMutex
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> nameThread;
        private NumberThreadWorker worker;
        private List<NumberThreadWorker.Report> reports;
        public event Action<List<NumberThreadWorker.Report>> SendReport;

        //variables for task 3 , limited to running up to 3 copies
        private static Mutex mutex;
        private string[] guids = new string[3] { "{84079a08-eb1c-4045-941e-08a5f337d471}", "{84079a08-eb1c-4045-941e-08a5f337d472}", "{84079a08-eb1c-4045-941e-08a5f337d473}" };

    public MainWindow()
        {
            bool createdNew;
            for(int i = 0; i < 3; i++)
            {
                mutex = new Mutex(true, guids[i],out createdNew);
                if (!createdNew)
                {
                    if (i == 2)
                    {
                        System.Windows.Forms.MessageBox.Show("Уже запущено максимальное количество копий приложения.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                        return;
                    }

                }
                else break;
            }
            InitializeComponent();
            listboxLogs.Visibility = Visibility.Hidden;
            labelLogs.Visibility = Visibility.Hidden;
            nameThread = new List<string>()
            {
                "Thread : Generating numbers and writing them to file",
                "Thread : Searching  prime numbers and writing them to file",
                "Thread : Search for numbers with 7 at the end and write them to a file"
            };
            reports = new List<NumberThreadWorker.Report>();


        }

        private void Worker_WorkEnd(string obj)
        {
            Dispatcher.Invoke(() =>
            {
                listboxLogs.Items.Add(obj);
                if (obj == "End operation")
                {
                    DialogResult result = MessageBox.Show("Report is ready. Show to window with report?", "NumberThreadWorker", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        WindowReport windowReport = new WindowReport();
                        windowReport.Show();
                    }
                }
            });

        }

        private void Worker_WorkBegin(string obj)
        {

            Dispatcher.Invoke(() =>
            {
                listboxLogs.Items.Add(obj);
            });
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            buttonStart.IsEnabled = false;
            buttonStart.Visibility = Visibility.Hidden;
            listboxLogs.Visibility = Visibility.Visible;
            labelLogs.Visibility = Visibility.Visible;
            worker = new NumberThreadWorker();
            worker.WorkBegin += Worker_WorkBegin;
            worker.WorkEnd += Worker_WorkEnd;
            Thread[] threads = new Thread[nameThread.Count];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(worker.Start);
                threads[i].Name = nameThread[i];
                threads[i].Start(i);
            }
        }

    }
}
