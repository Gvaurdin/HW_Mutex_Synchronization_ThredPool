using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp_The_Casino_app
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class CasinoMainWindow : Window
    {
        static Dictionary<Label, Label> places = new Dictionary<Label, Label>()
        {

        };
        private const int max_playesrs = 5;
        private const int min_all_players = 20;
        private const int max_all_players = 100;
        private const int max_money = 1000;
        private static int count = 0;
        private static int countStuffNumber = 0;
        private static int numberCasino = 0;
        private static int round = 0;
        private readonly object lockObj;
        private static bool isRunGame = false;

        public event Action<bool> threwNumber;
        public event Action<string> NewEvent;
        public event Action NextRound;
        public event Action finishRound;
        public event Action<Player> playerGameOver;

        Thread[] playersThreads;
        private Random random;
        private int waitingPlayers;
        private static List<Player> players;
        private static int[] selectNumber;
        public CasinoMainWindow()
        {
            InitializeComponent();
            random = new Random();
            selectNumber = new int[5]; 
            playersThreads = new Thread[5];
            lockObj = new object();
            ControlsShow(false);
            players = new List<Player>();
            NewEvent += CasinoMainWindow_NewEvent;
            playerGameOver += AddNewPlayer;
            threwNumber += CheckBoolThrew;
            finishRound += CasinoMainWindow_finishRound;
            NextRound += Game;
        }

        private void CasinoMainWindow_finishRound()
        {
            numberCasino = selectNumber[random.Next(selectNumber.Length)];
            for (int i = 0; i < players.Count; i++) 
            {
                if (players[i].throwNumber == numberCasino) 
                {
                    NewEvent($"Player[{players[i].name}] is win {players[i].moneyStuff}.$");
                }
                else
                {
                    NewEvent($"Player[{players[i].name}] is lose {players[i].moneyStuff}.$");
                }
                Thread.Sleep(3000);
            }
            countStuffNumber = 0;
            NewEvent($"Round {round} is finish");
            NextRound();
        }

        private void CasinoMainWindow_NewEvent(string obj)
        {
            Dispatcher.Invoke(() =>
            {
                listboxLogs.Items.Add(obj);
            });
        }

        private void Game()
        {
            if (!isRunGame)
            {
                NewEvent("Game is start. Round 1");
                round++;
                isRunGame = true;
                Thread.Sleep(3000);
            }
            if (round == 1)
            {
                for (int i = 0; i < selectNumber.Length; i++)
                {
                    selectNumber[i] = random.Next(0, 10 + 1);
                }
                NewEvent("Players, what are your bets?");
                Thread.Sleep(3000);
                for (int i = 0; i < playersThreads.Length; i++)
                {
                    playersThreads[i].Name = $"Thread player {players[i].name}";
                    Thread.Sleep(3000);
                    playersThreads[i].Start(players[i]);
                }
            }
            else
            {
                NewEvent($"Round {round}");
                for (int i = 0; i < selectNumber.Length; i++)
                {
                    selectNumber[i] = random.Next(0, 10 + 1);
                }
                NewEvent("Players, what are your bets?");
                for (int i = 0; i < players.Count; i++)
                {
                    Play(players[i]);
                }
                round++;
            }


        }

        private void CheckBoolThrew(bool flag)
        {
            if (flag) countStuffNumber++;

            if (countStuffNumber == 5)
            {
                NewEvent("The players placed bets");
                finishRound();
            }
        }

        private void Play(object playerObj)
        {
            Player player = (Player)playerObj;
            if(player.Money <= 0)
            {
                playerGameOver(player);
            }

            lock (lockObj)
            {
                player.throwNumber = selectNumber[random.Next(selectNumber.Length)];
                player.moneyStuff = random.Next(50, player.Money / 2);
                NewEvent($"Player[{player.name}] threw number {player.throwNumber} with a bet {player.moneyStuff}");
                threwNumber(true);

            }

            Thread.Sleep(3000);
        }


        private void AddNewPlayer(Player player)
        {
            foreach (Player item in players)
            {
                if (item.name == player.name) players.Remove(item);
            }
            count++;
            Player tmp = new Player(count, random.Next(100, max_money));
            players.Add(tmp);


        }
        private void LogPlayerResult(Player player)
        {
            string log = $"Player[{player.name}] : [initial amount of money = {player.InitialMoney}] [end amount of money = {player.Money}]";

            Dispatcher.Invoke(() =>
            {

            });

            if (player.Money <= 0)
            {
                lock (lockObj)
                {
                    //players.Remove(player);
                }
            }

            //if (remainingPlayers == 0 && players.Count == 0)
            //{
            //    SaveReportToFile();
            //}
        }

        private void ControlsShow(bool flag)
        {
            if (!flag)
            {
                listboxLogs.Visibility = Visibility.Hidden;
            }
            else if (flag)
            {
                listboxLogs.Visibility = Visibility.Visible;
            }
        }

        private void buttonStart_Click_1(object sender, RoutedEventArgs e)
        {
            waitingPlayers = random.Next(min_all_players, max_all_players + 1);
            labelWaitingPlayers.Content = waitingPlayers.ToString();
            ControlsShow(true);

            for (int i = 0,count = 1; i < playersThreads.Length; i++, count++)
            {
                int initialMoney = random.Next(100, max_money);
                Player player = new Player(count, initialMoney);
                players.Add(player);
                NewEvent($"Player[{player.name}] sat down at the table");
                playersThreads[i] = new Thread(Play);
            }
            Game();
        }

        //private void SaveReportToFile()
        //{
        //    string filePath = "CasinoReport.txt";

        //    using (StreamWriter writer = new StreamWriter(filePath))
        //    {
        //        foreach (Player player in players)
        //        {
        //            writer.WriteLine($"{player.Name} {player.InitialMoney} {player.Money}");
        //        }
        //    }

        //    MessageBox.Show("Simulation finished. Report saved to CasinoReport.txt");
        //}


    }
    public class Player
    {
        public int name { get; set; }
        public int InitialMoney { get; }
        public int Money { get; set; }

        public int throwNumber { get; set; }

        public int moneyStuff { get; set; }

        public Player(int _name, int initialMoney)
        {
            InitialMoney = initialMoney;
            Money = initialMoney;
            name = _name;
        }
    }
}
