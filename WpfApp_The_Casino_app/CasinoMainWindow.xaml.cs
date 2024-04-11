using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp_The_Casino_app
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class CasinoMainWindow : Window
    {
        private readonly object rouletteLock = new object();
        private readonly Random random = new Random();
        private readonly List<Player> players = new List<Player>();
        private readonly List<PlayerResult> playerResults = new List<PlayerResult>();
        private int totalPlayers;
        public static CasinoMainWindow Instance { get; private set; }
        public CasinoMainWindow()
        {
            InitializeComponent();
            Instance = this;
            StartGame();
        }

        private void StartGame()
        {
            for (int i = 0; i < 5; i++)
            {
                var player = new Player(1000);
                players.Add(player);
                Task.Run(() => player.Play());
            }
        }

        public void SavePlayerResult(int startingMoney, int finalMoney)
        {
            lock (playerResults)
            {
                totalPlayers = random.Next(20, 101);
                for (int i = 0; i < totalPlayers; i++)
                {
                    var player = new Player(1000);
                    players.Add(player);
                    Task.Run(() => player.Play());
                }
            }
        }

        private void WriteReportFile()
        {
            string reportFilePath = "CasinoReport.txt";
            using (StreamWriter writer = new StreamWriter(reportFilePath))
            {
                foreach (var result in playerResults)
                {
                    writer.WriteLine($"Player: Starting Money: {result.StartingMoney}, Final Money: {result.FinalMoney}");
                }
            }
            UpdateUI($"Report file created: {reportFilePath}");
        }

        public class PlayerResult
        {
            public int StartingMoney { get; }
            public int FinalMoney { get; }

            public PlayerResult(int startingMoney, int finalMoney)
            {
                StartingMoney = startingMoney;
                FinalMoney = finalMoney;
            }
        }



        public int SpinRoulette()
        {
            lock (rouletteLock)
            {
                return random.Next(0, 36);
            }
        }

        public void UpdateUI(string message)
        {
            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText(message + "\n");
            });
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Clear();

            players.Clear();
            StartGame();
        }


    }
}
