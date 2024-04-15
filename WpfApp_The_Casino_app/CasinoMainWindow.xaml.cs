using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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

        private const int max_playesrs = 5;
        private const int min_all_players = 5;
        private const int max_all_players = 5;
        private const int max_money = 1000;
        private static int count = 0;
        private static int countStuffNumber = 0;
        private static int numberCasino = 0;
        private static int round = 0;
        private readonly object lockObj;
        private static bool isRunGame = false;
        public event Action<string> NewEvent;
        List<Thread> playersThreads;
        private Random random;
        private int waitingPlayers;
        private static List<Player> players;
        private static int[] selectNumber;
        public CasinoMainWindow()
        {
            InitializeComponent();
            random = new Random();
            selectNumber = new int[5];
            playersThreads = new List<Thread>();
            lockObj = new object();
            players = new List<Player>();
            NewEvent += CasinoMainWindow_NewEvent;
        }


        private void CasinoMainWindow_finishRound()
        {
            numberCasino = selectNumber[random.Next(selectNumber.Length)];
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].throwNumber == numberCasino)
                {
                    NewEvent($"Player[{players[i].name}] is win {players[i].moneyStuff}.$");
                    if (players[i].moneyStuff > 200) players[i].Money += players[i].moneyStuff + 200;
                    else
                    {
                        players[i].Money += players[i].moneyStuff;
                    }
                    Thread.Sleep(100);
                }
                else
                {
                    NewEvent($"Player[{players[i].name}] is lose {players[i].moneyStuff}.$");
                    players[i].Money -= players[i].moneyStuff;
                    Thread.Sleep(100);
                }
            }
            CheckMoneyPlayers();
            NewEvent($"Round {round} is finish");
            if (waitingPlayers == 0 && players.Count < 2 && players.Count > 0)
            {
                EndGame();
            }
            else
            {
                round++;
                countStuffNumber = 0;
            }
        }

        private void CheckMoneyPlayers()
        {
            try
            {
                lock (lockObj)
                {
                    List<Player> removedPlayers = players.Where(p => p.Money < 100).ToList();
                    int countDeletePlayers = players.RemoveAll(p => p.Money < 100);
                    foreach (Player player in removedPlayers)
                    {
                        NewEvent($"Player[{player.name}] with balance {player.Money} was removed.");
                    }
                    if (countDeletePlayers > 0 && waitingPlayers > 0)
                    {
                        if (countDeletePlayers <= waitingPlayers)
                        {
                            waitingPlayers -= countDeletePlayers;
                            UpdateWaitingPlayers(waitingPlayers);
                            AddPlayers(countDeletePlayers);
                        }
                        else
                        {
                            countDeletePlayers = waitingPlayers;
                            waitingPlayers = 0;
                            UpdateWaitingPlayers(waitingPlayers);
                            AddPlayers(countDeletePlayers);
                        }
                    }
                    else if (players.Count == 0)
                    {
                        Player winner = removedPlayers.OrderByDescending(p => p.Money).FirstOrDefault();
                        NewEvent($"Winner player[{winner.name}]");
                        isRunGame = false;
                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error : {ex.Message}");
            }
        }


        private void CasinoMainWindow_NewEvent(string obj)
        {

            Dispatcher.Invoke(() =>
            {
                listboxLogs.Items.Add(obj);
            });
        }

        private void UpdateWaitingPlayers(int delPlayers)
        {
            Dispatcher.Invoke(() =>
            {
                labelWaitingPlayers.Content = delPlayers.ToString(); 
            });
        }

        private void Game()
        {
            try
            {
                NewEvent("Game is start. Round 1");
                Thread.Sleep(100);
                while (isRunGame)
                {
                    if (round == 0)
                    {
                        for (int i = 0; i < max_playesrs; i++)
                        {
                            count++;
                            int initialMoney = random.Next(500, max_money);
                            Player player = new Player(count, initialMoney);
                            players.Add(player);
                            NewEvent($"Player[{player.name}] sat down at the table");
                            Thread playerThread = new Thread(new ParameterizedThreadStart(Play));
                            playerThread.Name = $"Thread player {players[i].name}";
                            playersThreads.Add(playerThread);
                            Thread.Sleep(100);
                        }
                        for (int i = 0; i < selectNumber.Length; i++)
                        {
                            selectNumber[i] = random.Next(0, 10 + 1);
                        }
                        NewEvent("Players, what are your bets?");
                        PlayRound();
                    }
                    else
                    {
                        NewEvent($"Round {round}");
                        for (int i = 0; i < selectNumber.Length; i++)
                        {
                            selectNumber[i] = random.Next(0, 10 + 1);
                        }
                        NewEvent("Players, what are your bets?");
                        PlayRound();
                    }
                }
                NewEvent("Game over");
                SaveReportToFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void EndGame()
        {
            isRunGame = false;
            NewEvent($"Winner is player[{players[0].name}]");
        }

        private void PlayRound()
        {
            if(waitingPlayers == 0 && players.Count < 5 && players.Count != playersThreads.Count )
            {
                for(int i = players.Count - 1; i < playersThreads.Count;i++)
                {
                    playersThreads[i].Abort();
                    playersThreads.Remove(playersThreads[i]);
                }
            }
            for (int i = 0; i < playersThreads.Count; i++)
            {
                if (round == 0)
                {
                    playersThreads[i].Start(players[i]);
                    playersThreads[i].Join();
                }
                else
                {
                    playersThreads[i] = new Thread(new ParameterizedThreadStart(Play));
                    playersThreads[i].Name = $"Thread player {players[i].name}";
                    playersThreads[i].Start(players[i]);
                    playersThreads[i].Join();
                }
            }
            if(countStuffNumber == playersThreads.Count)
            CheckBoolThrew();

        }

        private void CheckBoolThrew()
        {

            lock (lockObj)
            { 
                if (countStuffNumber == playersThreads.Count)
                {
                    CasinoMainWindow_finishRound();
                }
            }

        }

        private void Play(object playerObj)
        {
            Player player = (Player)playerObj;
            try
            {
                lock (lockObj)
                {
                    player.throwNumber = selectNumber[random.Next(selectNumber.Length)];
                    player.moneyStuff = random.Next(50, player.Money + 1);
                    NewEvent($"Player[{player.name}] threw number {player.throwNumber} with a bet {player.moneyStuff}");
                    Thread.Sleep(100);
                    countStuffNumber++;

                }

            }
            catch (Exception ex)
            {

                MessageBox.Show($"Thread name {Thread.CurrentThread.Name}\n Max value : {player.Money}\n Player name {player.name}");

            }


        }


        private void AddPlayers(int countDeletePlayers)
        {
            lock (lockObj)
            {
                if(countDeletePlayers == 1)
                {
                    count++;
                    Player tmp = new Player(count, random.Next(500, max_money));
                    NewEvent($"Player[{tmp.name}] joins the game");
                    players.Add(tmp);
                }
                else
                {
                    for (int i = 0; i < countDeletePlayers; i++)
                    {
                        count++;
                        Player tmp = new Player(count, random.Next(500, max_money));
                        NewEvent($"Player[{tmp.name}] joins the game");
                        players.Add(tmp);
                    }
                }
            }
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

        private void ControlsShow(object flag)
        {
            bool isRun = (bool)flag;
            Dispatcher.Invoke(() =>
            {
                if (!isRun)
                {
                    listboxLogs.Visibility = Visibility.Hidden;
                }

                else if (isRun)
                {
                    listboxLogs.Visibility = Visibility.Visible;
                }
            });
        }

        private void buttonStart_Click_1(object sender, RoutedEventArgs e)
        {
            ControlsShow(true);
            isRunGame = true;
            waitingPlayers = random.Next(min_all_players, max_all_players);
            labelWaitingPlayers.Content = waitingPlayers.ToString();
            Thread threadMainGame = new Thread(new ThreadStart(Game));
            threadMainGame.Name = "Game Thread";
            threadMainGame.Start();
        }

        private void SaveReportToFile()
        {
            string filePath = "CasinoReport.txt";

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (string line in listboxLogs.Items)
                {
                    writer.WriteLine(line);
                }
            }

            MessageBox.Show("Simulation finished. Report saved to CasinoReport.txt");
        }


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
