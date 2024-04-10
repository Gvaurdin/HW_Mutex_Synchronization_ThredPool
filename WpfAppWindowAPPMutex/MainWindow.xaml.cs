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

namespace WpfAppWindowAPPMutex
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> nameThread;
        private NumberThreadWorker worker;
        public MainWindow()
        {
            InitializeComponent();
            listboxLogs.Visibility = Visibility.Hidden;
            nameThread = new List<string>()
            {
                "Generating numbers and writing them to file",
                "Searching  prime numbers and writing them to file",
                "Search for numbers with 7 at the end and write them to a file"
            };

            worker = new NumberThreadWorker();
            worker.WorkBegin += Worker_WorkBegin;
            worker.WorkEnd += Worker_WorkEnd;


        }

        private void Worker_WorkEnd(string obj)
        {
            Dispatcher.Invoke(() =>
            {
                listboxLogs.Items.Add(obj);
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
            listboxLogs.Visibility= Visibility.Visible;
            Thread[] threads = new Thread[nameThread.Count];
            for(int i = 0; i < threads.Length; i++) 
            {
                threads[i] = new Thread(worker.Start);
                threads[i].Name = nameThread[i];
                threads[i].Start(i);
            }
        }
    }
}
